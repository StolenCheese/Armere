﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{


    [CreateAssetMenu(menuName = "Game/Items/Offhand Item Data", fileName = "New Offhand Item Data")]
    [AllowItemTypes(ItemType.SideArm)]
    public class SideArmItemData : HoldableItemData
    {

    }
}