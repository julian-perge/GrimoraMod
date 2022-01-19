﻿using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameTombRobber = "ara_TombRobber";

	private void AddAra_TombRobber()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.ExplodeOnDeath)
			.SetBaseAttackAndHealth(0, 1)
			.SetDescription("Nothing... Nothing again... No treasure is left anymore.")
			.SetNames(NameTombRobber, "Tomb Robber")
			.Build()
		);
	}
}
