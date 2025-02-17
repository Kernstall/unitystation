﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;

public class Networking : Editor
{
	[MenuItem("Networking/Give Random Item To All (Server)")]
	private static void GiveItems()
	{
		PlayerNetworkActions[] players = FindObjectsOfType<PlayerNetworkActions>();
		Pickupable[] items = FindObjectsOfType<Pickupable>();

		//		var gameObject = items[Random.Range(1, items.Length)].gameObject;
		for (int i = 0; i < players.Length; i++)
		{
			GameObject gameObject = items[Random.Range(1, items.Length)].gameObject;
			players[i].AddItemToUISlot(gameObject, EquipSlot.leftHand);
		}
	}
	[MenuItem("Networking/Push everyone up")]
	private static void PushEveryoneUp()
	{
		foreach (ConnectedPlayer player in PlayerList.Instance.InGamePlayers)
		{
			player.GameObject.GetComponent<PlayerScript>().PlayerSync.Push(Vector2Int.up);
		}
	}
	[MenuItem("Networking/Spawn some meat")]
	private static void SpawnMeat()
	{
		foreach (ConnectedPlayer player in PlayerList.Instance.InGamePlayers) {
			Vector3 playerPos = player.GameObject.GetComponent<PlayerScript>().PlayerSync.ServerState.WorldPosition;
			Vector3 spawnPos = playerPos + new Vector3( 0, 2, 0 );
			GameObject mealPrefab = CraftingManager.Meals.FindOutputMeal("Meat Steak");
			var slabs = new List<CustomNetTransform>();
			for ( int i = 0; i < 5; i++ ) {
				slabs.Add( PoolManager.PoolNetworkInstantiate(mealPrefab, spawnPos).GetComponent<CustomNetTransform>() );
			}
			for ( var i = 0; i < slabs.Count; i++ ) {
				Vector3 vector3 = i%2 == 0 ? new Vector3(i,-i,0) : new Vector3(-i,i,0);
				slabs[i].ForceDrop( spawnPos + vector3/10 );
			}
		}
	}
	[MenuItem("Networking/Print player positions")]
	private static void PrintPlayerPositions()
	{
		//For every player in the connected player list (this list is serverside-only)
		foreach (ConnectedPlayer player in PlayerList.Instance.InGamePlayers) {
			//Printing this the pretty way, example:
			//Bob (CAPTAIN) is located at (77,0, 52,0, 0,0)
			Logger.LogFormat( "{0} ({1)} is located at {2}.", Category.Server, player.Name, player.Job, player.Script.WorldPos );
		}

	}

	[MenuItem("Networking/Spawn dummy player")]
	private static void SpawnDummyPlayer() {
		SpawnHandler.SpawnDummyPlayer( JobType.ASSISTANT );
	}

	[MenuItem("Networking/Transform Waltz (Server)")]
	private static void MoveAll()
	{
		CustomNetworkManager.Instance.MoveAll();
	}

	[MenuItem("Networking/Gib All (Server)")]
	private static void GibAll()
	{
		GibMessage.Send();
	}
	[MenuItem("Networking/Restart round")]
	private static void AdminPlzRestart()
	{
		GameManager.Instance.RestartRound();
	}
	[MenuItem("Networking/Extend round time")]
	private static void ExtendRoundTime()
	{
		GameManager.Instance.ResetRoundTime();
	}

	[MenuItem("Networking/Kill local player (Server only)")]
	private static void KillLocalPlayer()
	{
		if (CustomNetworkManager.Instance._isServer)
		{
			PlayerManager.LocalPlayerScript.playerHealth.ApplyDamage(null, 99999f, AttackType.Internal, DamageType.Brute);
		}
	}

	[MenuItem("Networking/Respawn local player (Server only)")]
	private static void RespawnLocalPlayer()
	{
		if (CustomNetworkManager.Instance._isServer)
		{
			PlayerManager.LocalPlayerScript.playerNetworkActions.CmdRespawnPlayer();
		}
	}
}