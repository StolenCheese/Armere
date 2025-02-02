using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(WaterController))]
public class WaterAudioController : MonoBehaviour
{
	public AudioSource source;
	public AudioListener listener;
	WaterController c;
	private void Start()
	{
		listener = FindObjectOfType<AudioListener>();
		c = GetComponent<WaterController>();
	}


	private void Update()
	{
		//Find the closest point on the path to the listener
		Vector3 rightPath = c.ClosestPointOnPath(listener.transform.position, -1);
		Vector3 leftPath = c.ClosestPointOnPath(listener.transform.position, 1);
		Vector3 closest = Vector3.Lerp(rightPath, leftPath, VectorMath.InverseLerp(rightPath, leftPath, listener.transform.position));
		source.transform.position = closest;
	}
}
