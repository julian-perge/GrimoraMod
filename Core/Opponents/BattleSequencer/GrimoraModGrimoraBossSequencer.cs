using System.Collections;
using DiskCardGame;
using Sirenix.Utilities;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class GrimoraModGrimoraBossSequencer : GrimoraModBossBattleSequencer
{
	private readonly RandomEx _rng = new();

	private bool hasPlayedArmyDialogue = false;

	private bool playedDialogueDeathTouch;

	private bool playedDialoguePossessive;

	public override Opponent.Type BossType => BaseBossExt.GrimoraOpponent;

	public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
	{
		return new EncounterData
		{
			opponentType = BossType
		};
	}

	public override IEnumerator GameEnd(bool playerWon)
	{
		if (playerWon)
		{
			if (!DialogueEventsData.EventIsPlayed("FinaleGrimoraBattleWon"))
			{
				Log.LogDebug($"FinaleGrimoraBattleWon has not played yet, playing now.");

				ViewManager.Instance.Controller.LockState = ViewLockState.Locked;
				yield return new WaitForSeconds(0.5f);
				yield return TextDisplayer.Instance.PlayDialogueEvent(
					"FinaleGrimoraBattleWon", TextDisplayer.MessageAdvanceMode.Input
				);
			}

			if (!ConfigHelper.Instance.isEndlessModeEnabled)
			{
				yield return new WaitForSeconds(0.5f);
				Log.LogInfo($"Player won against Grimora! Resetting run...");
				ConfigHelper.Instance.ResetRun();
			}
		}
		else
		{
			yield return base.GameEnd(false);
		}
	}

	public override IEnumerator OpponentUpkeep()
	{
		if (!playedDialogueDeathTouch &&
		    BoardManager.Instance.GetSlots(true).Exists(x => x.CardHasAbility(Ability.Deathtouch))
		    && BoardManager.Instance.GetSlots(false)
			    .Exists(slot => slot.CardHasSpecialAbility(GrimoraGiant.NewSpecialAbility.specialTriggeredAbility))
		   )
		{
			yield return new WaitForSeconds(0.5f);
			yield return TextDisplayer.Instance.ShowUntilInput(
				"DEATH TOUCH WON'T HELP YOU HERE DEAR." +
				"\nI MADE THESE GIANTS SPECIAL, IMMUNE TO QUITE A FEW DIFFERENT TRICKS!"
			);
			playedDialogueDeathTouch = true;
		}
		else if (!playedDialoguePossessive
		         && BoardManager.Instance.GetSlots(true).Exists(x => x.CardHasAbility(Possessive.ability))
		         && BoardManager.Instance.GetSlots(false).Exists(slot => slot.CardInSlotIs(NameBonelord))
		        )
		{
			yield return new WaitForSeconds(0.5f);
			yield return TextDisplayer.Instance.ShowUntilInput("THE BONE LORD CANNOT BE POSSESSED!");
			playedDialoguePossessive = true;
		}
	}

	public override IEnumerator PlayerUpkeep()
	{
		if (!DialogueEventsData.EventIsPlayed("FinaleGrimoraBattleStart"))
		{
			yield return new WaitForSeconds(0.5f);
			yield return TextDisplayer.Instance.PlayDialogueEvent(
				"FinaleGrimoraBattleStart", TextDisplayer.MessageAdvanceMode.Input
			);
		}
	}

	public override bool RespondsToOtherCardDie(
		PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer
	)
	{
		return !card.OpponentCard && (TurnManager.Instance.Opponent.NumLives == 3 || card.InfoName() == NameGiant);
	}
	private bool GrimoraInscrybeCard = false;
	public override IEnumerator OnOtherCardDie(
		PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer
	)
	{

		List<CardSlot> remainingGiant = BoardManager.Instance.OpponentSlotsCopy
			.Where(slot => slot.Card is not null && card.Slot != slot && slot.Card.InfoName() == NameGiant)
			.ToList();
		List<CardSlot> opponentQueuedSlots = BoardManager.Instance.GetQueueSlots();
		if (card.InfoName() == NameGiant && remainingGiant.Count == 1)
		{
			yield return TextDisplayer.Instance.ShowUntilInput(
				$"Oh dear, you've made {card.InfoName().Red()} quite angry."
				);
			ViewManager.Instance.SwitchToView(View.Board);
			card.Anim.StrongNegationEffect();
			card.AddTemporaryMod(new CardModificationInfo { attackAdjustment = 1});
		}
		else if (opponentQueuedSlots.IsNotEmpty())

		bool GrimoraInscrybeCard = false;

		if (GrimoraInscrybeCard == true)

		{
			List<CardSlot> opponentQueuedSlots = BoardManager.Instance.GetQueueSlots();
			if (opponentQueuedSlots.IsNotEmpty())
			{
				ViewManager.Instance.SwitchToView(View.BossCloseup);
				yield return TextDisplayer.Instance.PlayDialogueEvent(
					"GrimoraBossReanimate1",
					TextDisplayer.MessageAdvanceMode.Input
				);

				CardSlot slot = opponentQueuedSlots[UnityEngine.Random.Range(0, opponentQueuedSlots.Count)];
				yield return TurnManager.Instance.Opponent.QueueCard(card.Info, slot);
				yield return GrimoraInscrybeCard == false;
				yield return new WaitForSeconds(0.5f);
			}

		}
		else
		{
			yield return GrimoraInscrybeCard == true;
		}
	}
}
