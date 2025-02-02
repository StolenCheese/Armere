﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{
	public enum ArmourPosition { Helmet, Chest, Legs }

	[CreateAssetMenu(menuName = "Game/Items/Armour Item Data", fileName = "New Armour Data")]
	[AllowItemTypes(ItemType.Armour)]
	public class ArmourItemData : ItemData
	{
		[Header("Armour")]
		public float armourValue;
		public ArmourPosition armourPosition;
		public bool hideBody = true;
		public AssetReferenceGameObject armaturePrefab;
	}
}