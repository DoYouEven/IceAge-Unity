Shader "Energy Bar Toolkit/Unlit/Font" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_PrimaryColor ("Primary Color (White)", Color) = (1, 1, 1, 1)
		_SecondaryColor ("Secondary Color (Black)", Color) = (0, 0, 0, 1)
    }

    SubShader {
        Tags {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        LOD 100
 
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 
        Cull Off
        Lighting Off
        ColorMaterial AmbientAndDiffuse

         Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
 
            #include "UnityCG.cginc"
 
            uniform sampler2D _MainTex;
            uniform fixed4 _PrimaryColor;
            uniform fixed4 _SecondaryColor;
 
            struct vertexInput {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };
 
            struct fragmentInput{
                float4 position : SV_POSITION;
                float4 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };
 
            fragmentInput vert(vertexInput i){
                fragmentInput o;
                o.position = mul(UNITY_MATRIX_MVP, i.vertex);
                o.texcoord = i.texcoord;
                o.color = i.color;
                return o;
            }
 
            float4 frag(fragmentInput i) : COLOR {
                float4 color = tex2D(_MainTex, i.texcoord.xy);
                color *= i.color;
                color.rgb = color.rgb * _PrimaryColor.rgb + ((1 - color.rgb) * _SecondaryColor.rgb);
                return color;
            }
            ENDCG
        }
    }
}
