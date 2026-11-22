// Shader UI tan biến (dissolve) theo texture noise, có viền cháy sáng ở mép đang tan.
// Noise lấy theo toạ độ vertex (đơn vị canvas) -> nhiều Image dùng chung 1 material tan liền mạch như 1 khối.
// _Progress: 0 = nguyên vẹn, 1 = tan hết. Giữ alpha sprite + vertex color, hỗ trợ Mask/RectMask2D như UI/Default.
Shader "UI/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _NoiseTex ("Noise (Wrap = Repeat)", 2D) = "gray" {}
        _NoiseSize ("Noise Size (canvas units)", Float) = 260
        _Contrast ("Noise Contrast (giãn noise cho tan đều theo progress)", Range(1, 4)) = 2
        _Progress ("Progress", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.3)) = 0.08
        [HDR] _EdgeColor ("Edge Color", Color) = (1, 0.75, 0.25, 1)
        _EdgeColor2 ("Edge Color Outer", Color) = (1, 0.35, 0.1, 1)

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 noiseUV       : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _NoiseSize;
            float _Contrast;
            float _Progress;
            float _EdgeWidth;
            fixed4 _EdgeColor;
            fixed4 _EdgeColor2;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.noiseUV = v.vertex.xy / max(_NoiseSize, 1.0);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // Nới khoảng để progress = 0 chắc chắn nguyên vẹn và = 1 chắc chắn tan hết (kể cả phần viền)
                // noise mây dồn quanh 0.5 -> giãn tương phản để giá trị trải đều 0..1 (tan đều theo thời gian)
                half n = saturate((tex2D(_NoiseTex, IN.noiseUV).r - 0.5) * _Contrast + 0.5);
                half p = _Progress * (1.0 + _EdgeWidth * 2.0) - _EdgeWidth;
                half d = n - p;                                   // < 0: đã tan

                half alive = step(0.0, d);
                half edge = 1.0 - saturate(d / max(_EdgeWidth, 0.0001));   // 1 ở mép đang tan -> 0 vào trong
                edge *= alive * step(0.001, _Progress);

                fixed3 edgeCol = lerp(_EdgeColor.rgb, _EdgeColor2.rgb, edge);
                color.rgb = lerp(color.rgb, edgeCol, edge);
                color.a *= alive;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                clip(color.a - 0.001);
                return color;
            }
            ENDCG
        }
    }
}
