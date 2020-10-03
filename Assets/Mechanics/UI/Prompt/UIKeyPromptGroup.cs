﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using Malee.List;

[RequireComponent(typeof(LayoutGroup))]
public class UIKeyPromptGroup : MonoBehaviour
{
    [System.Serializable]
    public struct KeyPrompt
    {
        public string name;
        public string action;

        public KeyPrompt(string name, string action)
        {
            this.name = name;
            this.action = action;
        }
    }
    [System.Serializable]
    public struct KeyBindSprite
    {
        public string startsWith;
        public Sprite sprite;
        public bool includeLabel;
    }
    public float preferredWidth = 100f;
    public float preferredHeight = 100f;
    public TMP_FontAsset fontAsset;
    public Material maskingTextMaterial;
    public Material maskedSpriteMaterial;

    [System.Serializable]
    public class KeyBindSpritesArray : ReorderableArray<KeyBindSprite> { }
    [Reorderable(paginate = false)]
    public KeyBindSpritesArray keybindSpritesOrder;
    public static UIKeyPromptGroup singleton;



    private void Start()
    {
        //ShowPrompts(player, "Ground Action Map", starts);
        //player = PlayerInput.all[0];
    }
    private void Awake()
    {
        singleton = this;
    }
    public void RemovePrompts()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
    }
    public void ShowPrompts(PlayerInput player, string map, params KeyPrompt[] prompts)
    {



        for (int i = 0; i < prompts.Length; i++)
        {
            var holder = new GameObject(prompts[i].name, typeof(RectTransform), typeof(LayoutElement));
            holder.transform.SetParent(transform, false);
            var element = holder.GetComponent<LayoutElement>();
            element.preferredHeight = preferredHeight;
            element.preferredWidth = preferredWidth;
            //Name text
            var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var t = text.GetComponent<TextMeshProUGUI>();
            t.font = fontAsset;
            t.text = prompts[i].name;
            t.alignment = TextAlignmentOptions.Left;
            text.transform.SetParent(holder.transform, false);

            ExpandRectTransform(text.transform as RectTransform, new Vector2(0.5f, 0), Vector2.one);

            //Keybind text


            var bindText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var bindT = bindText.GetComponent<TextMeshProUGUI>();
            bindT.font = fontAsset;
            bindT.fontMaterial = maskingTextMaterial;
            bindT.text = player.actions[prompts[i].action].GetBindingDisplayString();

            bindT.alignment = TextAlignmentOptions.Right;

            bindText.transform.SetParent(holder.transform, false);

            ExpandRectTransform(bindText.transform as RectTransform, Vector2.zero, new Vector2(0.5f, 1f));


            var bindImage = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
            bindImage.transform.SetParent(bindText.transform, false);

            ExpandRectTransform(bindImage.transform as RectTransform, Vector2.zero, Vector2.one);


            bindImage.GetComponent<Image>().material = maskedSpriteMaterial;

        }
    }

    public static void ExpandRectTransform(RectTransform transform, Vector2 anchorMin, Vector2 anchorMax)
    {
        transform.anchorMin = anchorMin;
        transform.anchorMax = anchorMax;
        transform.anchoredPosition = Vector3.zero;
        transform.sizeDelta = Vector3.zero;
    }

}
