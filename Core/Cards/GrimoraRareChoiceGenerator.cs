﻿using DiskCardGame;

namespace GrimoraMod;

public class GrimoraRareChoiceGenerator : CardChoiceGenerator
{

	public override List<CardChoice> GenerateChoices(CardChoicesNodeData data, int randomSeed)
	{
		return RandomUtils.GenerateRandomChoicesOfCategory(CardLoader.allData, randomSeed, CardMetaCategory.Rare);
	}
}
