﻿Shader "MARS/Textured Doublesided Fresnel"
{
    Properties
    {
        _EdgeColor("Edge Color", COLOR) = (1,1,1,1)
        _Color("Color", COLOR) = (.25,.25,.25,.25)
        _EdgeData("Edge min, max, S-strength, S-Blend", VECTOR) = (0, 0.85, 0.5, 1)
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        // First, we do a stencil like technique of writing depth of the model,
        // so we don't have any transparent overdraw in subsequent steps
        Pass
        {
            Blend One One
            Cull Off
            Lighting Off
            ZTest Less
            ZWrite On
            ColorMask 0

            CGPROGRAM

                #pragma vertex vert
                #pragma fragment fragEmpty

                #include "UnityCG.cginc"
                #include "TexturedStableFresnelCommon.cginc"

            ENDCG
        }
        // Next, fill in with the base and rim color
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            Lighting Off
            ZTest LEqual
            ZWrite Off

            CGPROGRAM

                #pragma vertex vert
                #pragma fragment fragRimShader

                #include "UnityCG.cginc"
                #include "TexturedStableFresnelCommon.cginc"

            ENDCG
        }
    }
    FallBack "Diffuse"
}
