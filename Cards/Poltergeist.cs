using APIPlugin;
using DiskCardGame;

namespace GrimoraMod;

public partial class GrimoraPlugin
{
	public const string NamePoltergeist = "ara_Poltergeist";

	private void AddAra_Poltergeist()
	{
		NewCard.Add(CardBuilder.Builder
			.SetAsNormalCard()
			.SetAbilities(Ability.Flying, Ability.Submerge)
			.SetBaseAttackAndHealth(1, 1)
			.SetEnergyCost(3)
			.SetDescription("A skilled haunting ghost. Handle with caution.")
			.SetNames(NamePoltergeist, "Poltergeist")
			.Build()
		);
	}
}
