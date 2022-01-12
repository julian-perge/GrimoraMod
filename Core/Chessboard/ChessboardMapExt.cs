﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiskCardGame;
using Unity.Cloud.UserReporting.Plugin.SimpleJson;
using UnityEngine;
using Random = UnityEngine.Random;
using Resources = GrimoraMod.Properties.Resources;

// This class is literally just meant to be able to view the deck review sequencer
namespace GrimoraMod
{
	public class ChessboardMapExt : ChessboardMap
	{
		public new static ChessboardMapExt Instance => GameMap.Instance as ChessboardMapExt;

		public string PiecesDelimited => string.Join(",", this.pieces.Select(p => p.name));

		private GrimoraChessboard activeChessboard;
		private int currentChessboardIndex = -1;
		private List<GrimoraChessboard> chessboards;
		private List<string> removedPieces;

		public List<string> RemovedPieces => GrimoraPlugin.ConfigCurrentRemovedPieces.Value.Split(',').ToList();

		private ChessboardEnemyPiece bossPiece;

		public ChessboardEnemyPiece BossPiece
		{
			get
			{
				if (bossPiece != null)
				{
					return bossPiece;
				}

				foreach (var piece in pieces.Where(piece => piece.name.Contains("Boss")))
				{
					bossPiece = piece as ChessboardEnemyPiece;
				}

				return bossPiece;
			}
		}

		public bool ChangingRegion { get; private set; }

		public List<GrimoraChessboard> Chessboards
		{
			get
			{
				LoadData();
				return chessboards;
			}
		}

		public void LoadData()
		{
			if (chessboards == null)
			{
				GrimoraPlugin.Log.LogDebug($"[ChessboardMapExt] Loading json boards");
				string jsonString = Encoding.UTF8.GetString(Resources.GrimoraChessboards);

				chessboards = ParseJson(
					SimpleJson.DeserializeObject<List<List<List<int>>>>(jsonString)
				);
			}
		}

		private MapData MapData => RunState.Run.map;

		private void TryInitializeMapData()
		{
			// if (RunState.Run.map == null)
			// {
			// 	// RunState.Run.map = MapGenerator.GenerateMap(RunState.CurrentMapRegion, 3, 13, PredefinedNodes, PredefinedScenery);
			// 	RunState.Run.currentNodeId = RunState.Run.map.RootNode.id;
			// }
		}

		public List<GrimoraChessboard> ParseJson(List<List<List<int>>> chessboards)
		{
			return chessboards.Select(board => new GrimoraChessboard(board)).ToList();
		}

		private void Start()
		{
			if (ViewManager.Instance.Controller is not null
			    && !ViewManager.Instance.Controller.allowedViews.Contains(View.MapDeckReview))
			{
				GrimoraPlugin.Log.LogDebug($"[MapExt] Adding MapDeckReview to allowed views");
				ViewManager.Instance.Controller.allowedViews.Add(View.MapDeckReview);
			}

			GrimoraPlugin.Log.LogDebug($"[MapExt] Setting on view changed");
			ViewManager instance = ViewManager.Instance;
			instance.ViewChanged = (Action<View, View>)Delegate
				.Combine(instance.ViewChanged, new Action<View, View>(OnViewChanged));

			// if (MapNodeManager.Instance != null)
			// {
			// 	GrimoraPlugin.Log.LogDebug($"[MapExt] MapNodeManager Instance is not null, setting up pieces");
			//
			// 	GrimoraPlugin.Log.LogDebug($"[MapExt] MapNodeManager Setting nodes to active");
			// 	MapNodeManager.Instance.FindAndSetActiveNodeInteractable();
			// }
		}

