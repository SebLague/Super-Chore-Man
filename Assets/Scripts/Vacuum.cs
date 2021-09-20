using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Vacuum : MonoBehaviour
{

	public Transform hoseHeadConnectionPoint;
	public PathCreator hosePath;

	public float hoseThickness = 1;
	public MeshFilter hoseMeshFilter;
	float hoseLength;

	public Transform hoseCapeColliderHolder;
	SphereCollider[] hoseCapeColliders;

	const int numHosePoints = 40;
	const int numHoseCapeColliders = numHosePoints / 4;

	public float w1;
	public float w2;

	Rigidbody rb;
	Vector3[] hosePointsLocal;
	Vector3[] hosePointsWorld;

	IK ikSolver;
	Mesh hoseMesh;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		ikSolver = new IK();

		hosePointsLocal = new Vector3[numHosePoints];
		hosePointsWorld = new Vector3[numHosePoints];
		for (int i = 0; i < numHosePoints; i++)
		{
			float t = i / (numHosePoints - 1f);
			Vector3 hosePointWorld = hosePath.path.GetPointAtTime(1 - t, EndOfPathInstruction.Stop);
			hosePointsLocal[i] = hoseHeadConnectionPoint.InverseTransformPoint(hosePointWorld);
		}

		for (int i = 0; i < numHosePoints - 1; i++)
		{
			hoseLength += Vector3.Distance(hosePointsLocal[i], hosePointsLocal[i + 1]);
		}

		// Hose-cape colliders
		hoseCapeColliders = new SphereCollider[numHoseCapeColliders];

		for (int i = 0; i < numHoseCapeColliders; i++)
		{
			var g = new GameObject("Collider " + i);
			g.layer = hoseCapeColliderHolder.gameObject.layer;
			g.transform.parent = hoseCapeColliderHolder;
			var sphereCol = g.AddComponent<SphereCollider>();
			sphereCol.radius = hoseThickness * 1.5f;
			//sphereCol.isTrigger = true;
			hoseCapeColliders[i] = sphereCol;
		}
		CapeHelper cape = FindObjectOfType<CapeHelper>();
		for (int i = 0; i < numHoseCapeColliders - 1; i++)
		{
			cape.AddCapsuleCollider(hoseCapeColliders[i], hoseCapeColliders[i + 1]);
		}
	}

	Vector3 v;
	Quaternion r;
	bool updateRot;

	void Update()
	{
		Hose();
		Follow();

	}

	void FixedUpdate()
	{
		rb.AddForce(v, ForceMode.VelocityChange);
		if (updateRot)
		{
			rb.rotation = Quaternion.Slerp(rb.rotation, r, Time.deltaTime * 2);
		}
	}

	void Follow()
	{
		Vector3 offset = hosePointsWorld[hosePointsWorld.Length - 1] - hosePath.transform.position;

		float dst = -(offset).magnitude;
		if (Mathf.Abs(dst) > 0.01f)
		{
			rb.MovePosition(rb.position + offset);
		}

		float hoseDstBetweenBodyAndHead = Vector3.Distance(hosePath.transform.position, hoseHeadConnectionPoint.position);
		float bodyFollowHeadT = hoseDstBetweenBodyAndHead / hoseLength;
		float followStrength = Mathf.Pow(Mathf.InverseLerp(0.7f, 1, bodyFollowHeadT), 3);
		v = new Vector3(offset.x, 0, offset.z).normalized * followStrength;
		//rb.AddForce(new Vector3(offset.x, 0, offset.z).normalized * followStrength, ForceMode.VelocityChange);
		updateRot = false;
		if (followStrength > 0f)
		{
			updateRot = true;
			float moveAngle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
			r = Quaternion.Euler(Vector3.up * moveAngle);
			//rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.Euler(Vector3.up * moveAngle), Time.deltaTime * 2);
		}
	}

	void Hose()
	{
		for (int i = 0; i < numHosePoints; i++)
		{
			hosePointsWorld[i] = hoseHeadConnectionPoint.TransformPoint(hosePointsLocal[i]);
		}

		ikSolver.w1 = w1;
		ikSolver.w2 = w2;
		ikSolver.useDirBias = true;
		ikSolver.endBiasDir = hoseHeadConnectionPoint.up;
		ikSolver.startBiasDir = hosePath.transform.up;
		ikSolver.Solve(hosePointsWorld, hosePath.transform.position);

		for (int i = 0; i < numHosePoints; i++)
		{
			hosePointsLocal[i] = hoseHeadConnectionPoint.InverseTransformPoint(hosePointsWorld[i]);
		}

		CylinderGenerator.CreateMesh(ref hoseMesh, hosePointsWorld, 10, hoseThickness);
		hoseMeshFilter.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); // mesh is in worldspace
		hoseMeshFilter.mesh = hoseMesh;

		// Colliders
		int skipCount = numHosePoints / numHoseCapeColliders;
		for (int i = 0; i < numHoseCapeColliders; i++)
		{
			hoseCapeColliders[i].transform.position = hosePointsWorld[i * skipCount];
		}
	}

	void OnDrawGizmos()
	{
		if (Application.isPlaying && hosePointsLocal != null)
		{
			for (int i = 0; i < numHosePoints; i++)
			{
				Gizmos.DrawSphere(hosePointsWorld[i], 0.05f);
			}
		}
	}


	void OnValidate()
	{
		if (hosePath != null)
		{
			hosePath.bezierPath.SetPoint(0, Vector3.zero);
			hosePath.bezierPath.SetPoint(hosePath.bezierPath.NumPoints - 1, hosePath.transform.InverseTransformPoint(hoseHeadConnectionPoint.position));
		}
	}


}
