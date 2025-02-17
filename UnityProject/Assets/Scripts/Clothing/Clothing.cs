﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Clothing : NetworkBehaviour
{

	[SyncVar(hook = nameof(SyncType))]
	public ClothingVariantType Type;

	public int Variant = -1;

	public ClothingData clothingData;
	public ContainerData containerData;
	public HeadsetData headsetData;

	public bool Initialised;

	[HideInInspector] [SyncVar(hook = nameof(SyncFindData))]
	public string SynchronisedString;

	public Dictionary<ClothingVariantType, int> VariantStore = new Dictionary<ClothingVariantType, int>();
	public List<int> VariantList;
	public SpriteData SpriteInfo;

	public void SyncFindData(string syncString)
	{
		if (string.IsNullOrEmpty(syncString)) return;

		SynchronisedString = syncString;
		if (ClothFactory.Instance.ClothingStoredData.ContainsKey(syncString))
		{
			clothingData = ClothFactory.Instance.ClothingStoredData[syncString];
			TryInit();
		}
		else if (ClothFactory.Instance.BackpackStoredData.ContainsKey(syncString))
		{
			containerData = ClothFactory.Instance.BackpackStoredData[syncString];
			TryInit();
		}
		else if (ClothFactory.Instance.HeadSetStoredData.ContainsKey(syncString))
		{
			headsetData = ClothFactory.Instance.HeadSetStoredData[syncString];
			TryInit();
		}
		else
		{
			Logger.LogError($"No Cloth Data found for {syncString}", Category.SpriteHandler);
		}
	}

	public void SyncType(ClothingVariantType _Type) {
		Type = _Type;

	}

	public int ReturnSetState()
	{
		if (VariantStore.ContainsKey(Type))
		{
			return (VariantStore[Type]);
		}

		return (0);
	}

	public int ReturnState(ClothingVariantType CVT)
	{
		if (VariantStore.ContainsKey(CVT))
		{
			return (VariantStore[CVT]);
		}

		return (0);
	}

	public int ReturnVariant(int VI)
	{
		if (VariantList.Count > VI)
		{
			return (VariantList[VI]);
		}

		return (0);
	}

	public void SetSynchronise(ClothingData CD = null, ContainerData ConD = null, HeadsetData HD = null)
	{
		if (CD != null)
		{
			SynchronisedString = CD.name;
			clothingData = CD;
		}
		else if (ConD != null)
		{
			SynchronisedString = ConD.name;
			containerData = ConD;
		}
		else if (HD != null)
		{
			SynchronisedString = HD.name;
			headsetData = HD;
		}
	}


	void Start() {
		TryInit();
	}

	//Attempt to apply the data to the Clothing Item
	private void TryInit()
	{
		if (Initialised != true)
		{
			if (clothingData != null)
			{
				if (clothingData.ItemAttributes.itemName != "")
				{this.name = clothingData.ItemAttributes.itemName;}
				else {this.name = clothingData.name;}

				var _Clothing = GetComponent<Clothing>();
				var Item = GetComponent<ItemAttributes>();
				_Clothing.SpriteInfo = StaticSpriteHandler.SetUpSheetForClothingData(clothingData, this);
				Item.SetUpFromClothingData(clothingData.Base, clothingData.ItemAttributes);

				switch (Type)
				{
					case ClothingVariantType.Default:
						if (Variant > -1)
						{
							if (!(clothingData.Variants.Count >= Variant))
							{
								Item.SetUpFromClothingData(clothingData.Variants[Variant], clothingData.ItemAttributes);
							}
						}

						break;
					case ClothingVariantType.Skirt:
						Item.SetUpFromClothingData(clothingData.DressVariant, clothingData.ItemAttributes);
						break;
					case ClothingVariantType.Tucked:
						Item.SetUpFromClothingData(clothingData.Base_Adjusted, clothingData.ItemAttributes);
						break;
				}

				Initialised = true;
			}
			else if (containerData != null)
			{

				if (containerData.ItemAttributes.itemName != "")
				{this.name = containerData.ItemAttributes.itemName;}
				else {this.name = containerData.name;}

				var Item = GetComponent<ItemAttributes>();
				var Storage = GetComponent<StorageObject>();
				this.SpriteInfo = StaticSpriteHandler.SetupSingleSprite(containerData.Sprites.Equipped);
				Item.SetUpFromClothingData(containerData.Sprites, containerData.ItemAttributes);
				Storage.SetUpFromStorageObjectData(containerData.StorageData);
				Initialised = true;
			}
			else if (headsetData != null)
			{

				if (headsetData.ItemAttributes.itemName != "")
				{this.name = headsetData.ItemAttributes.itemName;}
				else {this.name = headsetData.name;}

				var Item = GetComponent<ItemAttributes>();
				var Headset = GetComponent<Headset>();
				this.SpriteInfo = StaticSpriteHandler.SetupSingleSprite(headsetData.Sprites.Equipped);
				Item.SetUpFromClothingData(headsetData.Sprites, headsetData.ItemAttributes);
				Headset.EncryptionKey = headsetData.Key.EncryptionKey;
				Initialised = true;
			}
		}
	}
}

public enum ClothingVariantType
{
	Default,
	Tucked,
	Skirt
}