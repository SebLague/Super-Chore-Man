using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComputeShaderUtility;
using UnityEngine.Rendering;


public class Ghost : MonoBehaviour
{
	public float moveSpeed;
	public float attackDstThreshold;
	public float attackLungeDst;
	public float attackForce;
	public float attackCooldownTime;
	public Color attackCol;
	public float shieldDuration;

	[Header("Particle settings")]
	public ComputeShader ghostCompute;
	public int numParticles;
	public Material material;
	public Mesh mesh;
	public float size;
	public float speedMultiplier = 5;
	public float orbitRadiusMultiplier = 1;
	public float moveToTargetSpeed = 5;
	public float attackLungeSpeed = 10;


	Material materialInstance;
	ComputeBuffer particleBuffer;
	ComputeBuffer positionBuffer;
	ComputeBuffer argsBuffer;

	VacuumHead vacuumHead;
	PlayerController player;
	bool attacking;
	float attackStartTime;
	bool dying;
	float deathT;
	float shieldT = 1;
	float timeSinceSpawn;

	void Start()
	{
		player = FindObjectOfType<PlayerController>();
		vacuumHead = FindObjectOfType<VacuumHead>();
		ghostCompute = ComputeShader.Instantiate(ghostCompute);
		materialInstance = new Material(material);
		ComputeHelper.CreateStructuredBuffer<Particle>(ref particleBuffer, numParticles);
		ComputeHelper.CreateStructuredBuffer<Vector4>(ref positionBuffer, numParticles);

		Particle[] particles = new Particle[numParticles];
		for (int i = 0; i < numParticles; i++)
		{
			Vector3 startPos = transform.position + Random.insideUnitSphere * 0.1f;
			Vector3 startVelocity = Random.insideUnitSphere;
			Particle p = new Particle();
			p.position = transform.position;

			const float minRadius = 0.1f;
			const float maxRadius = 1;

			float tx = Mathf.Min(Mathf.Min(Mathf.Min(Random.value, Random.value), Random.value), Random.value);
			float ty = Mathf.Min(Mathf.Min(Mathf.Min(Random.value, Random.value), Random.value), Random.value);
			p.ellipse = new Vector2(Mathf.Lerp(minRadius, maxRadius, tx), Mathf.Lerp(minRadius, maxRadius, ty));

			float t = Mathf.InverseLerp(minRadius * 2, maxRadius * 2, p.ellipse.x + p.ellipse.y);
			p.speed = Mathf.Lerp(0.1f, 1, 1 - t);

			p.iHat = Random.insideUnitSphere.normalized;
			p.jHat = Vector3.Cross(p.iHat, Random.insideUnitSphere.normalized).normalized;
			//p.iHat = Vector3.right;
			//p.jHat = Vector3.forward;
			particles[i] = p;
		}

		particleBuffer.SetData(particles);
		ghostCompute.SetBuffer(0, "particles", particleBuffer);
		ghostCompute.SetBuffer(0, "positions", positionBuffer);

		materialInstance.SetBuffer("positionBuffer", positionBuffer);

		// Create args buffer
		uint[] args = new uint[5];
		args[0] = (uint)mesh.GetIndexCount(0);
		args[1] = (uint)numParticles;
		args[2] = (uint)mesh.GetIndexStart(0);
		args[3] = (uint)mesh.GetBaseVertex(0);
		args[4] = 0; // offset

		argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
		argsBuffer.SetData(args);
	}

	void Update()
	{
		timeSinceSpawn += Time.deltaTime;

		HandleMovementAndAttacks();
		HandleDeath();
		UpdateParticles();
	}

