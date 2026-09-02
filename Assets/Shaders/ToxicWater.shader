Shader "Custom/2D/ToxicWater"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0.55, 0.9, 0.25, 0.75)
        _WaveSpeed ("Wave Speed", Float) = 1.5
        _WaveFrequency ("Wave Frequency", Float) = 12.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.03
        _ScrollSpeed ("UV Scroll Speed", Float) = 0.15
        [MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        // Fragment output is premultiplied (c.rgb *= c.a below), so blend with One/OneMinusSrcAlpha
        // to match - straight SrcAlpha blending here would darken translucent edges twice.
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;
            float _ScrollSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // Vertex wobble: nudge the mesh vertically so the sprite's silhouette itself
                // ripples, on top of the UV distortion applied in the fragment stage below.
                float vertexWave = sin(_Time.y * _WaveSpeed + IN.vertex.x * _WaveFrequency) * _WaveAmplitude;
                float4 animatedVertex = IN.vertex;
                animatedVertex.y += vertexWave;

                OUT.vertex = UnityObjectToClipPos(animatedVertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);
                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Sine-wave UV distortion plus a slow vertical scroll for the "flowing" look.
                uv.x += sin(uv.y * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveAmplitude;
                uv.y += frac(_Time.y * _ScrollSpeed);

                fixed4 c = SampleSpriteTexture(uv) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDHLSL
        }
    }
}
