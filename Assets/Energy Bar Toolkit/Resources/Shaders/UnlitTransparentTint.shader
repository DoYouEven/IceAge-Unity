// Unlit alpha-blended shader with tint color.
// - no lighting
// - no lightmap support
Shader "Energy Bar Toolkit/Unlit/Transparent Tint" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader {
        Tags { "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Fog { Mode Off }
        ZWrite Off
        Cull Off
        ColorMaterial AmbientAndDiffuse

        Pass {
            SetTexture [_MainTex] {
            	combine texture * primary
            }
        }
    }
}
