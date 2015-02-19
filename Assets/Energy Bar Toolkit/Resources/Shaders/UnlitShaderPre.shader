Shader "Custom/Energy Bar Toolkit/Unlit Pre" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color (RGB)", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { "Queue"="Overlay" }
        Blend One OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }
        
        CGPROGRAM
        #pragma surface surf NoLighting noforwardadd noambient

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input {
            fixed2 uv_MainTex;
        };
        
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            
            o.Albedo = c.rgb * (half3) _Color;
            o.Alpha = c.a * _Color.a;
        }
        
        half4 LightingNoLighting (SurfaceOutput s, half3 lightDir, half atten) {
            half4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }
        ENDCG
    }

    FallBack "Unlit/Transparent"
}
