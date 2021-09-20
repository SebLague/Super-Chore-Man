using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CylinderGenerator
{
	public static void CreateMesh(ref Mesh mesh, Vector3[] points, int resolutionU = 10, float thickness = 2)
	{
		List<Vector3> verts = new List<Vector3>();
		List<int> triangles = new List<int>();

		int numCircles = points.Length;

		PathVertex[] v = CalcNormals(points);

		for (int s = 0; s < numCircles; s++)
		{
			float segmentPercent = s / (numCircles - 1f);
			Vector3 centerPos = points[s];

			//Vector3 forward = (s == numCircles - 1) ? (points[s] - points[s - 1]).normalized : (points[s + 1] - points[s]).normalized;
			//Vector3 norm = Vector3.Cross(forward, Vector3.forward).normalized;
			Vector3 forward = v[s].tangent;
			Vector3 norm = v[s].normal;
			Vector3 tangentOrWhatEver = Vector3.Cross(norm, forward).normalized;

			for (int currentRes = 0; currentRes < resolutionU; currentRes++)
			{
				var angle = ((float)currentRes / resolutionU) * (Mathf.PI * 2.0f);

				var xVal = Mathf.Sin(angle) * thickness;
				var yVal = Mathf.Cos(angle) * thickness;

				var point = (norm * xVal) + (tangentOrWhatEver * yVal) + centerPos;
				verts.Add(point);

				//! Adding the triangles
				if (s < numCircles - 1)
				{
					int startIndex = resolutionU * s;
					triangles.Add(startIndex + currentRes);
					triangles.Add(startIndex + (currentRes + 1) % resolutionU);
					triangles.Add(startIndex + currentRes + resolutionU);

					triangles.Add(startIndex + (currentRes + 1) % resolutionU);
					triangles.Add(startIndex + (currentRes + 1) % resolutionU + resolutionU);
					triangles.Add(startIndex + currentRes + resolutionU);
				}

			}
		}

		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}

		mesh.SetVertices(verts);
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateNormals();
	}

	static PathVertex[] CalcNormals(Vector3[] localPoints)
	{
		PathVertex[] verts = new PathVertex[localPoints.Length];
		Vector3 lastRotationAxis = Vector3.up;

		// Loop through the data and assign to arrays.
		for (int i = 0; i < localPoints.Length; i++)
		{
			Vector3 tangent = Vector3.zero;
			Vector3 normal = Vector3.zero;

			if (i == 0)
			{
				tangent = (localPoints[i + 1] - localPoints[i]).normalized;
			}
			else if (i == localPoints.Length - 1)
			{
				tangent = (localPoints[i] - localPoints[i - 1]).normalized;
			}
			else
			{
				tangent = ((localPoints[i + 1] - localPoints[i]).normalized + (localPoints[i] - localPoints[i - 1]).normalized).normalized;
			}


			// Calculate normals
			if (i == 0)
			{
				lastRotationAxis = (Vector3.Dot(tangent, Vector3.up) > 0.5f) ? -Vector3.forward : Vector3.up;
				normal = Vector3.Cross(lastRotationAxis, tangent).normalized;
			}
			else
			{
				// First reflection
				Vector3 offset = (localPoints[i] - localPoints[i - 1]);
				float sqrDst = offset.sqrMagnitude;
				Vector3 r = lastRotationAxis - offset * 2 / sqrDst * Vector3.Dot(offset, lastRotationAxis);
				Vector3 t = verts[i - 1].tangent - offset * 2 / sqrDst * Vector3.Dot(offset, verts[i - 1].tangent);

				// Second reflection
				Vector3 v2 = tangent - t;
				float c2 = Vector3.Dot(v2, v2);

				Vector3 finalRot = r - v2 * 2 / c2 * Vector3.Dot(v2, r);
				Vector3 n = Vector3.Cross(finalRot, tangent).normalized;
				normal = n;
				lastRotationAxis = finalRot;
			}

			verts[i] = new PathVertex();
			verts[i].tangent = tangent;
			verts[i].normal = normal;

		}
		return verts;
	}

	public struct PathVertex
	{
		public Vector3 tangent;
		public Vector3 normal;
	}
}
