using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostProcessingManager : MonoBehaviour
{

	public PostProcessingEffect[] effects;

	void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		RenderTexture lastRenderedImage = source;

		for (int i = 0; i < effects.Length; i++)
		{
			if (effects[i])
			{
				lastRenderedImage = effects[i].Render(lastRenderedImage);
			}
		}
		Graphics.Blit(lastRenderedImage, target);

		for (int i = 0; i < effects.Length; i++)
		{
			if (effects[i])
			{
				effects[i].Release();
			}
		}
	}
}
