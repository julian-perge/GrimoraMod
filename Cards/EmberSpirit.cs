using APIPlugin;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameEmberSpirit = "GrimoraMod_Ember_Spirit";

	private void Add_EmberSpirit()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsRareCard()
			.SetAbilities(FlameStrafe.ability)
			.SetBaseAttackAndHealth(1, 3)
			.SetDescription("A TRICKSTER SPIRIT FLEEING AND LEAVING BEHIND ITS FLAMES.")
			.SetEnergyCost(6)
			.SetNames(NameEmberSpirit, "Spirit of Ember")
			.Build()
		);
	}
}