		public IEnumerator CompleteRegionSequence()
		{
			GrimoraPlugin.Log.LogDebug($"[CompleteRegionSequence] Starting CompleteRegionSequence");
			ChangingRegion = true;

			ViewManager.Instance.Controller.LockState = ViewLockState.Locked;

			// GrimoraPlugin.Log.LogDebug($"Calling NextRegion");

			// todo: replace with own method
			// RunState.Run.NextRegion();
			RunState.Run.regionTier++;
			RunState.Run.regionIndex = RegionProgression.GetRandomRegionIndexForTier(RunState.Run.regionTier);

			yield return new WaitForSeconds(0.5f);

			// yield return TextDisplayer.Instance.PlayDialogueEvent("RegionNext", TextDisplayer.MessageAdvanceMode.Input);

			ViewManager.Instance.SwitchToView(View.MapDefault);

			yield return new WaitForSeconds(0.5f);

			RunState.CurrentMapRegion.FadeInAmbientAudio();

			// yield return ShowMapSequence(0.75f);

			MapNodeManager.Instance.SetAllNodesInteractable(false);
			yield return new WaitForSeconds(1f);

			// yield return TextDisplayer.Instance.PlayDialogueEvent("Region" + RunState.CurrentMapRegion.name, TextDisplayer.MessageAdvanceMode.Input);

			GrimoraPlugin.Log.LogDebug($"[CompleteRegionSequence] Looping audio");
			AudioController.Instance.SetLoopAndPlay("finalegrimora_ambience");
			AudioController.Instance.SetLoopVolumeImmediate(0f);
			AudioController.Instance.FadeInLoop(1f, 1f);

			GrimoraPlugin.Log.LogDebug($"[CompleteRegionSequence] Current removed pieces before adding " +
			                           $"{GrimoraPlugin.ConfigCurrentRemovedPieces.Value}");
			GrimoraPlugin.ConfigCurrentRemovedPieces.Value += ChessboardMapExt.Instance.PiecesDelimited;
			GrimoraPlugin.ConfigCurrentRemovedPieces.Value =
				string.Join(",", GrimoraPlugin.ConfigCurrentRemovedPieces.Value.Split(',').Distinct());
			
			GrimoraPlugin.Log.LogDebug($"[CompleteRegionSequence] Current removed pieces after adding " +
			                           $"{GrimoraPlugin.ConfigCurrentRemovedPieces.Value}");
			
			GrimoraPlugin.Log.LogDebug($"[CompleteRegionSequence] HandleChessboardSetup called");
			// ChessboardMapExt.Instance.HandleChessboardSetup();
			
			MapNodeManager.Instance.FindAndSetActiveNodeInteractable();

			ViewManager.Instance.Controller.LockState = ViewLockState.Unlocked;

			GrimoraPlugin.Log.LogDebug($"[CompleteRegionSequence] No longer ChangingRegion");
			ChangingRegion = false;
		}

		public override IEnumerator UnrollingSequence(float unrollSpeed)
		{
			StoryEventsData.SetEventCompleted(StoryEvent.GrimoraReachedTable, true);

			TableRuleBook.Instance.SetOnBoard(false);

			// GrimoraPlugin.Log.LogDebug($"[ChessboardMap.UnrollingSequence] Setting each piece game object active to false");
			base.pieces.ForEach(delegate(ChessboardPiece x) { x.gameObject.SetActive(false); });
			// yield return new WaitForSeconds(0.5f);

			UpdateVisuals();

			GrimoraPlugin.Log.LogDebug($"[ChessboardMap.UnrollingSequence] Playing map anim enter");
			// base.mapAnim.speed = 1f;
			base.mapAnim.Play("enter", 0, 0f);
			yield return new WaitForSeconds(0.25f);

			// todo: play sound?

			// base.mapAnim.speed = unrollSpeed;
			// yield return new WaitForSeconds(0.15f);

			// GrimoraPlugin.Log.LogDebug($"[ChessboardMap.UnrollingSequence] Setting dynamicElements [{__instance.dynamicElementsParent}] to active");
			base.dynamicElementsParent.gameObject.SetActive(true);

			// for checking which nodes are active/inactive
			RenameMapNodesWithGridCoords();

			SetupGamePieces();

			yield return HandleActivatingChessPieces();

			HandlePlayerMarkerPosition();

			if (!DialogueEventsData.EventIsPlayed("FinaleGrimoraMapShown"))
			{
				yield return new WaitForSeconds(0.5f);
				yield return TextDisplayer.Instance.PlayDialogueEvent("FinaleGrimoraMapShown",
					TextDisplayer.MessageAdvanceMode.Input);
			}

			MapNodeManager.Instance.FindAndSetActiveNodeInteractable();
			
			SaveManager.SaveToFile();
		}

