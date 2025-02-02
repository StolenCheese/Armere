using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;

namespace Armere.PlayerController
{

	/// <summary> Same as walking except uses a navmeshagent to control the player

	public class AutoWalking : MovementState<AutoWalkingTemplate>
	{
		public override string StateName => "Auto-Walking";
		NavMeshAgent agent;
		float oldFriction;

		public AutoWalking(PlayerMachine c, AutoWalkingTemplate t) : base(c, t)
		{
		}

		public override void Start()
		{
			agent = c.gameObject.AddComponent<NavMeshAgent>();
			agent.radius = c.collider.radius;
			agent.height = c.collider.height;
			agent.stoppingDistance = 0.1f;
			oldFriction = c.collider.material.dynamicFriction;
			c.collider.material.dynamicFriction = 1;
		}

		public IEnumerator WaitForAgent()
		{
			yield return WaitForAgent(agent.stoppingDistance * 2 + 0.01f);
		}

		public IEnumerator WaitForAgent(float targetDistance)
		{
			yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < targetDistance);
		}

		public override void End()
		{
			MonoBehaviour.Destroy(agent);
			c.collider.material.dynamicFriction = oldFriction;
		}

		public void WalkTo(Vector3 position)
		{
			agent.SetDestination(position);
		}

		public override void Update()
		{

			animator.SetFloat(c.transitionSet.vertical.id, agent.velocity.magnitude);
			animator.SetFloat(c.transitionSet.horizontal.id, 0f);
		}
	}

}