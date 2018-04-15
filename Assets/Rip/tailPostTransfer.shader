Shader "Custom/tailPostTransfer" {

	Properties{
		_Hue("Hue", float) = 0
		    _MainTex("", 2D) = "white" {}
	}
	SubShader {
		// COLOR PASS
		Pass {
			Tags{ "LightMode" = "ForwardBase" }
			Cull Off
		
			CGPROGRAM
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

struct Transfer {
  float3 vertex;
  float3 normal;
  float3 tan;
  float2 uv;
  float3 debug;
};

uniform sampler2D _MainTex;

			uniform float _Hue;
			uniform float _transferOffset;
			StructuredBuffer<Transfer> _transferBuffer;

			struct varyings {
				float4 pos 		: SV_POSITION;
				float3 nor 		: TEXCOORD0;
				float2 uv  		: TEXCOORD1;
				float3 eye      : TEXCOORD5;
				float3 worldPos : TEXCOORD6;
				float3 debug    : TEXCOORD7;
				float3 closest    : TEXCOORD8;
				SHADOW_COORDS(2)
			};

			varyings vert(uint id : SV_VertexID) {

				Transfer t= _transferBuffer[id];
				float3 fPos 	= t.vertex;
				float3 fNor 	= t.normal;
				float2 fUV 		= t.uv;
				float2 fTan 	= t.tan;
				float3 fDebug = t.debug;

				varyings o;

				o.pos = mul(UNITY_MATRIX_VP, float4(fPos,1));
				o.worldPos = fPos;
				o.eye = _WorldSpaceCameraPos - fPos;
				o.nor = fNor;
				o.uv =  fUV;
				o.debug = fDebug;

				TRANSFER_SHADOW(o);

				return o;
			}

			float4 frag(varyings v) : COLOR {

				float3 col = v.nor * .5 + .5;

				fixed shadow = SHADOW_ATTENUATION(v) * .5 + .5 ;

				return float4(shadow*float3(1,1,1), 1.);
			}

			ENDCG
		}

		// SHADOW PASS

		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			Fog{ Mode Off }
			ZWrite On
			ZTest LEqual
			Cull Back
			Offset 1, 1

			CGPROGRAM
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
struct Transfer {
  float3 vertex;
  float3 normal;
  float3 tan;
  float2 uv;
  float3 debug;
};

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			StructuredBuffer<Transfer> _transferBuffer;

			v2f vert(appdata_base v, uint id : SV_VertexID)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(_transferBuffer[id].vertex, 1));
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}