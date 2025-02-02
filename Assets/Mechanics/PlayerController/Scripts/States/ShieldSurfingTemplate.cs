using UnityEngine;

namespace Armere.PlayerController
{
	[CreateAssetMenu(menuName = "Game/PlayerController/Shield Surfing")]
	public class ShieldSurfingTemplate : MovementStateTemplate
	{

		[Header("Shield Surfing")]
		public float turningTorqueForce;
		public float minSurfingSpeed;
		public float directionChangeSpeed;
		public float forwardForce = 10f;
		public float friction = 0.05f;
		public float jumpForce = 4f;
		public float groundStickForce = 10f;
		public SlideAnchor slideAnchor;
		public override MovementState StartState(PlayerMachine c)
		{
			return new ShieldSurfing(c, this);
		}
	}
}