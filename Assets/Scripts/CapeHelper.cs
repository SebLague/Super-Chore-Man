using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapeHelper : MonoBehaviour
{

	public Cloth capeSim;
	List<ClothSphereColliderPair> sphereColliderPairs;

	void Awake()
	{
		sphereColliderPairs = new List<ClothSphereColliderPair>(capeSim.sphereColliders);
	}

	public void AddSphereCollider(SphereCollider sphere)
	{
		sphereColliderPairs.Add(new ClothSphereColliderPair(sphere));
		capeSim.sphereColliders = sphereColliderPairs.ToArray();
	}

	public void AddCapsuleCollider(SphereCollider sphereA, SphereCollider sphereB)
	{
		sphereColliderPairs.Add(new ClothSphereColliderPair(sphereA, sphereB));
		capeSim.sphereColliders = sphereColliderPairs.ToArray();
	}
}
