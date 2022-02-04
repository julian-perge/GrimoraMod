﻿using System.Collections;
using DiskCardGame;
using UnityEngine;
using static GrimoraMod.GrimoraPlugin;

namespace GrimoraMod;

public abstract class BaseBossExt : Part1BossOpponent
{
	public const string PrefabPathMasks = "Prefabs/Opponents/Leshy/Masks";
	public const string PrefabPathRoyalBossSkull = "Prefabs/Opponents/Grimora/RoyalBossSkull";

	private protected GameObject RoyalBossSkull => RightWrist.transform.GetChild(6).gameObject;
	public GameObject RightWrist { get; } = GameObject.Find("Grimora_RightWrist");

	public const Type KayceeOpponent = (Type)1001;
	public const Type SawyerOpponent = (Type)1002;
	public const Type RoyalOpponent = (Type)1003;
	public const Type GrimoraOpponent = (Type)1004;

	public static readonly Dictionary<Type, string> BossMasksByType = new()
	{
		{ SawyerOpponent, $"{PrefabPathMasks}/MaskTrader" },
		{ KayceeOpponent, $"{PrefabPathMasks}/MaskWoodcarver" },
		// { RoyalOpponent, PrefabPathRoyalBossSkull }
	};


	public abstract StoryEvent EventForDefeat { get; }

	public abstract Type Opponent { get; }

	protected internal GameObject Mask { get; set; }

	public override IEnumerator IntroSequence(EncounterData encounter)
	{
		// Log.LogDebug($"[{GetType()}] Calling IntroSequence");
		yield return base.IntroSequence(encounter);

		// Royal boss has a specific sequence to follow so that it flows easier
		if (BossMasksByType.TryGetValue(OpponentType, out string prefabPath))
		{
			yield return ShowBossSkull();

			// Log.LogDebug($"[{GetType()}] Creating mask [{prefabPath}]");
			Mask = (GameObject)Instantiate(
				Resources.Load(prefabPath),
				RightWrist.transform
			);

			Mask.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
			Mask.transform.localPosition = new Vector3(0.02f, 0.18f, 0.07f);
			Mask.transform.localRotation = Quaternion.Euler(0, 0, 270);

			// UnityEngine.Object.Destroy(RoyalBossSkull);
			RoyalBossSkull.SetActive(false);
			yield return new WaitForSeconds(1f);

			AudioController.Instance.FadeOutLoop(0.75f);
			RunState.CurrentMapRegion.FadeOutAmbientAudio();
		}
	}

	public override IEnumerator OutroSequence(bool wasDefeated)
	{
		if (wasDefeated)
		{
			ConfigHelper.Instance.SetBossDefeatedInConfig(this);

			Log.LogDebug($"[{GetType()}] SaveFile is Grimora");

			if (Mask is not null)
			{
				Log.LogDebug($"[{GetType()}] Glitching mask");
				GlitchOutAssetEffect.GlitchModel(
					Mask.transform,
					true
				);
			}

			Log.LogDebug($"[{GetType()}] audio queue");
			AudioController.Instance.PlaySound2D("glitch_error", MixerGroup.TableObjectsSFX);

			Log.LogDebug($"[{GetType()}] hiding skull");
			GrimoraAnimationController.Instance.SetHeadTrigger("hide_skull");

			Log.LogDebug($"[{GetType()}] Destroying scenery");
			DestroyScenery();

			Log.LogDebug($"[{GetType()}] Set Scene Effects");
			SetSceneEffectsShown(false);

			Log.LogDebug($"[{GetType()}] Stopping audio");
			AudioController.Instance.StopAllLoops();

			yield return new WaitForSeconds(0.75f);

			Log.LogDebug($"[{GetType()}] CleanUpBossBehaviours");
			CleanUpBossBehaviours();

			ViewManager.Instance.SwitchToView(View.Default, false, true);

			Log.LogDebug($"[{GetType()}] Resetting table colors");
			TableVisualEffectsManager.Instance.ResetTableColors();
			yield return new WaitForSeconds(0.25f);

			Log.LogDebug($"Setting post battle special node to a rare code node data");
			TurnManager.Instance.PostBattleSpecialNode = new ChooseRareCardNodeData();
		}
		else
		{
			yield return base.OutroSequence(false);
		}
	}

	public static IEnumerator ShowBossSkull()
	{
		// Log.LogDebug($"[{GetType()}] Calling ShowBossSkull");
		GrimoraAnimationController.Instance.ShowBossSkull();

		// Log.LogDebug($"[{GetType()}] Setting Head Trigger");
		GrimoraAnimationController.Instance.SetHeadTrigger("show_skull");

		yield return new WaitForSeconds(0.25f);

		ViewManager.Instance.SwitchToView(View.BossCloseup, immediate: false, lockAfter: true);
	}

	public virtual IEnumerator ReplaceBlueprintCustom(EncounterBlueprintData blueprintData)
	{
		Blueprint = blueprintData;
		List<List<CardInfo>> plan = EncounterBuilder.BuildOpponentTurnPlan(Blueprint, 0, false);
		ReplaceAndAppendTurnPlan(plan);
		yield return QueueNewCards();
	}
}
