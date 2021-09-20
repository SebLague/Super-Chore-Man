Shader "Instanced/GhostShader" {
	Properties {
		_Color ("Colour", Color) = (1,1,1,1)
	}
	SubShader {

		Pass {

			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
			ZWrite On
			Lighting Off
			Fog { Mode Off }

			//Blend SrcAlpha OneMinusSrcAlpha 
			Blend OneMinusDstColor One
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma target 4.5

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"

			float4 _Color;

		#if SHADER_TARGET >= 45
			StructuredBuffer<float4> positionBuffer;
		#endif

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
			#if SHADER_TARGET >= 45
				float4 data = positionBuffer[instanceID];
			#else
				float4 data = 0;
			#endif

				float3 localPosition = v.vertex.xyz * data.w;
				float3 worldPosition = data.xyz + localPosition;

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				o.uv = v.texcoord;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 offsetFromCentre = 0.5 - i.uv;

				//float alpha = max(0, 1 - (length(offsetFromCentre) * 2));
				float alpha = 1;
				fixed4 col = float4(_Color.rgb, alpha * _Color.a);

				return col;
			}

			ENDCG
		}
	}
}