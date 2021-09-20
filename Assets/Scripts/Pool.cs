using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : PooledItem
{
	Queue<T> available;
	int numTimesUnavailableWhenRequested;

	public Pool(T prefab, int count)
	{
		available = new Queue<T>();

		GameObject holder = new GameObject($"Pool ({typeof(T)})");
		for (int i = 0; i < count; i++)
		{
			T entity = GameObject.Instantiate(prefab);
			entity.transform.SetParent(holder.transform);
			entity.onDestroy += (x) => available.Enqueue(x as T);
			available.Enqueue(entity);
			entity.gameObject.SetActive(false);
		}
	}

	public bool IsAvailable()
	{
		return available.Count > 0;
	}

	public bool TryInstantiate(out T instantiateEntity)
	{
		return TryInstantiate(out instantiateEntity, Vector3.zero, Quaternion.identity);
	}

	public bool TryInstantiate(out T instantiateEntity, Vector3 position, Quaternion rotation)
	{

		if (available.Count > 0)
		{
			instantiateEntity = available.Dequeue();
			instantiateEntity.transform.SetPositionAndRotation(position, rotation);
			instantiateEntity.gameObject.SetActive(true);
			return true;
		}

		numTimesUnavailableWhenRequested++;
		instantiateEntity = null;
		return false;
	}
}

public abstract class PooledItem : MonoBehaviour
{

	public event System.Action<PooledItem> onDestroy;

	protected void DestroyPooled()
	{
		Reset();
		gameObject.SetActive(false);
		onDestroy?.Invoke(this);
	}

	protected abstract void Reset();
}
