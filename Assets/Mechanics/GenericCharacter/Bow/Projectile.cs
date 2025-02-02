using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : SpawnableBody
{
	protected Rigidbody rb;
	protected Collider col;

	protected bool launched = false;
	protected float hitDamage;
	bool collided = false;
	public Vector3EventChannelSO onProjectileHitEventChannel;
	public void InitProjectile(float hitDamage)
	{
		this.hitDamage = hitDamage;
	}

	public void LaunchProjectile(Vector3 velocity)
	{
		rb = GetComponent<Rigidbody>();
		rb.velocity = velocity;
		transform.forward = velocity;
		rb.isKinematic = false;


		//Set initialzed
		launched = true;


		StartCoroutine(EnableCollider());

	}

	IEnumerator EnableCollider()
	{
		yield return null;
		//Enable collider next frame to stop self collision
		col = GetComponent<Collider>();
		col.enabled = true;
	}
	// Update is called once per frame
	void Update()
	{
		if (launched)
			transform.forward = rb.velocity;
		else
			transform.localPosition = Vector3.zero;
	}

	static void DrawCross(Vector3 center, float size, float time = 2)
	{
		Debug.DrawLine(center + Vector3.up * size, center - Vector3.up * size, Color.green, time);
		Debug.DrawLine(center + Vector3.right * size, center - Vector3.right * size, Color.green, time);
		Debug.DrawLine(center + Vector3.forward * size, center - Vector3.forward * size, Color.green, time);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!collision.collider.isTrigger && !collided && launched)
		{
			//Debug.Log($"{ammoName} collided with {other.gameObject.name}", other.gameObject);
			collided = true;
			OnProjectileHit(collision, out bool goodHit);
		}
	}

	public virtual void OnProjectileHit(Collision collision, out bool goodHit)
	{
		Vector3 collisionPoint = collision.contacts[0].point;

		//DrawCross(collisionPoint, 1, 10);
		//		Debug.Log(collision.gameObject.name, collision.gameObject);
		if (collision.gameObject.TryGetComponent<Health>(out var h))
		{
			h.Damage(hitDamage, gameObject);
			goodHit = true;
		}
		else
		{
			goodHit = false;
		}

		onProjectileHitEventChannel?.RaiseEvent(collisionPoint);

		Destroy();
	}
}
