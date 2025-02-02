﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Animation Transition Set", menuName = "Game/Animation Transition Set", order = 0)]
public class AnimationTransitionSet : ScriptableObject
{
	[Header("Weapons")]
	public AnimationTransition swingSword = new AnimationTransition("Sword Slash", 0.05f, 0.15f, Layers.BaseLayer);
	public AnimationTransition swingDoubleSword = new AnimationTransition("Double Sword Slash", 0.05f, 0.15f, Layers.BaseLayer);
	public AnimationTransition backSwingSword = new AnimationTransition("Sword Back Slash", 0.05f, 0.15f, Layers.BaseLayer);
	public AnimationTransition backSwingDoubleSword = new AnimationTransition("Double Sword Back Slash", 0.05f, 0.15f, Layers.BaseLayer);
	public AnimationTransition sheathSword = new AnimationTransition("Sheath Sword", 0.05f, 0.15f, Layers.UpperBody);
	public AnimationTransition drawSword = new AnimationTransition("Draw Sword", 0.05f, 0.05f, Layers.UpperBody);

	public AnimationTransition swordWalking = new AnimationTransition("Sword Walking", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition sword180 = new AnimationTransition("Sword 180", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition holdBow = new AnimationTransition("Hold Bow", 0.05f, 0.05f, Layers.UpperBody);
	[Header("Movement")]
	public AnimationTransition strafingMovement = new AnimationTransition("Strafing Movement", 0.05f, 0.05f, Layers.BaseLayer);



	public AnimationTransition freeMovement = new AnimationTransition("Free Movement", 0.05f, 0.05f, Layers.BaseLayer);

	public AnimationTransition stepUp = new AnimationTransition("Step Up", 0.05f, 0.05f, Layers.BaseLayer);


	public AnimationTransition jump = new AnimationTransition("Jump", 0.05f, 0.05f, Layers.BaseLayer);


	public AnimationTransition startSitting = new AnimationTransition("Start Sit", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition stopSitting = new AnimationTransition("Stop Sit", 0.05f, 0.05f, Layers.BaseLayer);


	public AnimationTransition shieldRaise = new AnimationTransition("Raise Shield", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition shieldLower = new AnimationTransition("Lower Shield", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition shieldImpact = new AnimationTransition("Shield Impact", 0.05f, 0.05f, Layers.BaseLayer);

	public AnimationTransition swordBackImpact = new AnimationTransition("Back Impact", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition swordFrontImpact = new AnimationTransition("Front Impact", 0.05f, 0.05f, Layers.BaseLayer);

	public AnimationTransition shieldSurf = new AnimationTransition("Surfing", 0.05f, 0.05f, Layers.BaseLayer);

	[Header("Ladders")]
	public AnimationTransition ladderClimb = new AnimationTransition("Climbing Ladder", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition stepDownFromLadder = new AnimationTransition("Step Down Exit", 0.05f, 0.05f, Layers.BaseLayer);
	public AnimationTransition climbUpFromLadder = new AnimationTransition("Climb Up Exit", 0.05f, 0.05f, Layers.BaseLayer);

	[Header("Reactions")]
	public AnimationTransition surprised = new AnimationTransition("Surprised", 0.05f, 0.05f, Layers.BaseLayer);


	[Header("Animator Variables")]
	public AnimatorVariable vertical = new AnimatorVariable("InputVertical");
	public AnimatorVariable horizontal = new AnimatorVariable("InputHorizontal");
	public AnimatorVariable walkingSpeed = new AnimatorVariable("WalkingSpeed");
	public AnimatorVariable isGrounded = new AnimatorVariable("IsGrounded");
	public AnimatorVariable verticalVelocity = new AnimatorVariable("VerticalVelocity");
	public AnimatorVariable groundDistance = new AnimatorVariable("GroundDistance");
	public AnimatorVariable animationSpeed = new AnimatorVariable("AnimSpeed");

}
