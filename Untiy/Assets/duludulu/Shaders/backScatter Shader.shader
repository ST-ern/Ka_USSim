Shader "Duludulu/FogEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (1,1,1,1)
        _DepthStart ("Depth Start", float) = 1
        _DepthDistance ("Depth Distance", float) = 1
        _UnitToMeter ("Unit to Meter", float) = 1
        _VerticalDepth ("Vertical Depth", float) = 1
        _AttenuationCoef_r("Attenuation Coefficient r", float) = 0.017
        _AttenuationCoef_g("Attenuation Coefficient g", float) = 0.017
        _AttenuationCoef_b("Attenuation Coefficient b", float) = 0.017

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

            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;
            fixed4 _FogColor;
            float _DepthStart, _DepthDistance;
            float _UnitToMeter;
            float _VerticalDepth;
            float _AttenuationCoef_r,_AttenuationCoef_g,_AttenuationCoef_b;
            float _attenuationCoef;   // 暂时不分离物体颜色的rgb通道，计算rgb的平均衰减系数。

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 scrpos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.scrpos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            } 

            sampler2D _MainTex;

            fixed4 frag (v2f i) : COLOR
            {
                //fixed4 col = tex2D(_MainTex, i.uv);
                // // just invert the colors
                // col.rgb =  col.rgb;

                // Unity面板中Camera的Far属性，调整为10，不然就全是黑的

                
                // float depthValue = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrpos)).r);  线性深度（0-1之间）： 与相机设置（前后切面）有关的深度信息
                // float depthValue2 = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r); 和上面一行作用一样

                float depthValue = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrpos)).r) * _ProjectionParams.z;  // 线性深度乘上_ProjectionParams.z（远剪切平面的距离）与相机设置分离，变成独立的深度信息
                // unity颜色只接收0-1之内的数值，所以距离超过1的时候全部视为白色
                // 这里的“距离”指该项目中的单位距离，默认1单位距离为1m
                // 相机的远近切面代表拍摄图像中的近距离物体（海底物体），其中远距离以外就全部视为海水 
                depthValue = (depthValue - _DepthStart + _VerticalDepth) * _UnitToMeter; // 乘上 _UnitToMeter 换算成米
                // 上面的+ _VerticalDepth部分原本应该考虑进物体距离水面高度的不同，这里全部忽略了，视为同一深度的光程距离

                fixed4 fogColor = _FogColor;

                float r = fogColor.r;
                float g = fogColor.g;
                float b = fogColor.b;
                float rgb = r + g + b;
                _attenuationCoef = _AttenuationCoef_r * r / rgb + _AttenuationCoef_g * g / rgb + _AttenuationCoef_b * b / rgb;

                

                fixed4 col = tex2Dproj(_MainTex, i.scrpos);

                

                return lerp(fogColor, col, exp(-1 * _attenuationCoef * depthValue));  // 这个 0.0978就是最终敲定的衰减系数，这个需要后期的更换
                // return depthValue;
                // 在col（物体颜色）和fogColor（雾的颜色）之间进行差值 ，t=depthValue
                // 最终返回的是一个fixed4类型的颜色。 在背景效果里（非人工光照部分），可以认为前向散射和后向散射的衰减系数是一致的，因此也还是一个插值问题。
                // 这里插值的系数就是 t = e^{\beta(\lambda)z}
                
            }
            ENDCG
        }
    }
}
