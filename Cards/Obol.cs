﻿using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameObol = "ara_Obol";

	private void AddAra_Obol()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.Reach, Ability.Sharp)
			.SetBaseAttackAndHealth(0, 3)
			.SetBoneCost(3)
			.SetDescription("Going into that well wasn't the best idea...")
			.SetNames(NameObol, "Ancient Obol")
			.Build()
		);
	}
}
