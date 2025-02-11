﻿namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameMummy = $"{GUID}_Mummy";

	private void Add_Card_Mummy()
	{
		CardBuilder.Builder
			.SetAsNormalCard()
			.SetBaseAttackAndHealth(3, 3)
			.SetBoneCost(8)
			.SetDescription("THE CYCLE OF THE MUMMY LORD IS NEVER ENDING.")
			.SetNames(NameMummy, "Mummy Lord")
			.Build();
	}
}
