
using static UnityEngine.Mathf;

// All functions take a time value 't' between 0 and 1 and return the eased result.

// Easing direction:
// In: starts slow and accelerates
// Out: starts fast and decelerates
// InOut: starts slow, speeds up in the middle, and slows down at the end

// Thanks to www.easings.net for many of the functions used here

public static class Ease
{

	public enum EaseDirection
	{
		EaseIn,
		EaseOut,
		EaseInOut
	}

	/// Cubic easing functions
	public class Cubic
	{

		public static float Ease(float t, EaseDirection direction)
		{
			switch (direction)
			{
				case EaseDirection.EaseIn:
					return In(t);
				case EaseDirection.EaseOut:
					return Out(t);
				case EaseDirection.EaseInOut:
					return InOut(t);
			}

			return t;
		}

		public static float In(float t)
		{
			t = Clamp01(t);
			return Pow(t, 3);
		}

		public static float Out(float t)
		{
			t = Clamp01(t);
			return 1 - Pow(1 - t, 3);
		}

		public static float InOut(float t)
		{
			t = Clamp01(t);
			return (t < 0.5f) ? 4 * t * t * t : 1 - Pow(-2 * t + 2, 3) / 2;
		}

	}

	public class Circular
	{
		public static float In(float t)
		{
			t = Clamp01(t);
			return 1 - Sqrt(1 - t * t);
		}

		public static float Out(float t)
		{
			t = Clamp01(t);
			return Sqrt(1 - Pow(t - 1, 2));
		}

		public static float InOut(float t)
		{
			if (t < 0.5f)
			{
				return (1 - Sqrt(1 - Pow(2 * t, 2))) / 2;
			}
			return (Sqrt(1 - Pow(-2 * t + 2, 2)) + 1) / 2;
		}
	}

	public class Elastic
	{
		public static float Out(float t)
		{
			const float c4 = (2 * PI) / 3;
			t = Clamp01(t);
			return Pow(2, -10 * t) * Sin((t * 10 - 0.75f) * c4) + 1;
		}



	}
}