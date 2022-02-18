using System.Collections;
using DiskCardGame;
using UnityEngine;
using static GrimoraMod.BlueprintUtils;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public class SawyerBossOpponent : BaseBossExt
{
	public override StoryEvent EventForDefeat => StoryEvent.FactoryCuckooClockAppeared;

	public override Type Opponent => SawyerOpponent;

	public override string SpecialEncounterId => "SawyerBoss";

	public override string DefeatedPlayerDialogue => "My dogs will enjoy your bones!";

	public override IEnumerator IntroSequence(EncounterData encounter)
	{
		AudioController.Instance.SetLoopAndPlay("gbc_battle_undead", 1);
		SpawnScenery("CratesTableEffects");
		yield return new WaitForSeconds(0.1f);

		ViewManager.Instance.SwitchToView(View.Default);

		SetSceneEffectsShownSawyer();

		yield return base.IntroSequence(encounter);
		yield return new WaitForSeconds(0.1f);

		yield return FaceZoomSequence();
		yield return TextDisplayer.Instance.ShowUntilInput(
			"Look away, Look away! If you want to fight, get it over quick!",
			-0.65f,
			0.4f
		);

		ViewManager.Instance.SwitchToView(View.Default);
		ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;
	}

	private static void SetSceneEffectsShownSawyer()
	{
		TableVisualEffectsManager.Instance.ChangeTableColors(
			GameColors.Instance.darkGold,
			GameColors.Instance.orange,
			GameColors.Instance.yellow,
			GameColors.Instance.yellow,
			GameColors.Instance.orange,
			GameColors.Instance.yellow,
			GameColors.Instance.brown,
			GameColors.Instance.orange,
			GameColors.Instance.brown
		);
	}

	public override IEnumerator StartNewPhaseSequence()
	{
		{
			InstantiateBossBehaviour<SawyerBehaviour>();

			yield return FaceZoomSequence();
			yield return TextDisplayer.Instance.ShowUntilInput(
				"Please, I don't want to fight anymore! Get it over with!",
				-0.65f,
				0.4f
			);
			yield return ClearQueue();
			yield return ClearBoard();

			yield return ReplaceBlueprintCustom(BuildNewPhaseBlueprint());
		}
	}

	public EncounterBlueprintData BuildNewPhaseBlueprint()
	{
		var blueprint = ScriptableObject.CreateInstance<EncounterBlueprintData>();
		blueprint.turns = new List<List<EncounterBlueprintData.CardBlueprint>>
		{
			new() { bp_Bonehound, bp_Bonehound },
			new(),
			new(),
			new(),
			new() { bp_Bonehound, bp_Draugr, bp_Draugr },
			new(),
			new(),
			new(),
			new() { bp_Bonehound, bp_Draugr, bp_Draugr },
			new(),
			new(),
			new() { bp_Bonehound },
		};

		return blueprint;
	}

	public override IEnumerator OutroSequence(bool wasDefeated)
	{
		if (wasDefeated)
		{
			yield return TextDisplayer.Instance.ShowUntilInput(
				"Thanks for getting it over with, and don't ever return!",
				-0.65f,
				0.4f
			);

			yield return new WaitForSeconds(0.5f);
			yield return base.OutroSequence(true);

			yield return FaceZoomSequence();
			yield return TextDisplayer.Instance.ShowUntilInput(
				"The next area won't be so easy. I asked Royal to do his best at making it impossible.",
				-0.65f,
				0.4f);
		}
		else
		{
			Log.LogDebug($"[{GetType()}] Defeated player dialogue");
			yield return TextDisplayer.Instance.ShowUntilInput(
				DefeatedPlayerDialogue,
				-0.65f,
				0.4f
			);
		}
	}
}
