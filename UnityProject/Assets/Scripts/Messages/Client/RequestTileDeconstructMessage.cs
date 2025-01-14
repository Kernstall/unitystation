﻿using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
///     Request from client to deconstruct a tile
/// </summary>
public class RequestTileDeconstructMessage : ClientMessage
{
	public static short MessageType = (short) MessageTypes.RequestTileDeconstructMessage;

	public uint Player;
	public uint MatrixRoot;
	public int TileType;
	public Vector3 CellPos;
	public Vector3 CellWorldPos;

	public override IEnumerator Process()
	{
		yield return WaitFor(Player, MatrixRoot);
		CraftingManager.Deconstruction.ProcessDeconstructRequest(NetworkObjects[0], NetworkObjects[1],
			(TileType) TileType, CellPos, CellWorldPos);
	}

	public static RequestTileDeconstructMessage Send(GameObject player, GameObject matrixRoot, TileType tileType, Vector3 cellPos, Vector3 cellWorldPos)
	{
		RequestTileDeconstructMessage msg = new RequestTileDeconstructMessage
		{
			Player = player.GetComponent<NetworkIdentity>().netId,
				MatrixRoot = matrixRoot.GetComponent<NetworkIdentity>().netId,
				TileType = (int) tileType,
				CellPos = cellPos,
				CellWorldPos = cellWorldPos
		};
		msg.Send();
		return msg;
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		Player = reader.ReadUInt32();
		MatrixRoot = reader.ReadUInt32();
		TileType = reader.ReadInt32();
		CellPos = reader.ReadVector3();
		CellWorldPos = reader.ReadVector3();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.WriteUInt32(Player);
		writer.WriteUInt32(MatrixRoot);
		writer.WriteInt32(TileType);
		writer.WriteVector3(CellPos);
		writer.WriteVector3(CellWorldPos);
	}
}