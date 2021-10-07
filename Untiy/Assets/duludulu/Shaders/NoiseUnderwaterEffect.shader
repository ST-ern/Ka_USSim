Shader "Duludulu/NoiseUnderwater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noisescale ("Noise Scale", float) = 1
        _Noisefrequency ("Noise Frequency", float) = 1
        _Noisespeed ("Noise Speed", float) = 1
        _Pixeloffset ("Pixel Offset", float) = 0.005
        _DepthStart ("Depth Start", float) = 1
        _DepthDistance ("Depth Distance", float) = 1
    }
    SubShader
    {
        

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define M_PI 3.141592653897932384626433832795
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "noiseSimplex.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
                float4 scrpos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _CameraDepthTexture;
            uniform float _Noisescale, _Noisefrequency, _Noisespeed, _Pixeloffset;
            float _DepthStart, _DepthDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.scrpos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                //fixed4 col = tex2D(_MainTex, i.uv);
                // // just invert the colors
                // col.rgb =  col.rgb;

                float depthValue = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrpos)).r) * _ProjectionParams.z;
                depthValue = 1 - saturate((depthValue - _DepthStart) / _DepthDistance);

                float3 spos = float3(i.scrpos.x, i.scrpos.y, 0) * _Noisefrequency;
                spos.z += _Time.x * _Noisespeed;
                float noise = _Noisescale * ((snoise(spos) + 1)/2);

                float4 noiseToDirection = float4(cos(noise*M_PI*2), sin(noise*M_PI*2), 0,0);
                fixed4 col = tex2Dproj(_MainTex, i.scrpos + (normalize(noiseToDirection)) * _Pixeloffset * depthValue);
                return col;
            }
            ENDCG
        }
    }
}
