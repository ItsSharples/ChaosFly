// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Instanced/Particle" {
	Properties {
		_particleColour("Colour of the Particle", Color) = (1,1,1,1)
	}
	SubShader {

		Pass {
			Tags
			{
				"LightMode" = "ForwardBase"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				//"Queue" = "Opaque"
			}
			Lighting Off
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			Fog { Mode Off }

			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members col)
//#pragma exclude_renderers d3d11

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

			#include "UnityCG.cginc"
			
			struct Particle {
				float3 position;
				float4 colour;
				float lifeT;
			};

			StructuredBuffer<Particle> Particles;
			float size;
			float stretch;
			float scale = 1.0f;
			fixed4 _particleColour;
			fixed4 _trailColour;

			int stage;
			int numParticles;


			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 col : COLOR0;
			};

			v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
				Particle particle = Particles[instanceID];

				float3 forward = normalize(particle.position);;//float3(0.0f, 0, 1);// normalize(particle.velocity);
				float3 up = normalize(particle.position);
				float3 right = normalize(cross(forward, up));
				forward = -normalize(cross(up, right));
			
				float4x4 mat = float4x4(
					float4(1, 0, 0, 0),
					float4(0, 1, 0, 0),
					float4(0, 0, 1, 0),
					float4(0,0,0, 1)
				);
				float lifeScale = 1 - 256 * pow(particle.lifeT-0.5, 8);
				float4 localPosition = v.vertex * size;// * lifeScale;

				float3 worldPosition = (particle.position + mul(mat, localPosition)) * scale;

				v2f o;
				//o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				//o.normal = v.normal;

				o.pos = UnityObjectToClipPos(worldPosition);

				bool isCurrent = (instanceID < numParticles);

				o.col = lerp(particle.colour, float4(1,1,1,1), particle.lifeT);// isCurrent ? _particleColour : _trailColour;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return i.col;
			}

			ENDCG
		}
	}
}