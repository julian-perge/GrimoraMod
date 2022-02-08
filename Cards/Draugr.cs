﻿using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameDraugr = "GrimoraMod_Draugr";

	private void Add_Draugr()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.IceCube)
			.SetBaseAttackAndHealth(0, 1)
			.SetBoneCost(1)
			.SetIceCube(NameSkeleton)
			.SetDescription("Hiding in a suit of armor, this skeleton won't last forever.")
			.SetNames(NameDraugr, "Draugr")
			.Build()
		);
	}
}
