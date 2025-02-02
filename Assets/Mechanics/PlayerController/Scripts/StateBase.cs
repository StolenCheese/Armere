using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Armere.PlayerController
{

	public abstract class MovementState : State<MovementState, PlayerMachine, MovementStateTemplate>
	{
		public bool updateWhilePaused = false;
		public bool canBeTargeted = true;
		public PlayerController c => machine.c;
		public Transform transform => c.transform;
		public GameObject gameObject => c.gameObject;
		public Animator animator => c.animator;
		public abstract char stateSymbol { get; }


		protected GameCameras cameras => GameCameras.s;
		public MovementState(PlayerMachine c) : base(c)
		{

		}

		public virtual void OnAnimatorIK(int layerIndex) { }
		public virtual void OnTriggerEnter(Collider other) { }
		public virtual void OnTriggerExit(Collider other) { }
	}
	public abstract class MovementState<TemplateT> : MovementState where TemplateT : MovementStateTemplate
	{
		public override char stateSymbol => t.stateSymbol;
		public readonly TemplateT t;


		protected MovementState(PlayerMachine c, TemplateT t) : base(c)
		{
			this.t = t;
		}
	}


	public abstract class MovementStateTemplate : StateTemplate<MovementState, PlayerMachine, MovementStateTemplate>
	{

	}

}