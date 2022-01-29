﻿using APIPlugin;
using DiskCardGame;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameScreamingSkull = "ara_ScreamingSkull";

	private void AddAra_ScreamingSkull()
	{

		CardInfo info = ScriptableObject.CreateInstance<CardInfo>();
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(AreaOfEffectStrike.ability)
			.SetBaseAttackAndHealth(1, 1)
			.SetBoneCost(8)
			// .SetDescription("Going into that well wasn't the best idea...")
			.SetNames(NameScreamingSkull, "Screaming Skull")
			.Build()
		);
	}
}
