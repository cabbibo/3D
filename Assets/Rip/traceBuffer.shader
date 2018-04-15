Shader "Custom/traceBuffer" {


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


      #pragma target 4.5
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
     #include "Chunks/noise.cginc"
     #include "Chunks/sdfStruct.cginc"

      uniform int _NumberSteps;
      uniform float _TotalDepth;
      uniform float _PatternSize;
      uniform float _HueSize;
      uniform float _BaseHue;


uniform int _VolDim;


RWStructuredBuffer<SDF> _volumeBuffer;

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
        // Getting the position for actual position
        o.pos = UnityObjectToClipPos(  fPos );


     
        float3 mPos = mul( unity_ObjectToWorld , fPos );

        // The ray origin will be right where the position is of the surface
        o.ro = mPos;//fPos;


        float3 camPos = mul( unity_WorldToObject , float4( _WorldSpaceCameraPos , 1. )).xyz;

        // the ray direction will use the position of the camera in local space, and 
        // draw a ray from the camera to the position shooting a ray through that point
        o.rd = normalize(mPos - _WorldSpaceCameraPos);//normalize( fPos.xyz - camPos );

        return o;

      }


float GetID( float3 pos ){


  if( abs(pos.x) < 1 &&abs(pos.y) < 1 &&abs(pos.z) < 1){
    int x = clamp( floor(((pos.x + 1)/2) * float(_VolDim)),0,_VolDim); 
    int y = clamp( floor(((pos.y + 1)/2) * float(_VolDim)),0,_VolDim); 
    int z = clamp( floor(((pos.z + 1)/2) * float(_VolDim)),0,_VolDim); 

    return x  + y * _VolDim + z * _VolDim * _VolDim;
    
    //return pos.x * _VolDim;// + sin(pos.y*100) + +sin(pos.z*100);//return floor(pos.x *100);//floor((pos.x +1)/2);//x * _VolDim * _VolDim + y * _VolDim + z;
  }else{
    return -1;
  }


}


      float map( float3 pos ){

      		int id = GetID(pos);

      		if( id >= 0 ){

      	  SDF vol = _volumeBuffer[id];


        	float depth =  vol.dist * vol.pn;//length( p - float3(0,0,0)) - .2;//info.a *2;
		
					/*depth -= noise( pos * 5 + float3(0,_Time.y,0))   * .03 *.8;
					depth -= noise( pos * 10 + float3(0,_Time.y,0)) * .02 *.8;
					depth -= noise( pos * 20 + float3(0,_Time.y,0)) * .01 *.8;*/

										return depth;
										}else{
											return 10000;
										}

      }


      float3 calcNormal(  float3 pos ){
	


      	float3 eps = float3( 2/float(_VolDim), 0.0, 0.0 );
      	float3 nor = float3(
      	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
      	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
      	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
      	return _volumeBuffer[GetID(pos)].nor;//normalize(nor);

      


      }

      // Fragment Shader
      fixed4 frag(VertexOut v) : COLOR {

				// Ray origin 
        float3 ro 			= v.ro;

        // Ray direction
        float3 rd 			= normalize(v.ro - _WorldSpaceCameraPos);       

        // Our color starts off at zero,   
        float3 col = float3( 0.0 , 0.0 , 0.0 );



        float3 p;
        float depth = 0;
        float dist = 0;

        float hit = 0;

        for( int i = 0; i < 20; i++ ){

        	p = ro + rd * dist;// * .5 + .5;


        	float depth = map( p );

					dist += depth;

				
					if( depth < .1){

        	float3 nor = calcNormal(p); 
        	float rim = (1+dot( rd , nor ));
						col = hsv( rim  , 1 ,rim*rim);
						//col +=.8* (1-rim) * (nor * .5 + .5);//refl * .5 + .5;info.rgb * .5 + .5;//float3(1,1,1);// (float(i)+1)/10;//float3( , info.rgb;//float3(1,0,0);
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
