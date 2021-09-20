using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComputeShaderUtility;
using UnityEngine.Rendering;
public class DustManager : MonoBehaviour
{

	public ComputeShader dustCompute;
	public int numParticles;
	public Material instancedMaterial;
	public Mesh mesh;
	public float size;
	public Transform vacuumMouth;
	public float attractRadius = 1;
	public float attractForce = 10;
	public Vector2 spawnRegion;
	public Texture2D spawnMap;
	public bool updateProgress;

	ComputeBuffer particleBuffer;
	ComputeBuffer positionBuffer;
	ComputeBuffer numParticlesConsumedBuffer;
	ComputeBuffer argsBuffer;

	const int InitDustKernel = 0;
	const int UpdateDustKernel = 1;
	AsyncGPUReadbackRequest readbackRequest;
	VacuumHead vacuumHead;

	void Start()
	{
		vacuumHead = FindObjectOfType<VacuumHead>();
		ComputeHelper.CreateStructuredBuffer<Particle>(ref particleBuffer, numParticles);
		ComputeHelper.CreateStructuredBuffer<Vector4>(ref positionBuffer, numParticles);

		Particle[] particles = new Particle[numParticles];
		for (int i = 0; i < numParticles; i++)
		{

			particles[i] = new Particle() { position = Random.insideUnitSphere * 10, velocity = Vector3.zero, alpha = 1 };

		}

		particleBuffer.SetData(particles);
		dustCompute.SetBuffer(UpdateDustKernel, "particles", particleBuffer);
		dustCompute.SetBuffer(UpdateDustKernel, "positions", positionBuffer);
		
		// Init dust particle positions
		dustCompute.SetTexture(InitDustKernel, "SpawnMap", spawnMap);
		dustCompute.SetBuffer(InitDustKernel, "particles", particleBuffer);
		dustCompute.SetBuffer(InitDustKernel, "positions", positionBuffer);
		dustCompute.SetInt("numParticles", numParticles);
		ComputeHelper.Dispatch(dustCompute, numParticles, 1, 1, InitDustKernel);

		// Create args buffer
		uint[] args = new uint[5];
		args[0] = (uint)mesh.GetIndexCount(0);
		args[1] = (uint)numParticles;
		args[2] = (uint)mesh.GetIndexStart(0);
		args[3] = (uint)mesh.GetBaseVertex(0);
		args[4] = 0; // offset

		argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
		argsBuffer.SetData(args);

		ComputeHelper.CreateStructuredBuffer<uint>(ref numParticlesConsumedBuffer, 1);
		numParticlesConsumedBuffer.SetData(new uint[] { 0 });
		dustCompute.SetBuffer(UpdateDustKernel, "numParticlesConsumed", numParticlesConsumedBuffer);

		RequestAsyncReadback();

		instancedMaterial.SetBuffer("positionBuffer", positionBuffer);
	}

	void RequestAsyncReadback()
	{
		readbackRequest = AsyncGPUReadback.Request(numParticlesConsumedBuffer);
	}

	void Update()
	{
		dustCompute.SetFloat("deltaTime", Time.deltaTime);
		dustCompute.SetVector("attractorPos", transform.position);
		dustCompute.SetInt("numParticles", numParticles);
		dustCompute.SetFloat("size", size);
		dustCompute.SetVector("attractPos", vacuumMouth.position);
		dustCompute.SetVector("xAxis", vacuumMouth.right);
		dustCompute.SetFloat("attractRadius", attractRadius);
		dustCompute.SetFloat("attractForce", (vacuumHead.InDustMode) ? attractForce : 0);
		ComputeHelper.Dispatch(dustCompute, numParticles, 1, 1, UpdateDustKernel);

		Graphics.DrawMeshInstancedIndirect(mesh, 0, instancedMaterial, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer);

		if (updateProgress && readbackRequest.done)
		{
			uint n = readbackRequest.GetData<uint>()[0];
			//Debug.Log(n + " / " + numParticles + "  " + (n / (float)numParticles) * 100 + "%");//
			RequestAsyncReadback();
		}
	}

	void OnDestroy()
	{
		ComputeHelper.Release(particleBuffer, positionBuffer, argsBuffer, numParticlesConsumedBuffer);
	}

	public struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public float alpha;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Vector3.zero, spawnRegion);
	}
}