	void HandleMovementAndAttacks()
	{
		if (dying)
		{
			transform.position = vacuumHead.mouth.position;
		}
		else if (attacking)
		{
			if (Time.time > attackStartTime + attackCooldownTime)
			{
				attacking = false;
			}
		}
		else
		{
			Vector2 playerPos2D = XZ(player.transform.position);
			float targetHeight = player.transform.position.y + 0.15f;

			Vector3 targetPos = new Vector3(playerPos2D.x, targetHeight, playerPos2D.y);
			Vector3 dirToPlayer = (targetPos - transform.position).normalized;
			transform.position += dirToPlayer * Time.deltaTime * moveSpeed;


			float dstToPlayer = (XZ(transform.position) - playerPos2D).magnitude;
			if (dstToPlayer < attackDstThreshold)
			{

				attacking = true;
				Vector3 lungeEndPos = targetPos + dirToPlayer * attackLungeDst;
				lungeEndPos.y = targetHeight + 0.5f;
				transform.position = lungeEndPos;
				attackStartTime = Time.time;

				player.GhostHit(new Vector3(dirToPlayer.x, 0, dirToPlayer.z) * attackForce);
				//player.rb.AddForce((Vector3.up + dirToPlayer * 0.5f) * attackForce, ForceMode.VelocityChange);
			}
		}
	}

	void HandleDeath()
	{
		if (dying)
		{
			deathT += Time.deltaTime;
			if (deathT > 1)
			{
				deathT = 0;
				Destroy(gameObject);
			}
		}
		else if (vacuumHead.InGhostEatingMode)
		{
			Vector2 vacuumPos2D = new Vector2(vacuumHead.mouth.position.x, vacuumHead.mouth.position.z);
			Vector2 offsetToGhost2D = new Vector2(transform.position.x, transform.position.z) - vacuumPos2D;
			float dst = offsetToGhost2D.magnitude;
			float angle = Vector2.Angle(XZ(vacuumHead.mouth.forward), offsetToGhost2D.normalized);

			if (dst < vacuumHead.ghostEatDst && angle < vacuumHead.ghostEatAngle / 2)
			{
				shieldT -= Time.deltaTime / shieldDuration;
				if (shieldT <= 0)
				{
					dying = true;
				}
			}
		}
		else
		{
			shieldT = 1;
		}
	}

	void UpdateParticles()
	{

		float particleMoveSpeed = moveToTargetSpeed;
		float spawnT = Mathf.Min(1, timeSinceSpawn);

		if (dying)
		{
			particleMoveSpeed = attackLungeSpeed;
		}
		else if (attacking)
		{
			particleMoveSpeed = attackLungeSpeed;
			materialInstance.color = Color.Lerp(materialInstance.color, attackCol, Time.deltaTime * 5);
		}
		else
		{
			materialInstance.color = Color.Lerp(materialInstance.color, material.color, Time.deltaTime * 2);
		}
		//materialInstance.color = material.color;
		ghostCompute.SetFloat("deltaTime", Time.deltaTime);
		ghostCompute.SetVector("attractorPos", transform.position);
		ghostCompute.SetInt("numParticles", numParticles);
		ghostCompute.SetFloat("size", size * Mathf.Lerp(1, 0, deathT) * spawnT);
		ghostCompute.SetVector("target", transform.position);
		ghostCompute.SetFloat("speedMultiplier", speedMultiplier * (deathT + 1));
		ghostCompute.SetFloat("orbitRadiusMultiplier", Mathf.Lerp(orbitRadiusMultiplier, 0, deathT) * spawnT);
		ghostCompute.SetFloat("time", Time.time);
		ghostCompute.SetFloat("moveToTargetSpeed", particleMoveSpeed);

		ComputeHelper.Dispatch(ghostCompute, numParticles, 1, 1, 0);

		Graphics.DrawMeshInstancedIndirect(mesh, 0, materialInstance, new Bounds(Vector3.zero, Vector3.one * 1000), argsBuffer);
	}

	void OnDestroy()
	{
		ComputeHelper.Release(particleBuffer, positionBuffer, argsBuffer);
	}

	public struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public Vector2 ellipse;
		public float speed;
		public Vector3 iHat;
		public Vector3 jHat;
	}

	static Vector2 XZ(Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

}
