using UnityEngine;

public abstract class PostProcessingEffect : ScriptableObject
{
	public Shader shader;
	protected Material material;
	RenderTexture target;

	public virtual RenderTexture Render(RenderTexture source)
	{
		if (material == null)
		{
			material = new Material(shader);
		}

		target = RenderTexture.GetTemporary(source.descriptor);
		RenderEffectToTarget(source, target);
		return target;
	}

	public virtual void Release()
	{
		if (target)
		{
			RenderTexture.ReleaseTemporary(target);
		}
	}

	protected abstract void RenderEffectToTarget(RenderTexture source, RenderTexture target);

}