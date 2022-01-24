using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NameDrownedSoul = "ara_DrownedSoul";

	private void AddAra_DrownedSoul()
	{
		List<Ability> abilities = new List<Ability>
		{
			Ability.Deathtouch,
			Ability.Submerge
		};

		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(abilities)
			.SetBaseAttackAndHealth(1, 1)
			.SetEnergyCost(5)
			.SetDescription("Going into that well wasn't the best idea...")
			.SetNames(NameDrownedSoul, "Drowned Soul")
			.Build()
		);
	}
}
