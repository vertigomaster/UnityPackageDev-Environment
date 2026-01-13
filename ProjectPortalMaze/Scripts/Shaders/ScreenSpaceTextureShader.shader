Shader "Unlit/ScreenSpaceTextureShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Cull Off //don't cull backfaces (or at all?)
        LOD 100

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
                // float2 uv : TEXCOORD0; //don't care, screenspace effect
            };

            struct v2f
            {
                // float2 uv : TEXCOORD0; //don't care, screenspace effect
                float4 screenPos : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // // sample the texture
                // fixed4 col = tex2D(_MainTex, i.screenPos);
                //reminder: w = 1/z with z being the distance from the NEAR PLANE (not the camera)
                //the inversion biases towards the front for more precise depth checks
                
                //project to screen
                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;

                float4 color = tex2D(_MainTex, screenSpaceUV);

                // apply fog to the color (that may look weird, later may do depth stuff)
                UNITY_APPLY_FOG(i.fogCoord, color);
                return color;
            }
            ENDCG
        }
    }
}