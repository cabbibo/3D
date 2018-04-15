Shader "Custom/TraceVolume" {


  Properties {
  
 		// This is how many steps the trace will take.
 		// Keep in mind that increasing this will increase
 		// Cost
    _NumberSteps( "Number Steps", Int ) = 3

    // Total Depth of the trace. Deeper means more parallax
    // but also less precision per step
    _TotalDepth( "Total Depth", Float ) = 0.16


    _PatternSize( "Pattern Size", Float ) = 10
    _HueSize( "Hue Size", Float ) = .3
    _BaseHue( "Base Hue", Float ) = .3

    _MainTex("", 3D) = "white" {}




  }

  SubShader {


    Pass {

      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
     #include "Chunks/noise.cginc"

      uniform int _NumberSteps;
      uniform float _TotalDepth;
      uniform float _PatternSize;
      uniform float _HueSize;
      uniform float _BaseHue;
 sampler3D _MainTex;

      struct VertexIn{
         float4 position  : POSITION; 
         float3 normal    : NORMAL; 
         float4 texcoord  : TEXCOORD0; 
         float4 tangent   : TANGENT;
      };


      struct VertexOut {
          float4 pos    	: POSITION; 
          float3 normal 	: NORMAL; 
          float4 uv     	: TEXCOORD0; 
          float3 ro     	: TEXCOORD1;
          float3 rd     	: TEXCOORD2;
      };


			float3 hsv(float h, float s, float v){
        return lerp( float3( 1.0,1,1 ), clamp(( abs( frac(h + float3( 3.0, 2.0, 1.0 ) / 3.0 )
        					 * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
      }

      float getFogVal( float3 pos ){

      	return abs( sin( pos.x * _PatternSize ) + sin(pos.y * _PatternSize ) + sin( pos.z * _PatternSize ));
      }
      
      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;

        o.uv = v.texcoord;


       
        float3 fPos = v.position;

//        float n = noise( fPos * 10 );

        //fPos+= n * v.normal * .3 * o.uv.x;

        // Getting the position for actual position
        o.pos = UnityObjectToClipPos(  fPos );


     
        float3 mPos = mul( unity_ObjectToWorld , fPos );

        // The ray origin will be right where the position is of the surface
        o.ro = fPos;


        float3 camPos = mul( unity_WorldToObject , float4( _WorldSpaceCameraPos , 1. )).xyz;

        // the ray direction will use the position of the camera in local space, and 
        // draw a ray from the camera to the position shooting a ray through that point
        o.rd = normalize( fPos.xyz - camPos );

        return o;

      }


      float map( float3 pos ){
      		float4 info = tex3D(_MainTex, pos  + .5 );

        	float depth =  info.a;//length( p - float3(0,0,0)) - .2;//info.a *2;
		
					depth -= noise( pos * 10 + float3(0,_Time.y,0)) * .03 * 1;
					depth -= noise( pos * 30 + float3(0,_Time.y,0)) * .02 * 1;
					depth -= noise( pos * 60 + float3(0,_Time.y,0)) * .01 * 1;

										return depth;

      }


      float3 calcNormal( in float3 pos ){

      	float3 eps = float3( 0.01, 0.0, 0.0 );
      	float3 nor = float3(
      	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
      	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
      	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
      	return normalize(nor);

      }

      // Fragment Shader
      fixed4 frag(VertexOut v) : COLOR {

				// Ray origin 
        float3 ro 			= v.ro;

        // Ray direction
        float3 rd 			= v.rd;       

        // Our color starts off at zero,   
        float3 col = float3( 0.0 , 0.0 , 0.0 );



        float3 p;
        float depth = 0;
        float dist = 0;

        float hit = 0;

        for( int i = 0; i < 10; i++ ){

        	p = float3(ro + rd * dist);// * .5 + .5;


        	float depth = map( p );

        	float3 nor = calcNormal(p); 
					dist += depth;

				
					if( depth < .01){

						col =  nor * .5 + .5;//refl * .5 + .5;info.rgb * .5 + .5;//float3(1,1,1);// (float(i)+1)/10;//float3( , info.rgb;//float3(1,0,0);
						hit = 1;
						break;
					}

        }
        if( hit == 0 ){ discard; }


        //col =  v.normal * .5 + .5;// 2* _NumberSteps;

		    fixed4 color;
        color = fixed4( col , 1. );
        return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}