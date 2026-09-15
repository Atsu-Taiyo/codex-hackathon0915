Shader "EnglishWorld/IslandWater"
{
    Properties
    {
        _Color ("Deep water", Color) = (0.16,0.49,0.55,1)
        _Shallow ("Shallow water", Color) = (0.39,0.75,0.70,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color, _Shallow;
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 p = IN.worldPos.xz;
            float wave = sin(p.x * 2.1 + p.y * 1.4 + _Time.y * 0.6)
                       * sin(p.y * 2.8 - p.x * 0.6 - _Time.y * 0.4);
            float coast = 1 - saturate(length(p / float2(13, 10)) - 0.4);
            float glint = pow(saturate(wave), 18) * 0.035;
            o.Albedo = lerp(_Color.rgb, _Shallow.rgb, coast * 0.7 + wave * 0.035) + glint;
            o.Emission = o.Albedo * 0.16;
            o.Metallic = 0.05;
            o.Smoothness = 0.58;
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
