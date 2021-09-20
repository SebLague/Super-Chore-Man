using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK
{

	public Vector3 startBiasDir;
	public Vector3 endBiasDir;
	public bool useDirBias;
	public float w1;
	public float w2;

	bool hasCalculatedLengths;
	float[] lengths;
	float totalLength;

	public void Solve(Vector3[] positions, Vector3 target)
	{
		//Vacuum hardcoded constraints
		//positions[1] = positions[0] + Vector3.up * lengths[0];
		//positions[positions.Length - 2] = positions[positions.Length - 1] + Vector3.up * lengths[positions.Length - 2];

		// Calculate lengths of each bone:
		if (!hasCalculatedLengths)
		{
			hasCalculatedLengths = true;
			lengths = new float[positions.Length - 1];
			totalLength = 0;
			for (int i = 0; i < positions.Length - 1; i++)
			{
				lengths[i] = (positions[i + 1] - positions[i]).magnitude;
				totalLength += lengths[i];
			}

		}

		// If target is out of reach, set all bones in straight line towards the target:
		bool targetOutOfReach = (target - positions[0]).magnitude >= totalLength;
		if (targetOutOfReach)
		{
			Vector3 dirToTarget = (target - positions[0]).normalized;
			for (int i = 1; i < positions.Length; i++)
			{
				positions[i] = positions[i - 1] + dirToTarget * lengths[i - 1];
			}
		}
		// Target can be reached, so use ik solver:
		else
		{
			SolvePass(positions, lengths, target, positions[0], true, 0);
		}
	}

	void SolvePass(Vector3[] positions, float[] lengths, Vector3 anchor, Vector3 reachTarget, bool forwardPass, int iteration)
	{
		const int maxIterations = 100;
		const float acceptableTargetError = 0.01f;
		const float sqrAcceptableTargetError = acceptableTargetError * acceptableTargetError;

		// Solve use FABRIK (Forward and Backward Reaching Inverse Kinematics)
		// Reverse arrays to alternate between backward and forwards passes:
		System.Array.Reverse(positions);
		System.Array.Reverse(lengths);

		positions[0] = anchor;
		float iterationT = iteration / (maxIterations - 1f);

		for (int i = 0; i < positions.Length - 1; i++)
		{
			Vector3 dir = (positions[i + 1] - positions[i]).normalized;

			if (useDirBias)
			{
				float t = i / (positions.Length - 2f);
				float vacuumVertStr = Mathf.Exp(-t * w1) * Mathf.Exp(-iterationT * 10);
				float negVacuumVertStr = (1 - Mathf.Exp(-t * w2)) * Mathf.Exp(-iterationT * 10);
				Vector3 biasDir = (forwardPass) ? startBiasDir : endBiasDir;
				dir = (biasDir * vacuumVertStr + dir + negVacuumVertStr * Vector3.down).normalized;
			}

			positions[i + 1] = positions[i] + dir * lengths[i];
			positions[i + 1].y = Mathf.Max(0, positions[i+1].y);
		}

		// If on backward pass, check if should terminate
		if (!forwardPass)
		{
			float sqrDstToTarget = (positions[positions.Length - 1] - reachTarget).sqrMagnitude;
			if (sqrDstToTarget <= sqrAcceptableTargetError)
			{
				//print("Acceptable Result");
				return;
			}
			if (iteration >= maxIterations)
			{
				//print("Max iterations reached");
				return;
			}


		}

		SolvePass(positions, lengths, reachTarget, anchor, !forwardPass, iteration + 1);

	}

}
