using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComputeShaderUtility
{

	public static class GaussianBlur
	{
		static ComputeShader blurCompute;

		public static void Blur(RenderTexture texture, int halfBlurSize = 8, float sigma = 5)
		{
			if (blurCompute == null)
			{
				blurCompute = (ComputeShader)Resources.Load("Blur");
			}
			int kernelSize = Mathf.Abs(halfBlurSize) * 2 + 1;
			float[] kernelValues = Calculate1DGaussianKernel(kernelSize, sigma);

			ComputeBuffer kernelValueBuffer = new ComputeBuffer(kernelValues.Length, sizeof(float));
			kernelValueBuffer.SetData(kernelValues);
			RenderTexture horizontalPassTexture = new RenderTexture(texture.descriptor);

			blurCompute.SetBuffer(0, "kernelValues", kernelValueBuffer);
			blurCompute.SetTexture(0, "HorizontalPassTexture", horizontalPassTexture);
			blurCompute.SetTexture(0, "Source", texture);

			blurCompute.SetBuffer(1, "kernelValues", kernelValueBuffer);
			blurCompute.SetTexture(1, "HorizontalPassTexture", horizontalPassTexture);
			blurCompute.SetTexture(1, "Source", texture);

			blurCompute.SetInt("kernelSize", kernelSize);
			blurCompute.SetInt("width", texture.width);
			blurCompute.SetInt("height", texture.height);

			ComputeHelper.Dispatch(blurCompute, texture.width, texture.height, kernelIndex: 0);
			ComputeHelper.Dispatch(blurCompute, texture.width, texture.height, kernelIndex: 1);

			horizontalPassTexture.Release();
			kernelValueBuffer.Release();

		}

		static float CalculateGaussianValue(int x, float sigma)
		{
			float c = 2 * sigma * sigma;
			return Mathf.Exp(-x * x / c) / Mathf.Sqrt(c * Mathf.PI);
		}

		public static float[] Calculate1DGaussianKernel(int kernelSize, float sigma)
		{
			float[] kernelValues = new float[kernelSize];
			float sum = 0;
			for (int i = 0; i < kernelSize; i++)
			{
				kernelValues[i] = CalculateGaussianValue(i - kernelSize / 2, sigma);
				sum += kernelValues[i];
			}

			for (int i = 0; i < kernelSize; i++)
			{
				kernelValues[i] /= sum;
			}

			return kernelValues;
		}
	}
}