Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TileNum ("TileNum", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZWrite Off
	        Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog
            #define HASHSCALE3 float3(.1031, .1030, .0973)
            float _TileNum ; 

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            float3 ProcessFrag(float2 uv);
            float4 frag(v2f i) : SV_Target
            {
	            return float4(ProcessFrag(i.uv),1.0);
            }


            //fixed4 frag (v2f i) : SV_Target
            //{
                // sample the texture
            //    fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
            //    UNITY_APPLY_FOG(i.fogCoord, col);
            //    return col;
            //}


            //----------------------------------------------------------------------------------------
            ///  2 out, 2 in...
            float2 Hash22(float2 p)
            {
	            float3 p3 = frac(float3(p.xyx) * HASHSCALE3);
                p3 += dot(p3, p3.yzx+19.19);
                return frac((p3.xx+p3.yz)*p3.zy);

            }
            //----------------------------------------------------------------------------------------
            ///  3 out, 3 in... 
            float3 Hash33(float3 p3)
            {
	            p3 = frac(p3 * HASHSCALE3);
                p3 += dot(p3, p3.yxz+19.19);
                return frac((p3.xxy + p3.yxx)*p3.zyx);

            }


            //voronoi worleyNoise
            float WNoise(float2 p,float time) {
	            float2 n = floor(p);
	            float2 f = frac(p);
	            float md = 5.0;
	            float2 m = float2(0.,0.);
	            for (int i = -1;i<=1;i++) {
		            for (int j = -1;j<=1;j++) {
			            float2 g = float2(i, j);
			            float2 o = Hash22(n+g);
			            o = 0.5+0.5*sin(time+6.28*o);
			            float2 r = g + o - f;
			            float d = dot(r, r);
			            if (d<md) {
				            md = d;
				            m = n+g+o;
			            } 
		            }
	            }
	            return md;
            }
            //3D version please ref to https://www.shadertoy.com/view/ldl3Dl 
            float3 WNoise( in float3 x ,float time)
            {
                float3 p = floor( x );
                float3 f = frac( x );

                float id = 0.0;
                float2 res = float2( 100.0,100.0 );
                for( int k=-1; k<=1; k++ )
                for( int j=-1; j<=1; j++ )
                for( int i=-1; i<=1; i++ )
                {
                    float3 b = float3( float(i), float(j), float(k) );
		            float3 o = Hash33( p + b );
		            o = 0.5+0.5*sin(time+6.28*o);
                    float3 r = float3( b ) - f + o;
                    float d = dot( r, r );

                    if( d < res.x )
                    {
                        id = dot( p+b, float3(1.0,57.0,113.0 ) );
                        res = float2( d, res.x );         
                    }
                    else if( d < res.y )
                    {
                        res.y = d;
                    }
                }

                return float3( sqrt( res ), abs(id) );
            }



            float CausticVoronoi(float2 p,float time)
            {
	            float v = 0.0;
	            float a = 0.4;
	            for (int i = 0;i<3;i++) {
		            v+= WNoise(p,time)*a;
		            p*=2.0;
		            a*=0.5;
	            }
	            v = pow(v,2.)*5.;
	            return v;
            }


            float3 ProcessFrag(float2 _uv){
				float2 uv = _TileNum * _uv;
				float time = _Time.y;
				float val = CausticVoronoi(uv,time); 
                return float3(val,val,val);
            }
            ENDCG
        }
    }
}
