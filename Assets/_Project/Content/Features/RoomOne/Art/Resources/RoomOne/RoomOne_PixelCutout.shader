Shader "RoomOne/PixelCutout"
{
    Properties
    {
        _MainTex ("Generated sprite", 2D) = "white" {}
        _Cutoff ("Opaque pixel threshold", Range(0,1)) = 0.96
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _Cutoff;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            Output vert(Input input)
            {
                Output output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }
            fixed4 frag(Output input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv);
                clip(color.a - _Cutoff);
                return fixed4(color.rgb, 1);
            }
            ENDCG
        }
    }
}
