﻿using System.Collections;
using DiskCardGame;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class GrimoraModRoyalBossSequencer : GrimoraModBossBattleSequencer
{
	private readonly RandomEx _rng = new();

	private GameObject GameTable => GameObject.Find("GameTable");

	private const float DurationTableSway = 3.5f;

	public int boardSwayCounter = 0;
	public bool boardSwayedLeftLast = false;

	public override Opponent.Type BossType => BaseBossExt.RoyalOpponent;

	private void PlayTableSway()
	{
		GameTable
			.GetComponent<Animator>()
			.Play(
				!boardSwayedLeftLast
					? "sway_left"
					: "sway_right",
				0,
				0f
			);

		boardSwayedLeftLast = !boardSwayedLeftLast;
	}

	public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
	{
		return new EncounterData()
		{
			opponentType = BossType
		};
	}

	public override IEnumerator OpponentCombatEnd()
	{
		var activePlayerCards = BoardManager.Instance.GetPlayerCards(pCard => !pCard.FaceDown);
		if (activePlayerCards.IsNotEmpty() && _rng.NextBoolean())
		{
			var playableCard = activePlayerCards[UnityEngine.Random.Range(0, activePlayerCards.Count)];
			ViewManager.Instance.SwitchToView(View.Board);
			yield return new WaitForSeconds(0.25f);
			yield return TextDisplayer.Instance.ShowUntilInput(
				$"YARRRR, I WILL ENJOY THE KABOOM OF {playableCard.Info.displayedName.BrightRed()}"
			);
			playableCard.AddTemporaryMod(new CardModificationInfo(LitFuse.ability));
		}

		if (++boardSwayCounter >= 2 && activePlayerCards.IsNotEmpty())
		{
			boardSwayCounter = 0;
			yield return TextDisplayer.Instance.ShowUntilInput(
				$"Seven seas for the table!"
			);

			ViewManager.Instance.SwitchToView(View.Default, lockAfter: true);
			// z axis for table movement

			PlayTableSway();

			var allCardsOnBoard = BoardManager.Instance.AllSlotsCopy
				.Where(slot => !slot.CardHasAbility(SeaLegs.ability))
				.Select(slot => slot.Card)
				.ToList();

			if (boardSwayedLeftLast)
			{
				// the reason for this is so that the cards are executed right to left and not left to right.
				// if they're executed left to right like how swaying left will is,
				//	it will kill cards that have cards next to them 
				allCardsOnBoard.Reverse();
			}

			foreach (var playableCard in allCardsOnBoard)
			{
				yield return StartCoroutine(DoStrafe(playableCard, boardSwayedLeftLast));
			}

			ViewManager.Instance.SetViewUnlocked();
		}
	}


	protected virtual IEnumerator DoStrafe(PlayableCard playableCard, bool movingLeft)
	{
		Log.LogInfo($"[DoStrafe] starting strafe for card {playableCard.GetNameAndSlot()}");
		CardSlot toLeftSlot = BoardManager.Instance.GetAdjacent(playableCard.Slot, true);
		CardSlot toRightSlot = BoardManager.Instance.GetAdjacent(playableCard.Slot, false);
		bool canMoveLeft = toLeftSlot != null && toLeftSlot.Card == null;
		bool canMoveRight = toRightSlot != null && toRightSlot.Card == null;

		CardSlot destination = movingLeft
			? toLeftSlot
			: toRightSlot;
		bool destinationValid = destination is not null
		                        && (movingLeft
			                        ? canMoveLeft
			                        : canMoveRight);
		Log.LogInfo(
			$"[DoStrafe] {playableCard.GetNameAndSlot()} Destination [{destination?.name}] DestValid [{destinationValid}]"
		);
		yield return MoveToSlot(playableCard, destination, destinationValid, movingLeft);
	}

	private IEnumerator MoveToSlot(
		PlayableCard playableCard,
		CardSlot destination,
		bool destinationValid,
		bool movingLeft
	)
	{
		if (playableCard.HasAnyAbilities(Ability.Strafe, Ability.StrafePush))
		{
			playableCard.RenderInfo.SetAbilityFlipped(
				playableCard.Info.abilities.Find(ab => ab is Ability.Strafe or Ability.StrafePush),
				movingLeft
			);
			playableCard.RenderInfo.flippedPortrait = movingLeft && playableCard.Info.flipPortraitForStrafe;
			playableCard.RenderCard();
		}

		Vector3 positionCopy = playableCard.transform.localPosition;
		if (destination != null && destinationValid)
		{
			Log.LogInfo(
				$"[MoveToSlot] Card {playableCard.GetNameAndSlot()} will be moved to slot [{destination.name}]!"
			);

			yield return BoardManager.Instance.AssignCardToSlot(playableCard, destination, DurationTableSway + 2);
			Tween.LocalRotation(
				playableCard.transform,
				new Vector3(90, 0, 0),
				0,
				0,
				Tween.EaseIn
			);
			yield return new WaitForSeconds(0.25f);
		}
		else
		{
			if (!playableCard.InOpponentQueue)
			{
				float leftOrRightX = movingLeft
					? positionCopy.x - 6
					: positionCopy.x + 6;
				Log.LogInfo($"[MoveToSlot] Card {playableCard.GetNameAndSlot()} is about to fucking die me hearty!");
				TweenBase slidingCard = Tween.LocalPosition(
					playableCard.transform,
					new Vector3(leftOrRightX, positionCopy.y, positionCopy.z),
					DurationTableSway,
					0,
					Tween.EaseIn
				);
				yield return new WaitForSeconds(0.15f);
				yield return new WaitUntil(() => slidingCard.Status == Tween.TweenStatus.Finished);
				yield return playableCard.Die(false);
			}
		}
	}
}
