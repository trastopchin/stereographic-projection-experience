Shader "Custom/GizmoShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Specular ("Specular", Range(0,1)) = 0.0
        _Emission ("Emission", Color) = (0,0,0,1)
    }
    SubShader
    {
      Tags {
        "Queue"="Transparent"
        "RenderType"="Transparent"
      }

      ZTest always

      CGPROGRAM
      // Use custom surfaceProgram, physically based Standard lighting model,
      // no shadows, use custom vetexProgram, set alpha mode to premultiplied
      #pragma surface surfaceProgram StandardSpecular alpha:premul

      // Use shader model 3.0 target, to get nicer looking lighting
      #pragma target 3.0

      // Use our custom gizmo lighting programs
      #include "GizmoLighting.cginc"

      ENDCG
    }
    FallBack "Standard"
  }
