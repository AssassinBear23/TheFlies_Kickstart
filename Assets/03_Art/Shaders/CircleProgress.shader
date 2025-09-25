Shader "Unlit/RippleMaskedAnimated"
{
    Properties
    {
        _Progress ("Circle Progress (0=small,1=big)", Range(0,1)) = 1
        _RippleCount ("Ripple Count", Float) = 6
        _LineWidth ("Ripple Line Width", Range(0.001,0.2)) = 0.02
        _Speed ("Ripple Speed", Float) = 1
        _Color ("Ripple Color", Color) = (1,1,1,1)
        _FadeSmoothness ("Fade Smoothness", Range(0.1,3)) = 1.5
        _FadeFraction ("Fade Fraction of Radius", Range(0.0,1)) = 0.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Progress;
            float _RippleCount;
            float _LineWidth;
            float _Speed;
            float _FadeSmoothness;
            float _FadeFraction;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Center coords
                float2 p = i.uv - 0.5;
                float dist = length(p) * 2.0;

                // Ripple pattern (animated)
                float ripplePhase = dist * _RippleCount - _Time.y * _Speed;
                float wave = 1.0 - smoothstep(0.0, _LineWidth, abs(frac(ripplePhase) - 0.5));

                // Circle cutoff radius
                float radius = 0.5 * _Progress;
                float aa = fwidth(dist) * 1.5;
                float mask = 1.0 - smoothstep(radius, radius + aa, dist);

                // --- Fade near the outer edge ---
                float fadeStart = radius * (1.0 - _FadeFraction); // e.g. 80% of radius
                float fadeEnd   = radius;                         // full cutoff
                float baseFade  = smoothstep(fadeStart, fadeEnd, dist);

                // Smoothness: >1 = more gradual
                float edgeFade = pow(baseFade, 1.0 / _FadeSmoothness);

                // Combine
                float final = wave * mask * edgeFade;

                // Dead center mask
                if (dist < 0.001) final = 0;

                fixed4 col = _Color;
                col.a *= final;
                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
