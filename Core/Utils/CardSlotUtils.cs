﻿using DiskCardGame;

namespace GrimoraMod;

public class CardSlotUtils
{
	public static List<CardSlot> GetPlayerSlotsWithCards()
	{
		return BoardManager.Instance.PlayerSlotsCopy.FindAll(slot => slot.Card != null);
	}

	public static List<CardSlot> GetOpponentSlotsWithCards()
	{
		return BoardManager.Instance.OpponentSlotsCopy.FindAll(slot => slot.Card != null);
	}
}