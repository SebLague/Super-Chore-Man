using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumHead : MonoBehaviour
{

	public Transform ghostModeHandle;
	public Transform mouth;
	public float collisionForce = 10;

	public float ghostEatDst;
	public float ghostEatAngle;
	public float suckObjectForce;

	public SuckLine suckLinePrefab;
	public float maxTimeBetweenLines;
	float nextLineSpawnTime;

	Vector3 normalModeLocalPos;
	Quaternion normalModeLocalRot;
	Pool<SuckLine> pool;


	float ghostModeTransitionT;

	void Start()
	{
		pool = new Pool<SuckLine>(suckLinePrefab, 20);
		normalModeLocalPos = transform.localPosition;
		normalModeLocalRot = transform.localRotation;
	}

	void Update()
	{
		if (Input.GetKey(PlayerController.ShootKey))
		{
			ghostModeTransitionT += Time.deltaTime * 5;
		}
		else
		{
			ghostModeTransitionT -= Time.deltaTime * 5;
		}
		ghostModeTransitionT = Mathf.Clamp01(ghostModeTransitionT);

		float t = Ease.Circular.InOut(ghostModeTransitionT);
		transform.localPosition = Vector3.Lerp(normalModeLocalPos, ghostModeHandle.localPosition, t);
		transform.localRotation = Quaternion.Slerp(normalModeLocalRot, ghostModeHandle.localRotation, t);

		if (InGhostEatingMode)
		{
			if (Time.time > nextLineSpawnTime)
			{
				nextLineSpawnTime = Time.time + Random.value * maxTimeBetweenLines;
				SuckLine line;
				if (pool.TryInstantiate(out line))
				{
					float randomAngle = (Random.value - 0.5f) * ghostEatAngle * Mathf.Deg2Rad;
					float randomDst = Mathf.Lerp(ghostEatDst * 0.25f, ghostEatDst, Random.value);
					float currentAngle = Mathf.Atan2(mouth.forward.z, mouth.forward.x);

					Vector3 randomDir = new Vector3(Mathf.Cos(currentAngle + randomAngle), 0, Mathf.Sin(currentAngle + randomAngle));
					line.transform.position = mouth.position + randomDir * randomDst;
					line.Init(this);
				}
			}

			var cols = Physics.OverlapSphere(mouth.position + mouth.forward * ghostEatDst/2,5);

			foreach (var c in cols) {
				Rigidbody r = null;
				if (c.TryGetComponent<Rigidbody>(out r)) {
					r.AddForce((mouth.position - r.position).normalized * suckObjectForce, ForceMode.Impulse);
				}
			}
		}
	}

	public bool InGhostEatingMode
	{
		get
		{
			return ghostModeTransitionT > 0.75f;
		}
	}

	public bool InDustMode
	{
		get
		{
			return ghostModeTransitionT < 0.1f;
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null)
		{
			if (!collision.gameObject.CompareTag("IgnoreForce"))
			{
				//collision.rigidbody.AddForce((Vector3.up) * collisionForce, ForceMode.VelocityChange);
				collision.rigidbody.AddForceAtPosition((Vector3.up) * collisionForce, collision.contacts[0].point, ForceMode.VelocityChange);
			}
		}
	}
}
