// Unlit alpha-blended shader with tint color.
// - no lighting
// - no lightmap support
Shader "Energy Bar Toolkit/Unlit/Transparent Tint Pre" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader {
        Tags { "Queue"="Overlay" }
        Blend One OneMinusSrcAlpha
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