		public void SetupGamePieces()
		{
			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Setting up game pieces");

			HandleChessboardSetup();

			activeChessboard.CreateBlockerPiecesForBoard(this);

			var firstRow = activeChessboard.GetRowsWithOpenPathNodes.First();
			// var rowsWithOpenPathNodes = activeChessboard.GetRowsWithOpenPathNodes;

			var openPathNodes = activeChessboard.GetAllOpenPathNodes();
			// var nodeNearestToZeroZero = openPathNodes.First();
			// var nodeFarthestFromZeroZero = openPathNodes.Last();

			var firstNodeFirstRow = firstRow.GetOpenPathNodes().First();

			var nextRowAfterFirst = activeChessboard.GetRowsWithOpenPathNodes.Skip(1).First().GetOpenPathNodes().First();

			if (GrimoraPlugin.ConfigRoyalThirdBossDead.Value)
			{
				//tptohereforzone4
				GrimoraPlugin.Log.LogDebug($"Royal defeated");
				// ResetChessboard(__instance);

				ChessPieceUtils.CreateBossPiece(this, "GrimoraBoss", firstNodeFirstRow.GridX, firstNodeFirstRow.GridY);

				ChessPieceUtils.CreateEnemyPiece(this, nextRowAfterFirst.GridX, nextRowAfterFirst.GridY);

				ChessPieceUtils.CreateChestPiece(this, openPathNodes.Last().GridX, openPathNodes.Last().GridY);
			}
			else if (GrimoraPlugin.ConfigDoggySecondBossDead.Value)
			{
				GrimoraPlugin.Log.LogDebug($"Doggy defeated");

				ChessPieceUtils.CreateBossPiece(this, "RoyalBoss", firstNodeFirstRow.GridX, firstNodeFirstRow.GridY);

				ChessPieceUtils.CreateEnemyPiece(this, nextRowAfterFirst.GridX, nextRowAfterFirst.GridY);

				ChessPieceUtils.CreateChestPiece(this, openPathNodes.Last().GridX, openPathNodes.Last().GridY);
			}
			else if (GrimoraPlugin.ConfigKayceeFirstBossDead.Value)
			{
				//tptohereforzone2
				GrimoraPlugin.Log.LogDebug($"Kaycee defeated");

				ChessPieceUtils.CreateBossPiece(this, "DoggyBoss", firstNodeFirstRow.GridX, firstNodeFirstRow.GridY);

				ChessPieceUtils.CreateEnemyPiece(this, nextRowAfterFirst.GridX, nextRowAfterFirst.GridY);

				ChessPieceUtils.CreateChestPiece(this, openPathNodes.Last().GridX, openPathNodes.Last().GridY);
			}
			else
			{
				GrimoraPlugin.Log.LogDebug($"No bosses defeated yet, creating Kaycee");

				ChessPieceUtils.CreateBossPiece(this, "KayceeBoss", firstNodeFirstRow.GridX, firstNodeFirstRow.GridY);

				ChessPieceUtils.CreateEnemyPiece(this, nextRowAfterFirst.GridX, nextRowAfterFirst.GridY);

				ChessPieceUtils.CreateChestPiece(this, openPathNodes.Last().GridX, openPathNodes.Last().GridY);
			}

			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Finished setting up game pieces." +
			                           $" Current active list {PiecesDelimited}");
			
			var removedList = RemovedPieces;
			if (pieces.Count == removedList.Count)
			{
				// means it did not correctly create the new map
				GrimoraPlugin.ConfigCurrentRemovedPieces.Value = "";
				ChangingRegion = true;
				SetupGamePieces();
				ChangingRegion = false;
			}
		}

