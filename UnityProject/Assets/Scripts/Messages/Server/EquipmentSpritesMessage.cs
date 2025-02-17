﻿using System.Collections;
using UnityEngine;
using Mirror;

public class EquipmentSpritesMessage : ServerMessage
{
	public static short MessageType = (short) MessageTypes.EquipmentSpritesMessage;
	public int Reference;
	public int Index;
	public uint EquipmentObject;
	public uint ItemNetID;
	public bool ForceInit;

	//Is this for the body parts or for the clothing items:
	public bool IsBodySprites;

	public override IEnumerator Process()
	{
		yield return WaitFor(EquipmentObject, ItemNetID);
		//Debug.Log(
		//	$"Received EquipMsg: Index {Index} ItemID: {ItemNetID} EquipID: {EquipmentObject} ForceInit: {ForceInit} IsBody: {IsBodySprites}");

		if (NetworkObjects[0] != null)
		{
			if (!IsBodySprites)
			{
				ClothingItem c = NetworkObjects[0].GetComponent<Equipment>().clothingSlots[Index];
				if (ItemNetID == NetId.Invalid)
				{
					if (!ForceInit) c.SetReference(null);
				}
				else
				{
					c.SetReference(NetworkObjects[1]);
				}

				if (ForceInit) c.PushTexture();
			}
			else
			{
				ClothingItem c = NetworkObjects[0].GetComponent<PlayerSprites>().characterSprites[Index];
				if (ItemNetID == NetId.Invalid)
				{
					if (!ForceInit) c.SetReference(null);
				}
				else
				{
					c.SetReference(NetworkObjects[1]);
				}

				if (ForceInit) c.PushTexture();
			}
		}
	}

	public static EquipmentSpritesMessage SendToAll(GameObject equipmentObject, int index, GameObject _Item,
		bool _forceInit = false, bool _isBodyParts = false)
	{
		var msg = CreateMsg(equipmentObject, index, _Item, _forceInit, _isBodyParts);
		msg.SendToAll();
		return msg;
	}

	public static EquipmentSpritesMessage SendTo(GameObject equipmentObject, int index, GameObject recipient,
		GameObject _Item, bool _forceInit, bool _isBodyParts)
	{
		var msg = CreateMsg(equipmentObject, index, _Item, _forceInit, _isBodyParts);
		msg.SendTo(recipient);
		return msg;
	}

	public static EquipmentSpritesMessage CreateMsg(GameObject equipmentObject, int index, GameObject _Item,
		bool _forceInit, bool _isBodyParts)
	{
		if (_Item != null)
		{
			return new EquipmentSpritesMessage
			{
				Index = index,
				EquipmentObject = equipmentObject.NetId(),
				ItemNetID = _Item.NetId(),
				ForceInit = _forceInit,
				IsBodySprites = _isBodyParts
			};
		}
		else
		{
			return new EquipmentSpritesMessage
			{
				Index = index,
				EquipmentObject = equipmentObject.NetId(),
				ItemNetID = NetId.Invalid,
				ForceInit = _forceInit,
				IsBodySprites = _isBodyParts
			};
		}
	}
}