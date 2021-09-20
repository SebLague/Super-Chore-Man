using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PostProcessing/Screen Colour")]
public class ScreenCol : PostProcessingEffect
{

	[Range(0,1)]
	public float red;
	[Range(0,1)]
	public float greyScale;

	protected override void RenderEffectToTarget(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("redStrength", red);
		material.SetFloat("greyscale", greyScale);
		Graphics.Blit(source, destination, material);
	}
}
