Shader "Custom/RIP_FLAT" {
	Properties {
		}


  SubShader{
//        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
    Cull Off
    Pass{

      //Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

      CGPROGRAM
      #pragma target 4.5

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "Chunks/noise.cginc"
      #include "Chunks/hsv.cginc"


		  uniform int _SmoothedCount;
		  uniform int _Count;

      uniform samplerCUBE _CubeMap;
      uniform sampler2D _NormalMap;
      uniform sampler2D _Audio;

 
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




	float3 cubicCurve( float t , float3  c0 , float3 c1 , float3 c2 , float3 c3 ){
  
  float s  = 1. - t; 

  float3 v1 = c0 * ( s * s * s );
  float3 v2 = 3. * c1 * ( s * s ) * t;
  float3 v3 = 3. * c2 * s * ( t * t );
  float3 v4 = c3 * ( t * t * t );

  float3 value = v1 + v2 + v3 + v4;

  return value;

}

float3 cubicFromValue( in float val , out float3 upPos , out float3 doPos  , out float cap , out float3 data ){

  //float3 upPos;
  //float3 doPos;




  float3 p0 = float3( 0. , 0. , 0. );
  float3 v0 = float3( 0. , 0. , 0. );
  float3 p1 = float3( 0. , 0. , 0. );
  float3 v1 = float3( 0. , 0. , 0. );

  float3 p2 = float3( 0. , 0. , 0. );

  float base = val * (float(_Count));
  float baseUp   = floor( base );
  float baseDown = ceil( base );
  float amount = base - baseUp;

  float nAmount = amount / ( baseDown - baseUp );

  int id1 = (int( baseUp ) +_Count)  % _Count;
  int id2 = (int( baseDown )+_Count)   % _Count;
  int id3 = int( (baseUp+_Count) - 1. ) % _Count;
  int id4 = int( baseDown+_Count + 1. ) % _Count;


  if( id1 < _Count && id2 < _Count && id3 < _Count && id4 < _Count &&
      id1 >= 0 && id2 >= 0 && id3 >= 0 && id4 >= 0
    ){
    Vert vUp = _vertBuffer[ id1 ];
    Vert vDown = _vertBuffer[  id2 ];

    cap = max( vUp.cap , vDown.cap ); 

    p0 = vUp.pos;
    p1 = vDown.pos;


    float3 pMinus;

    int bUU = int( (baseUp+_Count) - 1. );
    bUU %= _Count;

    int bDD = int( baseDown+_Count + 1. );
    bDD %= _Count;

    pMinus = _vertBuffer[ bUU ].pos;
    p2 =     _vertBuffer[ bDD ].pos;

    v1 = .5 * ( p2 - p0 );
    v0 = .5 * ( p1 - pMinus );


    float3 c0 = p0;
    float3 c1 = p0 + v0/3.;
    float3 c2 = p1 - v1/3.;
    float3 c3 = p1;


    float3 pos = cubicCurve( amount , c0 , c1 , c2 , c3 );

    upPos = cubicCurve( amount  + .1 , c0 , c1 , c2 , c3 );
    doPos = cubicCurve( amount  - .1 , c0 , c1 , c2 , c3 );

    data = cubicCurve(amount, vDown.debug,vDown.debug,vUp.debug, vUp.debug);

    return pos;

  }else{

    upPos = float3( 0,0,0);
    doPos = float3( 0,0,0);
    data = float3( 0,0,0);
    cap = 1;
    return float3(0,0,0);

  }


}

            


      //uniform float4x4 worldMat;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos      : SV_POSITION;
          float3 nor      : TEXCOORD0;
          float3 worldPos : TEXCOORD1;
          float3 eye      : TEXCOORD2;
          float3 debug    : TEXCOORD3;
          float2 uv       : TEXCOORD4;
          float  noiseVal : TEXCOORD5;
      };

        
       

      //Our vertex function simply fetches a point from the buffer corresponding to the vertex index
      //which we transform with the view-projection matrix before passing to the pixel program.
      varyings vert (uint id : SV_VertexID){

        varyings o;

        int bID = id / 6;

        float row1 = float(bID) / _SmoothedCount;
        float row2 = (float(bID)+1) / _SmoothedCount;


       	int remainID = id % 6;


       	float3 upPos1; float3 doPos1; float cap1; float3 data1;
       	float3 upPos2; float3 doPos2; float cap2; float3 data2;

       	float3 pos1 = cubicFromValue( row1 , upPos1 , doPos1  , cap1 , data1 );
       	float3 pos2 = cubicFromValue( row2 , upPos2 , doPos2  , cap2 , data2 );


				float3 eye1 = normalize( _WorldSpaceCameraPos - pos1 );
				float3 eye2 = normalize( _WorldSpaceCameraPos - pos2 );

       	if( cap1 == 0  && cap2 == 0){

       		float width1 = .004;
       		float3 dir1 = normalize(upPos1 - pos1);
       		float3 down1 = normalize( doPos1 - pos1);
       		float3 nor1 = eye1;//normalize(cross(dir1 ,down1));
       		float3 left1 = normalize( cross(nor1 , dir1) );

       		float width2 = .004 ;
       		float3 dir2 = normalize(upPos2 - pos2);
       		float3 down2 = normalize( doPos2 - pos2);
       		float3 nor2 = eye2;//normalize(cross( dir2 ,down2));
       		float3 left2 = normalize( cross(nor2 , dir2));

       		float3 p1 = pos1 - left1* width1; float3 p2 = pos1 + left1 * width1;
       		float3 p3 = pos2 - left2* width2; float3 p4 = pos2 + left2 * width2;
       	
       		float3 fPos;
       		float3 fNor;
       		float2 fUV;



       		if( remainID == 0 || remainID == 3 ){
       			fPos = p1;
       			fNor = nor1;
       			fUV = float2( row1 , 0);
       		}else if( remainID == 1 ){
						fPos = p2;
       			fNor = nor1;
       			fUV = float2( row1 , 1);
       		}else if( remainID == 2 || remainID == 4 ){
						fPos = p4;
       			fNor = nor2;
       			fUV = float2( row2 , 1);
       		}else{
       			fPos = p3;
       			fNor = nor2;
       			fUV = float2( row2 , 0);
       		}


       		o.worldPos = fPos;//mul( worldMat , float4( v.pos , 1.) ).xyz;
	        o.debug = fNor * .5 + .5;
	        o.eye = _WorldSpaceCameraPos - o.worldPos;
          o.nor = fNor;//fPos;
          o.uv = fUV;

	        o.pos = mul (UNITY_MATRIX_VP, float4(o.worldPos,1.0f));


       	}

    
        return o;

      }


      float sdSphere( float3 p, float s ){
        return length(p)-s;
      }

      float2 smoothU( float2 d1, float2 d2, float k){
          float a = d1.x;
          float b = d2.x;
          float h = clamp(0.5+0.5*(b-a)/k, 0.0, 1.0);
          return float2( lerp(b, a, h) - k*h*(1.0-h), lerp(d2.y, d1.y, pow(h, 2.0)));
      }


float3 opRep( float3 p, float3 c )
{
    float3 q = (p%c)-0.5*c;
    return q;
}


       float2 map( in float3 pos ){
        
        float2 res;

        float3 nP = opRep( pos , float3( .03 , .03 , .03 ));

        //res = float2( -sdBox( pos , float3( 1. , 1. , 1. ) * .53 ) , 0. );
        //res = smoothU( res , float2( sdSphere( pos , .04) , 1. ) , 0.1);


        res = float2( -sdSphere( nP , .01 ) , 1. );
        res.x -= .04 * noise( pos * 100);

  	    return res;//float2( length( pos ) - .3, 0.1 ); 
  	 
  	  }

      float3 calcNormal( in float3 pos ){

      	float3 eps = float3( 0.001, 0.0, 0.0 );
      	float3 nor = float3(
      	    map(pos+eps.xyy).x - map(pos-eps.xyy).x,
      	    map(pos+eps.yxy).x - map(pos-eps.yxy).x,
      	    map(pos+eps.yyx).x - map(pos-eps.yyx).x );
      	return normalize(nor);

      }
              
         
      float _MaxTraceDistance = 1;
      float _IntersectionPrecision = .001;

      float2 calcIntersection( in float3 ro , in float3 rd ){     
            
               
        float h =  _IntersectionPrecision * 2;
        float t = 0.0;
        float res = -1.0;
        float id = -1.0;
        
        for( int i=0; i< 20; i++ ){
            
            if( h < _IntersectionPrecision || t > _MaxTraceDistance ) break;
    
            float3 pos = ro + rd*t;
            float2 m = map( pos );
            
            h = m.x;
            t += h;
            id = m.y;
            
        }
    
    
        if( t <  _MaxTraceDistance ){ res = t; }
        if( t >  _MaxTraceDistance ){ id = -1.0; }
        
        return float2( res , id );
          
      
      }
            

      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {

    			float n = noise( v.worldPos * 100 * (sin(v.uv.x * 100)));

					float3 col = float3( 1,1,1);
					float cutoffVal = abs(.5 - v.uv.y) - n * .6;


					float3 ro = v.worldPos;
					float3 rd = normalize( v.eye );

    			if( cutoffVal > 0){
          // = v.nor * .5 + .5;

	          if( cutoffVal > .04 ){
	          	discard;
	          }


          }else{

          	float3 p;

          	float2 res = calcIntersection( ro , rd );

          	if( res.y > 0 ){

          		float3 fPos = ro + rd * res.x;
          		float3 nor = calcNormal( fPos );

          		float m = dot( nor , rd );

          		col = hsv( floor(m * 6)/6 , 1,1);//nor * .5 + .5;
          	}

          	/*
          	for( int i = 0; i < 5; i++){
          		p = float(i) * .1 * v.eye + v.worldPos;
          		float n2 = noise( p * 1000 );
          		if( n2 > .4 ){
          			col -= hsv( n2 * .2 + .3 * float(i) , 1. , n2 )/ 5;
          			//break;
          		}else{

          		}
          	} */

          	//col /= 3;
          	
          }

          //col = float3( v.uv.x , v.uv.y , .5);
          return float4( col , 1 );

      }

      ENDCG

    }
  }

  Fallback Off


}