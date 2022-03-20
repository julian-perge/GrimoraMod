﻿using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace GrimoraMod;

public class LitFuse : ExplodeOnDeath
{
	public static Ability ability;

	public override Ability Ability => ability;

	public override bool RespondsToUpkeep(bool playerUpkeep)
	{
		return !Card.OpponentCard && playerUpkeep || Card.OpponentCard && !playerUpkeep;
	}

	public override IEnumerator OnUpkeep(bool playerUpkeep)
	{
		ViewManager.Instance.SwitchToView(View.Board, lockAfter: true);
		yield return new WaitForSeconds(0.25f);
		yield return Card.TakeDamage(1, null);
		yield return new WaitForSeconds(0.25f);
		ViewManager.Instance.SetViewUnlocked();
	}

	public static AbilityManager.FullAbility Create()
	{
		const string rulebookDescription =
			"[creature] loses 1 health per turn. "
			+ "When [creature] dies, the creature opposing it, as well as adjacent friendly creatures, are dealt 10 damage.";

		return ApiUtils.CreateAbility<LitFuse>(rulebookDescription);
	}
}