		private void HandleChessboardSetup()
		{
			currentChessboardIndex = GrimoraPlugin.ConfigCurrentChessboardIndex.Value;
			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Current chess board idx [{currentChessboardIndex}]");

			if (currentChessboardIndex == -1 || ChangingRegion)
			{
				currentChessboardIndex = Random.RandomRangeInt(0, Chessboards.Count);
				GrimoraPlugin.ConfigCurrentChessboardIndex.Value = currentChessboardIndex;
				GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] -> Setting new chessboard idx [{currentChessboardIndex}]");

				if (ChangingRegion)
				{
					GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Transitioning from boss," +
					                           $" setting active board to null");
					activeChessboard = null;
					// GrimoraPlugin.ConfigCurrentRemovedPieces.Value = "";
				}
			}

			if (activeChessboard is null)
			{
				GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] -> Index is not negative one, setting active chessboard");
				activeChessboard = Chessboards[currentChessboardIndex];
			}

			// DestroyDefaultChessboardPieces();
			ResetChessboard();
		}

		private void ResetChessboard()
		{
			var allPieces = new List<ChessboardPiece>(pieces);
			GrimoraPlugin.Log.LogDebug($"[ResetChessboard]" +
			                           $" Resetting board for all pieces [{string.Join(",", allPieces.Select(p => p.name))}]");
			foreach (var piece in allPieces)
			{
				pieces.Remove(piece);
				piece.gameObject.SetActive(false);
				piece.MapNode.OccupyingPiece = null;
				Destroy(piece.gameObject);
			}

			SetAllNodesActive();
		}

		public void AddPieceToRemovedPiecesConfig(string pieceName)
		{
			GrimoraPlugin.ConfigCurrentRemovedPieces.Value += "," + pieceName + ",";
		}

		private void SetAllNodesActive()
		{
			GrimoraPlugin.Log.LogDebug($"[SetAllNodesActive] setting all chess nodes active");
			foreach (var zone in ChessboardNavGrid.instance.zones)
			{
				zone.GetComponent<ChessboardMapNode>().SetActive(true);
			}
		}

		private void DestroyDefaultChessboardPieces()
		{
			GrimoraPlugin.Log.LogDebug($"Current active pieces " +
			                           $"[{string.Join(",", pieces.Select(p => p.name))}]");
			GrimoraPlugin.Log.LogDebug($"Current removed pieces " +
			                           $"[{string.Join(",", RemovedPieces)}]");

			foreach (var piece in FindObjectsOfType<ChessboardPiece>())
			{
				// pieces.Remove(piece);
				piece.MapNode.OccupyingPiece = null;
				piece.gameObject.SetActive(false);
				Destroy(piece.gameObject);
			}

			GrimoraPlugin.Log.LogDebug($"Pieces after removal " +
			                           $"[{string.Join(",", pieces.Select(p => p.name))}]");
		}

		private void HandlePlayerMarkerPosition()
		{
			int x = GrimoraSaveData.Data.gridX;
			int y = GrimoraSaveData.Data.gridY;
			var occupyingPiece = this.navGrid.zones[x, y].GetComponent<ChessboardMapNode>().OccupyingPiece;
			
			if (ChangingRegion || occupyingPiece is not null)
			{
				GrimoraPlugin.Log.LogDebug(
					$"[HandlePlayerMarkerPosition] Is boss transition or current active node has an already occupying piece");
				var allOpenPathNodes = activeChessboard.GetAllOpenPathNodes();

				do
				{
					for (var i = allOpenPathNodes.Count - 1; i > 0; i--)
					{
						x = allOpenPathNodes[i].GridX;
						y = allOpenPathNodes[i].GridY;
						
						occupyingPiece = this.navGrid.zones[x, y]
							.GetComponent<ChessboardMapNode>().OccupyingPiece;
					}
				} while (occupyingPiece is not null);
			}

			MapNodeManager.Instance.ActiveNode = this.navGrid.zones[x, y].GetComponent<MapNode>();
			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] MapNodeManager ActiveNode is x[{x}]y[{y}]");

			PlayerMarker.Instance.transform.position = MapNodeManager.Instance.ActiveNode.transform.position;
		}

		private IEnumerator HandleActivatingChessPieces()
		{
			// var activeList = ChessPieceUtils.CurrentActivePieces;
			var removedList = RemovedPieces;
			GrimoraPlugin.Log.LogDebug($"[HandleActivatingChessPieces] Pieces after removal " +
			                           $"[{string.Join(",", removedList)}]");

			pieces.ForEach(delegate(ChessboardPiece piece)
			{
				if (removedList.Contains(piece.name))
				{
					GrimoraPlugin.Log.LogDebug(
						$"[HandleSaveStatesForPieces] Setting inactive [{piece.gameObject}]] Node is active? [{piece.MapNode.isActiveAndEnabled}]]");
					piece.gameObject.SetActive(false);
					piece.MapNode.OccupyingPiece = null;
					// GrimoraPlugin.Log.LogDebug($"[HandleSaveStatesForPieces] -> is node active and enabled? [{piece.MapNode.isActiveAndEnabled}]]");
				}
				else
				{
					// GrimoraPlugin.Log.LogDebug($"[HandleSaveStatesForPieces] Setting active [{piece.name}]");
					piece.gameObject.SetActive(true);
				}

				piece.Hide(true);
			});

			GrimoraPlugin.Log.LogDebug("[HandleSaveStatesForPieces] Finished UpdatingSaveStates of pieces");

			yield return new WaitForSeconds(0.05f);

			yield return ShowPiecesThatAreActive();
		}

		private IEnumerator ShowPiecesThatAreActive()
		{
			foreach (var piece in pieces.Where(piece => piece.gameObject.activeInHierarchy))
			{
				// GrimoraPlugin.Log.LogDebug($"-> Piece [{piece.name}] saveId [{piece.saveId}] is active in hierarchy, calling Show method");
				piece.Show();
				yield return new WaitForSeconds(0.020f);
			}

			GrimoraPlugin.Log.LogDebug("[HandleSaveStatesForPieces] Finished showing all active pieces");
		}

		private void UpdateVisuals()
		{
			GrimoraPlugin.Log.LogDebug($"[{this.GetType()}] Updating visuals");
			TableVisualEffectsManager.Instance.SetFogPlaneShown(true);
			CameraEffects.Instance.SetFogEnabled(true);
			CameraEffects.Instance.SetFogAlpha(0f);
			CameraEffects.Instance.TweenFogAlpha(0.6f, 1f);

			TableVisualEffectsManager.Instance.SetDustParticlesActive(!RunState.CurrentMapRegion.dustParticlesDisabled);
		}


		private void OnViewChanged(View newView, View oldView)
		{
			switch (oldView)
			{
				case View.MapDefault when newView == View.MapDeckReview:
				{
					if (MapNodeManager.Instance != null)
					{
						MapNodeManager.Instance.SetAllNodesInteractable(false);
					}

					DeckReviewSequencer.Instance.SetDeckReviewShown(true, base.transform, base.DefaultPosition);
					break;
				}
				case View.MapDeckReview when newView == View.MapDefault:
				{
					DeckReviewSequencer.Instance.SetDeckReviewShown(false, base.transform, base.DefaultPosition);
					if (MapNodeManager.Instance != null)
					{
						MapNodeManager.Instance.FindAndSetActiveNodeInteractable();
					}

					break;
				}
			}
		}

		public void RenameMapNodesWithGridCoords()
		{
			if (string.Equals(
				this.navGrid.zones[0, 0].name,
				"ChessBoardMapNode",
				StringComparison.InvariantCultureIgnoreCase)
			)
			{
				GrimoraPlugin.Log.LogDebug($"ChessboardMap.UnrollingSequence] Renaming all map nodes");

				var zones = ChessboardNavGrid.instance.zones;
				for (var i = 0; i < zones.GetLength(0); i++)
				{
					for (var i1 = 0; i1 < zones.GetLength(1); i1++)
					{
						var obj = ChessboardNavGrid.instance.zones[i, i1].GetComponent<ChessboardMapNode>();
						obj.name = $"ChessboardMapNode_x[{i}]y[{i1}]";
					}
				}
			}
		}
	}
}