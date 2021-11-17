#if !defined(GIZMO_LIGHTING)
#define GIZMO_LIGHTING

#include "UnityCG.cginc"

// Material properties
half4 _Color;
half _Glossiness;
half _Specular;
half4 _Emission;

struct Input
{
    float2 uv_MainTex;
};

void surfaceProgram (Input IN, inout SurfaceOutputStandardSpecular o)
{
  o.Albedo = _Color.rgb;
  o.Emission = _Emission.rgb;
  o.Specular = _Specular;
  o.Smoothness = _Glossiness;
  o.Alpha = _Color.a;
}

#endif
