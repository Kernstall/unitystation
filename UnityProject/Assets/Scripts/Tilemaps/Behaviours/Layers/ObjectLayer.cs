﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// ObjectLayer holds all the objects on all the tiles in the game world - specifically the RegisterTile components of those objects.
/// It provides functionality for checking what should occur on given tiles, such as if a tile at a specific location should be passable.
/// </summary>
[ExecuteInEditMode]
public class ObjectLayer : Layer
{
	private TileList serverObjects;
	private TileList clientObjects;

	public TileList ServerObjects => serverObjects ?? (serverObjects = new TileList());
	public TileList ClientObjects => clientObjects ?? (clientObjects = new TileList());

	public override void SetTile(Vector3Int position, GenericTile tile, Matrix4x4 transformMatrix)
	{
		ObjectTile objectTile = tile as ObjectTile;

		if (objectTile)
		{
			if (!objectTile.IsItem)
			{
				tilemap.SetTile(position, null);
			}

			objectTile.SpawnObject(position, tilemap, transformMatrix);
		}
		else
		{
			base.SetTile(position, tile, transformMatrix);
		}
	}

	public override bool HasTile(Vector3Int position, bool isServer)
	{
		return (isServer ? ServerObjects.HasObjects(position) : ClientObjects.HasObjects(position)) || base.HasTile(position, isServer);
	}

	public override void RemoveTile(Vector3Int position, bool removeAll = false)
	{
		foreach ( RegisterTile obj in ClientObjects.Get(position) )
		{
			DestroyImmediate(obj.gameObject);
		}
		foreach ( RegisterTile obj in ServerObjects.Get(position) )
		{
			DestroyImmediate(obj.gameObject);
		}

		base.RemoveTile(position, removeAll);
	}

	public override bool IsPassableAt(Vector3Int origin, Vector3Int to, bool isServer,
									  CollisionType collisionType = CollisionType.Player, bool inclPlayers = true, GameObject context = null)
	{
		//Targeting windoors here
		foreach ( RegisterTile t in isServer ? ServerObjects.Get(origin) : ClientObjects.Get(origin) )
		{
			if (!t.IsPassableTo(to, isServer) && (!context || t.gameObject != context))
			{
				//Can't get outside the tile because windoor doesn't allow us
				return false;
			}
		}

		foreach ( RegisterTile o in isServer ? ServerObjects.Get(to) : ClientObjects.Get(to) )
		{
			if ((inclPlayers || o.ObjectType != ObjectType.Player) && !o.IsPassable(origin, isServer) && (!context || o.gameObject != context))
			{
				return false;
			}
		}

		return base.IsPassableAt(origin, to, isServer, collisionType: collisionType, inclPlayers: inclPlayers, context: context);
	}

	public override bool IsAtmosPassableAt(Vector3Int origin, Vector3Int to, bool isServer)
	{
		foreach ( RegisterTile t in isServer ? ServerObjects.Get(to) : ClientObjects.Get(to) )
		{
			if (!t.IsAtmosPassable(origin, isServer))
			{
				return false;
			}
		}

		foreach ( RegisterTile t in isServer ? ServerObjects.Get(origin) : ClientObjects.Get(origin) )
		{
			if (!t.IsAtmosPassable(to, isServer))
			{
				return false;
			}
		}

		return base.IsAtmosPassableAt(origin, to, isServer);
	}

	public override bool IsSpaceAt(Vector3Int position, bool isServer)
	{
		return IsAtmosPassableAt(position, position, isServer) && base.IsSpaceAt(position, isServer);
	}

	public override void ClearAllTiles()
	{
		for (var i = 0; i < ClientObjects.AllObjects.Count; i++)
		{
			RegisterTile obj = ClientObjects.AllObjects[i];
			if (obj != null)
			{
				DestroyImmediate(obj.gameObject);
			}
		}
		for (var i = 0; i < ServerObjects.AllObjects.Count; i++)
		{
			RegisterTile obj = ServerObjects.AllObjects[i];
			if (obj != null)
			{
				DestroyImmediate(obj.gameObject);
			}
		}

		base.ClearAllTiles();
	}
}