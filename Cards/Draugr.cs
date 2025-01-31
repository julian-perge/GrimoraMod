﻿using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameDraugr = $"{GUID}_Draugr";

	private void Add_Card_Draugr()
	{
		CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.IceCube)
			.SetBaseAttackAndHealth(0, 1)
			.SetBoneCost(1)
			.SetDescription("HIDING IN A SUIT OF ARMOR, OR ICE, WHAT DOES IT MATTER. THIS SKELETON WON'T LAST FOREVER.")
			.SetIceCube(NameSkeleton)
			.SetNames(NameDraugr, "Draugr")
			.Build();
	}
}
