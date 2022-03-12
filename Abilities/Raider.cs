﻿using APIPlugin;
using DiskCardGame;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class Raider : StrikeAdjacentSlots
{
	public static Ability ability;

	public override Ability Ability => ability;

	protected override Ability strikeAdjacentAbility => ability;

	public static NewAbility Create()
	{
		const string rulebookDescription = "[creature] will strike its adjacent slots, except other Raiders.";

		return ApiUtils.CreateAbility<Raider>(rulebookDescription);
	}
}
