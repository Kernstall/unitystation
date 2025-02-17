﻿using System.Linq;
using UnityEditor;
using UnityEngine;
using Utility = UnityEngine.Networking.Utility;
using Mirror;

public class Sprite2PrefabChild_Gun_MenuItem
{
	/// <summary>
	///     Creates prefab, with sprites as child
	/// </summary>
	[MenuItem("Assets/Create/Sprite2PrefabChild/Gun", false, 11)]
	public static void ScriptableObjectTemplateMenuItem()
	{
		bool makeSeperateFolders = EditorUtility.DisplayDialog("Prefab Folders?", "Do you want each prefab in it's own folder?", "Yes", "No");
		for (int i = 0; i < Selection.objects.Length; i++)
		{
			string spriteSheet = AssetDatabase.GetAssetPath(Selection.objects[i]);
			Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
			string[] splitSpriteSheet = spriteSheet.Split('/');
			string fullFolderPath = Inset(spriteSheet, 0, splitSpriteSheet[splitSpriteSheet.Length - 1].Length + 1) + "/" + Selection.objects[i].name;
			string folderName = Selection.objects[i].name;
			string adjFolderPath = InsetFromEnd(fullFolderPath, Selection.objects[i].name.Length + 1);

			if (!AssetDatabase.IsValidFolder(fullFolderPath))
			{
				AssetDatabase.CreateFolder(adjFolderPath, folderName);
			}

			GameObject parent = new GameObject();
			parent.AddComponent<BoxCollider2D>();
			parent.AddComponent<NetworkIdentity>();
			parent.AddComponent<NetworkTransform>();
			parent.AddComponent<ItemAttributes>();
			parent.AddComponent<Gun>();
			parent.AddComponent<ObjectBehaviour>();
			parent.AddComponent<RegisterItem>();
			GameObject spriteObject = new GameObject();
			SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
			Material spriteMaterial = Resources.Load("Sprite-PixelSnap", typeof(Material)) as Material;
			for (int j = 0; j < sprites.Length; j++)
			{
				EditorUtility.DisplayProgressBar(i + 1 + "/" + Selection.objects.Length + " Generating Prefabs", "Prefab: " + j, j / (float) sprites.Length);
				parent.name = sprites[j].name;
				spriteObject.name = "Sprite";
				spriteRenderer.sprite = sprites[j];
				spriteObject.GetComponent<SpriteRenderer>().material = spriteMaterial;


				string savePath = makeSeperateFolders
					? fullFolderPath + "/" + sprites[j].name + "/" + sprites[j].name + ".prefab"
					: fullFolderPath + "/" + sprites[j].name + ".prefab";

				if (makeSeperateFolders)
				{
					if (!AssetDatabase.IsValidFolder(fullFolderPath + "/" + sprites[j].name))
					{
						AssetDatabase.CreateFolder(fullFolderPath, sprites[j].name);
					}
				}
				spriteObject.transform.parent = parent.transform;
				PrefabUtility.CreatePrefab(savePath, parent);
			}
			Object.DestroyImmediate(parent);
			Object.DestroyImmediate(spriteObject);
		}
		EditorUtility.ClearProgressBar();
	}

	/// <summary>
	///     removes inset amounts from string ie. "0example01" with leftIn at 1 and with rightIn at 2 would result in "example"
	/// </summary>
	/// <param name="me"></param>
	/// <param name="inset"></param>
	/// <returns></returns>
	public static string Inset(string me, int leftIn, int rightIn)
	{
		return me.Substring(leftIn, me.Length - rightIn - leftIn);
	}

	/// <summary>
	///     removes inset amount from string end ie. "example01" with inset at 2 would result in "example"
	/// </summary>
	/// <param name="me"></param>
	/// <param name="inset"></param>
	/// <returns></returns>
	public static string InsetFromEnd(string me, int inset)
	{
		return me.Substring(0, me.Length - inset);
	}
}