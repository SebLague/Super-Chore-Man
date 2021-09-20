using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

	public PlayerController player;
	public float height = 11;
	public float smoothTime;
	Vector3 smoothV;

	void Start()
	{

	}

	void LateUpdate()
	{
		Vector3 targetPos = new Vector3(player.rb.position.x, height, player.rb.position.z);
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothV, smoothTime);
	}
}
