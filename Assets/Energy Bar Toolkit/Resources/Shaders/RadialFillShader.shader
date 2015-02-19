Shader "Custom/Energy Bar Toolkit/Radial Fill" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color (RGB)", Color) = (1, 1, 1, 1)
        _CenterX("Rotation Center X", Float) = 0.5
        _CenterY("Rotation Center Y", Float) = 0.5
        _Offset ("Start Pos (0-1)", Float) = 0.0
        _Length ("End Pos (0-1)", Float) = 1.0
        _Progress ("Progress", Float) = 0.5
        _Invert ("Invert", Float) = 0.0
    }
    SubShader {
        Tags { "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }
        ZWrite Off
        Cull Off
        
        CGPROGRAM
        #pragma surface surf NoLighting noforwardadd noambient

        sampler2D _MainTex;
        fixed4 _Color;
        fixed _CenterX;
        fixed _CenterY;
        fixed _Offset;
        fixed _Length;
        half _Progress;
        bool _Invert;

        struct Input {
            fixed2 uv_MainTex;
        };
        
        bool isVisible(fixed2 p, half progress, bool invert) {
            half x = p.x - _CenterX;
            half y = p.y - _CenterY;
            
            if (x == 0) {
                x = 0.001; // hack to not divide by 0. Did this because of 64 instruction limit
            }
            fixed angle = (atan(y/x) + 1.57079) / 3.14159;
            
            if (x > 0) {
                angle = (1.0 - angle) / 2; 
            } else {
                angle = 1.0 - angle / 2;
            }
            
            angle = fmod((angle - _Offset) + 1, 1);
//            progress = clamp(progress, _Offset, _Length);
            progress = progress * _Length;
            
            if (!invert) {
                return angle < progress;
            } else {
                return angle > 1.0 - progress;
            }
        }
        
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            
            if (!isVisible(IN.uv_MainTex, _Progress, _Invert)) {
                // NOTE: due to Unity bug this statement must be done after tex2D()   
                discard;
            }
            
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

    FallBack "Diffuse"
}
