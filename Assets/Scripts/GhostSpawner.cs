using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
	public Ghost ghostPrefab;
	public Vector2 spawnPerimeter;

	public float initialDelay;
	public float delayBetweenSpawnsStart;
	public float delayBetweenSpawnsEnd;
	public float timeToMaxDifficulty;

	float nextSpawnTime;

	void Start()
	{
		nextSpawnTime = initialDelay;
	}

	void Update()
	{
		if (Time.time > nextSpawnTime)
		{
			Spawn();
			float difficultyT = Mathf.Clamp01((Time.time - initialDelay) / timeToMaxDifficulty);
			nextSpawnTime = Time.time + Mathf.Lerp(delayBetweenSpawnsStart, delayBetweenSpawnsEnd, difficultyT);
		}
	}

	void Spawn()
	{
		bool spawnOnX = Random.value > 0.5f;
		float randX = (spawnOnX) ? Random.value - 0.5f : Mathf.Sign(Random.value - 0.5f) * 0.5f;
		float randY = (!spawnOnX) ? Random.value - 0.5f : Mathf.Sign(Random.value - 0.5f) * 0.5f;
		float spawnX = randX * spawnPerimeter.x;
		float spawnY = randY * spawnPerimeter.y;
		Vector3 pos = new Vector3(spawnX, 0.5f, spawnY) + transform.position;
		Instantiate(ghostPrefab, pos, Quaternion.identity);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, new Vector3(spawnPerimeter.x, 0, spawnPerimeter.y));
	}
}
