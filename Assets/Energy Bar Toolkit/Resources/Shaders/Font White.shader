Shader "Energy Bar Toolkit/Unlit/Font White" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        LOD 100
 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 
        Cull Off
        Lighting Off
        ColorMaterial AmbientAndDiffuse
        
        Pass {
            SetTexture [_MainTex] {
                constantColor (1, 1, 1, 1)
                combine constant * primary, texture * primary
            } 
        }
    }
}
