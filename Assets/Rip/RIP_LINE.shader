Shader "Custom/RIP_LINE" {


    SubShader{
        Cull off
        Pass{

            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
            CGPROGRAM
            #pragma target 4.5
 
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"

            uniform int _Count;


 
struct Vert{
    float3 pos;
    float3 vel;
    float3 nor;
    float2 uv;
    float id;
    float width;
    float cap; // 0 for no cap, 1 for start , 2 for end
    float3 targetPos;
    float3 debug;
  };

            

            StructuredBuffer<Vert> _vertBuffer;

            //A simple input struct for our pixel shader step containing a position.
            struct varyings {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 nor      : TEXCOORD0;
                float3 eye      : TEXCOORD2;
                float3 debug    : TEXCOORD3;
                float2 uv       : TEXCOORD4;
            };

            
           

            //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
            //which we transform with the view-projection matrix before passing to the pixel program.
            varyings vert (uint id : SV_VertexID){

                varyings o;

                
                int base  = id / 2;
                int offsetID = id % 2;



                int id1 = base % _Count;
                int id2 = (base + 1 ) % _Count;

                if( id1 < _Count &&  id2 < _Count && id1 >= 0 &&  id2 >= 0){

                  Vert v1 = _vertBuffer[id1];
                  Vert v2 = _vertBuffer[id2];



                  float3 nor = float3( 1,0,0 );
                  float3 fPos = float3( 0,0,0);

                  nor = normalize(v1.pos - v2.pos );

                  float3 debug;

                  if( offsetID  == 0 ){
                  	fPos = v1.pos;
                      debug = v1.debug;
                  }else{
                  	fPos = v2.pos;
                      debug = v2.debug;
                  }

                  

                  //if its at the end END IT.
                  if( v1.cap == 1 ){
                      o.debug = float3( 0 , 1, 0);
                      fPos = v1.pos;
                  }

                  if( v1.cap == 2 ){
                      o.debug = float3( 1 , 0, 0);
                      fPos = v1.pos;
                  }

                  if( v2.cap == 1 ){
                      o.debug = float3( 1, 0, 0);
                      fPos = v1.pos;
                  }

                  if( v2.cap == 2 ){
                      o.debug = float3( 0 , 0, 1);
                      fPos = v1.pos;
                  }


                  o.worldPos = fPos;//mul( worldMat , float4( v.pos , 1.) ).xyz;

                  o.eye = _WorldSpaceCameraPos - o.worldPos;

                  o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));

                  o.debug = debug;
                  // v.debug;//normalize(v.pos-v.vel) * .5+.5;//n * .5 + .5;
                  o.uv = float2(0,0);
                  o.nor = nor;

                }

            
                return o;

            }
 
            //Pixel function returns a solid color for each point.
            float4 frag (varyings v) : COLOR {


            

                float3 col = v.debug;//cubeCol;// * (.6*audio+.7) * float3( 6 , 3 + sin(_Time.x) , 2) * .2;// float3(1,0,0);//v.debug * v.uv.x;//v.nor * .5 + .5;
                return float4( col , 1 );

            }
 
            ENDCG
 
        }
    }
 
    Fallback Off

}