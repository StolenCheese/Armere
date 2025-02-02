﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace Armere.UI
{
	public class UIController : MonoBehaviour
	{

		public UIMenu tabMenu;
		public GameObject buyMenu;
		public WorldIndicator itemIndicator;
		public WorldIndicator npcIndicator;
		public static UIController singleton;

		public Image fadeoutImage;
		public TextMeshProUGUI fadeoutText;
		public GameObject scrollingSelector;

		public GameObject deathScreen;

		public UIBossBar bossBar;

		public BoolEventChannelSO onToggleTabMenuEvent;

		public UnityEvent onSceneChangeBegin;

		public InputReader inputReader;

		public AutoDraggerUI autoDragger;

		private void Awake()
		{
			singleton = this;
			fadeoutImage.gameObject.SetActive(true);
		}


		private void Start()
		{
			onToggleTabMenuEvent.OnEventRaised += SetTabMenu;
			fadeoutImage.gameObject.SetActive(false);
		}
		private void OnDestroy()
		{
			onToggleTabMenuEvent.OnEventRaised -= SetTabMenu;
			if (singleton == this) singleton = null;
		}


		public IEnumerator FullFade(float fadeTime, float time, string text = null)
		{
			fadeTime = Mathf.Clamp(fadeTime, 0, time / 2);
			bool useText = text != null;

			// fadeoutImage.color = Color.clear; //Black with full transparency
			fadeoutImage.gameObject.SetActive(true);

			if (useText)
			{
				fadeoutText.text = text;
				fadeoutText.gameObject.SetActive(true);
			}

			yield return Fade(0, 1, fadeTime, useText);


			float fullyBlackTime = time - fadeTime * 2;

			yield return new WaitForSecondsRealtime(fullyBlackTime);

			yield return Fade(1, 0, fadeTime, useText);

			DisableFadeout();
		}

		public void FadeOut(float fadeTime, string text = null)
		{
			bool useText = text != null;

			fadeoutImage.gameObject.SetActive(true);

			if (useText)
			{
				fadeoutText.text = text;
				fadeoutText.gameObject.SetActive(true);
			}

			StartFade(0, 1, fadeTime, useText);
		}
		public void FadeIn(float fadeTime, bool useText)
		{
			fadeoutImage.gameObject.SetActive(true);
			if (useText)
			{
				fadeoutText.gameObject.SetActive(true);
			}
			StartCoroutine(Fade(1, 0, fadeTime, useText, DisableFadeout));
		}

		//Change alpha
		public IEnumerator Fade(float startAlpha, float endAlpha, float fadeTime, bool useText, System.Action onComplete = null)
		{
			StartFade(startAlpha, endAlpha, fadeTime, useText);
			yield return new WaitForSecondsRealtime(fadeTime);
			onComplete?.Invoke();
		}
		public void StartFade(float startAlpha, float endAlpha, float fadeTime, bool useText)
		{
			fadeoutImage.canvasRenderer.SetAlpha(startAlpha);
			fadeoutImage.CrossFadeAlpha(endAlpha, fadeTime, true);
			//do the same for text
			if (useText)
			{
				fadeoutText.canvasRenderer.SetAlpha(startAlpha);
				fadeoutText.CrossFadeAlpha(endAlpha, fadeTime, true);
			}
		}


		public void DisableFadeout()
		{
			fadeoutImage.gameObject.SetActive(false);
			fadeoutText.gameObject.SetActive(false);
		}



		public void SetTabMenu(bool active)
		{
			if (active)
				tabMenu.OpenMenu();
			else
				tabMenu.CloseMenu();
		}


	}
}