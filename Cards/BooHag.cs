﻿using APIPlugin;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameBooHag = "GrimoraMod_BooHag";

	private void Add_BooHag()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(SkinCrawler.ability)
			.SetBaseAttackAndHealth(1, 1)
			.SetBoneCost(5)
			.SetDescription("WHEN YOU KNOW SHE'S THERE, IT'S ALREADY TOO LATE.")
			.SetNames(NameBooHag, "Boo Hag")
			.Build()
		);
	}
}
