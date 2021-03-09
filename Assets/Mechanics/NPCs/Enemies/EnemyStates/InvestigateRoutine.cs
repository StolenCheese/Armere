using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Investigate Routine", menuName = "Game/NPCs/Investigate Routine", order = 0)]
public class InvestigateRoutine : AIStateTemplate
{
	public AnimationCurve investigateRateOverDistance = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);
	[System.NonSerialized] public Collider investigating;
	public override AIState StartState(AIHumanoid c)
	{
		return new Investigate(c, this);
	}

	public InvestigateRoutine Investigate(Collider investigating)
	{
		this.investigating = investigating;
		return this;
	}
}

public class Investigate : AIState<InvestigateRoutine>
{
	public override bool alertOnAttack => true;

	public override bool searchOnEvent => false;

	public override bool investigateOnSight => false;
	Coroutine r;
	public Investigate(AIHumanoid c, InvestigateRoutine t) : base(c, t)
	{
		r = c.StartCoroutine(Routine());
	}
	public override void End()
	{
		c.StopCoroutine(r);
	}
	IEnumerator Routine()
	{
		//Do not re-enter investigate
		//if (c.alert == null || c.alert.gameObject == null)
		//	c.alert = IndicatorsUIController.singleton.CreateAlertIndicator(c.transform, Vector3.up * c.height);

		//c.alert.EnableInvestigate(true);
		//c.alert.EnableAlert(false);

		float investProgress = 0;
		//Try to look at the player long enough to alert
		while (true)
		{
			//c.alert.SetInvestigation(investProgress);

			float visibility = c.ProportionBoundsVisible(t.investigating.bounds);

			if (visibility != 0)
			{
				//can see player
				//Distance is the 0-1 scale where 0 is closestest visiable and 1 is furthest video
				float playerDistance = Mathf.InverseLerp(c.clippingPlanes.x, c.clippingPlanes.y, Vector3.Distance(c.eye.position, t.investigating.transform.position));
				//Invest the player slower if they are further away
				investProgress += Time.deltaTime * t.investigateRateOverDistance.Evaluate(playerDistance) * visibility;
			}
			else
			{
				investProgress -= Time.deltaTime;
			}


			if (investProgress < -0.5f)
			{
				//Cannot see player
				c.ChangeToState(c.defaultState);

				break;
			}
			else if (investProgress >= 1)
			{
				//Seen player
				c.ChangeToState(new EnemyAI.AlertRoutine(c, 1, t.investigating));
				break;
			}


			yield return null;
		}
	}

}