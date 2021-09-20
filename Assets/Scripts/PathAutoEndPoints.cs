using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathAutoEndPoints : MonoBehaviour
{

	public Transform origin;
	public Transform target;
	public bool runInPlayMode;

	public PathCreation.PathCreator pathCreator;


	void Update()
	{

		if (runInPlayMode || !Application.isPlaying && pathCreator)
		{
			if (origin)
			{
				pathCreator.bezierPath.SetPoint(0, transform.InverseTransformPoint(origin.position));
			}
			if (target)
			{
				pathCreator.bezierPath.SetPoint(pathCreator.bezierPath.NumPoints - 1, transform.InverseTransformPoint(target.position));
			}
		}
	}
}
