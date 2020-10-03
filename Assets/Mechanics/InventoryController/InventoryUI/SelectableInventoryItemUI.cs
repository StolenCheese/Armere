﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class SelectableInventoryItemUI : InventoryItemUI, ISelectHandler, IPointerEnterHandler, IPointerClickHandler
{
    public event System.Action onSelect;
    public InventoryUI inventoryUI;
    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryUI.ShowContextMenu(type, itemIndex, eventData.position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onSelect?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelect?.Invoke();
    }


}
