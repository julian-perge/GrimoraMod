﻿using APIPlugin;
using DiskCardGame;
using UnityEngine;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameRevenant = "GrimoraMod_Revenant";

	private void Add_Revenant()
	{
		Sprite ogSprite = "Revenant".GetCardInfo().portraitTex; 
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.Brittle)
			.SetBaseAttackAndHealth(3, 1)
			.SetBoneCost(3)
			.SetDescription("THE REVENANT, BRINGING THE SCYTHE OF DEATH.")
			.SetNames(NameRevenant, "Revenant", ogSprite)
			.Build()
		);
	}
}
