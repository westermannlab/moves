Shader "Custom/Blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Blend ("Blend", float) = 0
        _OffsetX ("X Offset", int) = 1
        _OffsetY ("Y Offset", int) = 0
        _PreviousOffsetX ("Previous X Offset", int) = 0
        _PreviousOffsetY ("Previous Y Offset", int) = 0
        _SpriteHeight ("Sprite height in pixels", int) = 8
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        Lighting Off
        ZWrite On
        ZTest Always
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            sampler2D _HighlightTex;
            float4 _HighlightTex_ST;
            float _Blend;
            int _OffsetX;
            int _OffsetY;
            int _PreviousOffsetX;
            int _PreviousOffsetY;
            int _SpriteHeight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv + float2(_PreviousOffsetX, _PreviousOffsetY) * (_SpriteHeight / _MainTex_TexelSize.z)) * i.color;
                if (col.a == 0)
                {
                    discard;
                }
                
                fixed4 highlightColor = tex2D(_MainTex, i.uv + float2(_OffsetX, _OffsetY) * (_SpriteHeight / _MainTex_TexelSize.z)) * i.color;
                return lerp(col, highlightColor, _Blend);
                
                return col;
            }
            ENDCG
        }
    }
}
