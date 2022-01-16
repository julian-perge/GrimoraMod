﻿using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace GrimoraMod
{
	public class GrimoraChessboard
	{
		public readonly int indexInList;
		public readonly List<ChessNode> BlockerNodes;
		public readonly ChessNode BossNode;
		public readonly List<ChessNode> ChestNodes;
		public readonly List<ChessNode> EnemyNodes;
		public readonly List<ChessNode> OpenPathNodes;
		public readonly ChessNode PlayerNode;

		protected internal ChessboardEnemyPiece BossPiece =>
			GetPieceAtSpace(BossNode.GridX, BossNode.GridY) as ChessboardEnemyPiece;

		public readonly List<ChessRow> Rows;

		public Opponent.Type ActiveBossType;

		public GrimoraChessboard(IEnumerable<List<int>> board, int indexInList)
		{
			this.Rows = board.Select((_board, idx) => new ChessRow(_board, idx)).ToList();
			this.BlockerNodes = GetBlockerNodes();
			this.BossNode = GetBossNode();
			this.ChestNodes = GetChestNodes();
			this.EnemyNodes = GetEnemyNodes();
			this.OpenPathNodes = GetOpenPathNodes();
			this.PlayerNode = GetPlayerNode();
			this.indexInList = indexInList;
		}

		public List<ChessNode> GetOpenPathNodes()
		{
			return Rows.SelectMany(row => row.GetNodesOfType(0)).ToList();
		}

		private List<ChessNode> GetBlockerNodes()
		{
			return Rows.SelectMany(row => row.GetNodesOfType(1)).ToList();
		}

		private List<ChessNode> GetChestNodes()
		{
			return Rows.SelectMany(row => row.GetNodesOfType(2)).ToList();
		}

		private List<ChessNode> GetEnemyNodes()
		{
			return Rows.SelectMany(row => row.GetNodesOfType(3)).ToList();
		}

		private ChessNode GetBossNode()
		{
			return Rows.SelectMany(row => row.GetNodesOfType(4)).Single();
		}

		public ChessNode GetPlayerNode()
		{
			return Rows.SelectMany(row => row.GetNodesOfType(9)).Single();
		}

		public void SetupBoard()
		{
			if (GrimoraPlugin.ConfigRoyalThirdBossDead.Value)
			{
				GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Royal defeated");
				PlaceBossPiece("GrimoraBoss");
			}
			else if (GrimoraPlugin.ConfigDoggySecondBossDead.Value)
			{
				GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Doggy defeated");
				PlaceBossPiece("RoyalBoss");
			}
			else if (GrimoraPlugin.ConfigKayceeFirstBossDead.Value)
			{
				GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Kaycee defeated");
				PlaceBossPiece("DoggyBoss");
			}
			else
			{
				GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] No bosses defeated yet, creating Kaycee");
				PlaceBossPiece("KayceeBoss");
			}

			PlaceBlockerPieces();
			PlaceChestPieces();
			PlaceEnemyPieces();
		}

		public void UpdatePlayerMarkerPosition(bool changingRegion)
		{
			int x = GrimoraSaveData.Data.gridX;
			int y = GrimoraSaveData.Data.gridY;

			GrimoraPlugin.Log.LogDebug($"[HandlePlayerMarkerPosition] " +
			                           $"Player Marker name [{PlayerMarker.Instance.name}] " +
			                           $"x{x}y{y} coords");

			var occupyingPiece = GetPieceAtSpace(x, y);

			bool isPlayerOccupied = occupyingPiece is not null && PlayerMarker.Instance.name == occupyingPiece.name;

			GrimoraPlugin.Log.LogDebug($"[HandlePlayerMarkerPosition] isPlayerOccupied? [{isPlayerOccupied}]");

			if (changingRegion)
			{
				// the PlayerNode will be different since this is now a different chessboard
				x = GetPlayerNode().GridX;
				y = GetPlayerNode().GridY;
			}

			MapNodeManager.Instance.ActiveNode = ChessboardNavGrid.instance.zones[x, y].GetComponent<MapNode>();
			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] MapNodeManager ActiveNode is x[{x}]y[{y}]");

			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] SetPlayerAdjacentNodesActive");
			ChessboardNavGrid.instance.SetPlayerAdjacentNodesActive();

			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Setting player position to active node");
			PlayerMarker.Instance.transform.position = MapNodeManager.Instance.ActiveNode.transform.position;
		}

		public void SetSavePositions()
		{
			// set the updated position to spawn the player in
			GrimoraSaveData.Data.gridX = GetPlayerNode().GridX;
			GrimoraSaveData.Data.gridY = GetPlayerNode().GridY;
		}

		#region Prefabs

		public const string PrefabPath = "Prefabs/Map/ChessboardMap";

		public static Mesh MeshFilterBlockerIceBlock => GrimoraPlugin.AllAssets[8] as Mesh;
		public static Mesh MeshFilterBlockerBones => GrimoraPlugin.AllAssets[5] as Mesh;
		public static Mesh MeshFilterBlockerBarrels => GrimoraPlugin.AllAssets[2] as Mesh;

		public static ChessboardBlockerPiece PrefabTombstone =>
			ResourceBank.Get<ChessboardBlockerPiece>($"{PrefabPath}/Chessboard_Tombstone_1");

		public static ChessboardEnemyPiece PrefabEnemyPiece =>
			ResourceBank.Get<ChessboardEnemyPiece>($"{PrefabPath}/ChessboardEnemyPiece");

		public static ChessboardEnemyPiece PrefabBossPiece =>
			ResourceBank.Get<ChessboardEnemyPiece>($"{PrefabPath}/BossFigurine");

		public static ChessboardChestPiece PrefabChestPiece =>
			ResourceBank.Get<ChessboardChestPiece>($"{PrefabPath}/ChessboardChestPiece");

		#endregion

		#region HelperMethods

		private static ChessboardPiece GetPieceAtSpace(int x, int y)
		{
			return ChessboardNavGrid.instance.zones[x, y].GetComponent<ChessboardMapNode>().OccupyingPiece;
		}

		private static EncounterBlueprintData GetBlueprint(string bossType = "")
		{
			List<Opponent.Type> opponentTypes = BlueprintUtils.RegionWithBlueprints.Keys.ToList();

			var randomType = string.IsNullOrEmpty(bossType)
				? opponentTypes[UnityEngine.Random.RandomRangeInt(0, opponentTypes.Count)]
				: BaseBossExt.BossTypesByString.GetValueSafe(bossType);

			var blueprints = BlueprintUtils.RegionWithBlueprints[randomType];
			return blueprints[UnityEngine.Random.RandomRangeInt(0, blueprints.Count)];
		}

		#endregion

		#region PlacingPieces

		public void PlaceBlockerPieces()
		{
			GrimoraPlugin.Log.LogDebug($"[SetupGamePieces] Creating blocker pieces for the board");
			BlockerNodes.ForEach(node => CreateBlockerPiece(node.GridX, node.GridY));
		}

		public void PlaceBossPiece(string bossName)
		{
			CreateBossPiece(bossName, BossNode.GridX, BossNode.GridY);
		}

		public void PlaceChestPiece(int x, int y)
		{
			CreateChestPiece(x, y);
		}

		public void PlaceChestPieces()
		{
			ChestNodes.ForEach(node => CreateChestPiece(node.GridX, node.GridY));
		}

		public void PlaceEnemyPiece(int x, int y)
		{
			CreateEnemyPiece(x, y);
		}

		public void PlaceEnemyPieces()
		{
			EnemyNodes.ForEach(node => CreateEnemyPiece(node.GridX, node.GridY));
		}

		#endregion

		#region CreatePieces

		private void CreateBossPiece(string id, int x, int y)
		{
			CreateBaseEnemyPiece(PrefabBossPiece, x, y, id);
		}

		private void CreateChestPiece(int x, int y)
		{
			// GrimoraPlugin.Log.LogDebug($"Attempting to create chest piece at x [{x}] y [{y}]");
			CreateChessPiece(PrefabChestPiece, x, y);
		}

		private void CreateEnemyPiece(int x, int y)
		{
			// GrimoraPlugin.Log.LogDebug($"Space is not occupied, attempting to create enemy piece at x [{x}] y [{y}]");
			CreateBaseEnemyPiece(PrefabEnemyPiece, x, y);
		}

		private void CreateBaseEnemyPiece(ChessboardPiece prefab, int x, int y, string id = "")
		{
			// GrimoraPlugin.Log.LogDebug($"Space is not occupied, attempting to create enemy piece at x [{x}] y [{y}]");
			CreateChessPiece(prefab, x, y, id);
		}

		private void CreateBlockerPiece(int x, int y)
		{
			// GrimoraPlugin.Log.LogDebug($"Attempting to create blocker piece at x [{x}] y [{y}]");
			CreateChessPiece(PrefabTombstone, x, y);
		}

		public Mesh GetActiveRegionBlockerMesh()
		{
			Mesh meshObj = ActiveBossType switch
			{
				BaseBossExt.DoggyOpponent => MeshFilterBlockerBones,
				BaseBossExt.RoyalOpponent => MeshFilterBlockerBarrels,
				_ => MeshFilterBlockerIceBlock
			};

			return meshObj;
		}

		private void CreateChessPiece(ChessboardPiece prefab, int x, int y, string id = "")
		{
			string coordName = $"x[{x}]y[{y}]";

			ChessboardPiece piece = GetPieceAtSpace(x, y);

			if (ChessboardMapExt.Instance.RemovedPieces.Exists(c => piece is not null && c == piece.name))
			{
				GrimoraPlugin.Log.LogDebug($"-> Skipping [{coordName}] as it already exists. Setting MapNode to active.");
				piece.MapNode.SetActive(true);
			}
			else
			{
				if (piece is null)
				{
					piece = UnityEngine.Object.Instantiate(prefab, ChessboardMapExt.Instance.dynamicElementsParent);
					piece.gridXPos = x;
					piece.gridYPos = y;
					piece.saveId = x * 10 + y * 1000;

					string nameTemp = piece.GetType().Name.Replace("Chessboard", "") + "_" + coordName;

					switch (piece)
					{
						case ChessboardEnemyPiece enemyPiece:
						{
							enemyPiece.GoalPosX = x;
							enemyPiece.GoalPosX = y;
							enemyPiece.blueprint = GetBlueprint(id);

							if (prefab.name.Contains("Boss"))
							{
								// GrimoraPlugin.Log.LogDebug($"Prefab piece is boss, setting name");
								enemyPiece.specialEncounterId = id;
								nameTemp = nameTemp.Replace("Enemy", "Boss");
							}

							break;
						}
						case ChessboardBlockerPiece blockerPiece:
						{
							Mesh blockerMesh = GetActiveRegionBlockerMesh();
							foreach (var meshFilter in blockerPiece.GetComponentsInChildren<MeshFilter>())
							{
								GameObject meshFilerObj = meshFilter.gameObject;
								if (meshFilerObj.name != "Base")
								{
									UnityEngine.Object.Destroy(meshFilter);
								}
								else
								{
									// meshFilter59.mesh = (Pluginz.allAssets[2] as Mesh);
									// .material.mainTexture = (Pluginz.allAssets[3] as Texture2D);
									// .sharedMaterial.mainTexture = (Pluginz.allAssets[3] as Texture2D);

									meshFilter.mesh = blockerMesh;
									// meshObj.GetComponent<MeshRenderer>().material.mainTexture = blockerMesh as Texture2D;
									// meshObj.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = allAssets[3] as Texture2D;
									meshFilerObj.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
									meshFilerObj.transform.localPosition = new Vector3(0f, -0.0209f, 0f);
								}
							}

							break;
						}
					}

					piece.name = nameTemp;

					GrimoraPlugin.Log.LogDebug($"[CreatingPiece] {piece.name}");
					ChessboardMapExt.Instance.pieces.Add(piece);
				}
			}
		}

		#endregion
	}
}