﻿using APIPlugin;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameMummy = "ara_Mummy";

	private void AddAra_Mummy()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetBaseAttackAndHealth(3, 3)
			.SetBoneCost(8)
			.SetDescription("The cycle of the Mummy Lord is never ending.")
			.SetNames(NameMummy, "Mummy Lord")
			.Build()
		);
	}
}
