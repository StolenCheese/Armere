﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class InventoryItemUI : MonoBehaviour
{
    //Set by script
    public int itemIndex;
    public ItemType type;
    //Set by hand
    public Image thumbnail;
    public TMPro.TextMeshProUGUI countText;
    public TMPro.TextMeshProUGUI infoText;
    public TMPro.TextMeshProUGUI nameText;
    AsyncOperationHandle<Sprite> asyncOperation;
    public void ChangeItemIndex(int newIndex)
    {

        itemIndex = newIndex;
        if (itemIndex != -1)
            SetupItemAsync(InventoryController.singleton.db[InventoryController.ItemAt(itemIndex, type)]);
    }

    public async void SetupItemAsync(ItemData item)
    {
        type = item.type;

        switch (item)
        {
            case MeleeWeaponItemData melee:
                infoText?.SetText(melee.damage.ToString());
                if (countText != null) countText.enabled = false;
                break;
        }

        if (item.displaySprite.RuntimeKeyIsValid())
        {
            asyncOperation = Addressables.LoadAssetAsync<Sprite>(item.displaySprite);

            Sprite s = await asyncOperation.Task;
            //The image may have been destroyed before finishing
            if (thumbnail != null) thumbnail.sprite = s;
        }
    }


    private void OnDestroy()
    {
        ReleaseCurrentSprite();
    }
    public void ReleaseCurrentSprite()
    {
        if (asyncOperation.IsValid())
            Addressables.Release(asyncOperation);
    }

}
