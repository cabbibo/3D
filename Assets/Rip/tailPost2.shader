Shader "Custom/tailPost2" {

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

			// #pragma surface surf Lambert vertex:vert
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Chunks/hsv.cginc"

struct Transfer {
  float3 vertex;
  float3 normal;
  float3 tan;
  float2 uv;
  float3 debug;
};
	
			uniform sampler2D _MainTex;// _Color;
			uniform float3 _Color;
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
				float3 fTan 	= t.tan;
				float3 fDebug = t.debug;

				varyings o;

				o.pos = mul(UNITY_MATRIX_VP, float4(fPos,1));
				o.worldPos = fPos;
				o.eye = fTan;//_WorldSpaceCameraPos - fPos;
				o.nor = fNor;
				o.uv =  fUV;
				o.debug = fDebug;

				TRANSFER_SHADOW(o);

				return o;
			}

			float4 frag(varyings v) : COLOR {

				float3 x = ddx(v.worldPos);
				float3 y = ddy(v.worldPos);

				float3 nor = -normalize(cross(x,y));


				float3 col = nor * .5 + .5;


				float d = -dot( _WorldSpaceLightPos0.xyz , nor);

				col = v.nor * .5 + .5; //normalize(v.eye) * .5 + .5;//float3(1,1,1);//* hsv(d,.5 , 1)*max(d,0);
				col = nor * .5 + .5;

				col = _Color;
				col = tex2D(_MainTex , (1-v.uv.yx));

				float3 f = col;// _Color;//normalize(v.eye)* .5 +.5;//col;// * hsv( v.debug.x * .3  + v.debug.y * .8+d * .1, .2 , 1 );
				//f *= (1-v.uv.y);

				if( length(col) < .1){discard;}

				fixed shadow = SHADOW_ATTENUATION(v) * .8 + .2 ;

				return float4(shadow*f , 1.);
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