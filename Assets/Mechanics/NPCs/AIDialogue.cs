using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn;
using Armere.UI;
using TMPro;
using System.Linq;
using Armere.Inventory;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(AIHumanoid))]
public class AIDialogue : MonoBehaviour, IDialogue, IInteractable
{

	[Range(0, 360)]
	public float requiredLookAngle = 180;
	public float requiredLookDot => Mathf.Cos(requiredLookAngle);
	bool IInteractable.canInteract { get => enabled; set => enabled = value; }

	public string interactionDescription => "Talk";
	public Transform headPosition;
	public string npcName;


	public IDialogue target;

	public Minigame[] minigames;
	AIHumanoid ai;
	[System.NonSerialized] public Transform[] conversationGroupOverride, walkingPoints, focusPoints;
	public System.Action<IInteractor> onInteract;

	public BuyMenuItem[] buyInventory;

	public string StartNode => target.StartNode;

	public string interactionName => NPCManager.singleton.data[npcName].spokenTo ? npcName : "";

	public Vector3 worldOffset => headPosition.localPosition;

	public AssetReferenceT<YarnProgram> Dialogue => target.Dialogue;

	[System.NonSerialized] public IVariableAddon dialogueAddon;

	private void Start()
	{
		ai = GetComponent<AIHumanoid>();
	}
	public void GoToWalkingPoint(string name, System.Action onComplete = null)
	{
		ai.GoToPosition(GetTransform(walkingPoints, name).position, onComplete);
	}
	public void SetupCommands(DialogueRunner runner)
	{
		target.SetupCommands(runner);
	}
	public Transform GetTransform(Transform[] transforms, string name) => transforms.First(t => t.name == name);
	public Transform GetFocusPoint(string name) => GetTransform(focusPoints, name);
	public void RemoveCommands(DialogueRunner runner)
	{
		target.RemoveCommands(runner);
	}

	public void Interact(IInteractor interactor)
	{
		onInteract?.Invoke(interactor);
	}


}
