﻿using System.Collections;
using DiskCardGame;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class GrimoraModRoyalBossSequencer : GrimoraModBossBattleSequencer
{
	private readonly RandomEx _rng = new();

	public override Opponent.Type BossType => BaseBossExt.RoyalOpponent;

	public override EncounterData BuildCustomEncounter(CardBattleNodeData nodeData)
	{
		return new EncounterData()
		{
			opponentType = BossType
		};
	}

	public override bool RespondsToUpkeep(bool playerUpkeep)
	{
		return playerUpkeep;
	}

	public override IEnumerator OnUpkeep(bool playerUpkeep)
	{
		var activePlayerCards = BoardManager.Instance.GetPlayerCards();
		if (activePlayerCards.IsNotEmpty() && _rng.NextBoolean())
		{
			var playableCard = activePlayerCards[UnityEngine.Random.Range(0, activePlayerCards.Count)];
			ViewManager.Instance.SwitchToView(View.Board);
			yield return new WaitForSeconds(0.25f);
			yield return TextDisplayer.Instance.ShowUntilInput(
				$"YARRRR, I WILL ENJOY THE KABOOM OF {playableCard.Info.displayedName.BrightRed()}",
				1f,
				0.5f,
				Emotion.Anger
			);
			if (!playableCard.TemporaryMods.Exists(mod => mod.abilities.Contains(LitFuse.ability)))
			{
				playableCard.AddTemporaryMod(new CardModificationInfo(LitFuse.ability));
			}
		}
	}
}
