Shader "Custom/RiemannSphereShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Specular ("Specular", Range(0,1)) = 0.0
        _GridColor ("Grid Color", Color) = (1,1,1,1)
        _Scale ("Grid Scale", Float) = 1.0
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Cull Back

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows alpha:premul addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Specular;
        fixed4 _Color;
        fixed4 _EmissionColor;

        fixed4 _GridColor;
        float _Scale;

        static const float pi = 3.14159;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float3 objectPos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;
            float theta = acos(objectPos.y / length(objectPos));
            float phi = atan(objectPos.z /objectPos.x);
            //float2 uv = float2(_UScale * theta / pi, _VScale * phi / pi);
            float2 uv1 = _Scale * objectPos.xz;
            float2 uv2 = _Scale * objectPos.xy;

            // http://www.madebyevan.com/shaders/grid/
            // Compute anti-aliased world-space grid lines
            float2 grid1 = abs(frac(uv1 - 0.5) - 0.5) / (fwidth(uv1) + 1e-4);
            float2 grid2 = abs(frac(uv2 - 0.5) - 0.5) / (fwidth(uv2) + 1e-4);
            float lineMix = 1-saturate(min(min(grid1.x, grid1.y), min(grid2.x, grid2.y)));

            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Specular = _Specular;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = _EmissionColor;
        }
        ENDCG

        Cull Front

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows alpha:premul addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Specular;
        fixed4 _Color;
        fixed4 _EmissionColor;

        fixed4 _GridColor;
        float _Scale;

        static const float pi = 3.14159;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            float3 objectPos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;
            float theta = acos(objectPos.y / length(objectPos));
            float phi = atan(objectPos.z /objectPos.x);
            //float2 uv = float2(_UScale * theta / pi, _VScale * phi / pi);
            float2 uv1 = _Scale * objectPos.xz;
            float2 uv2 = _Scale * objectPos.xy;

            // http://www.madebyevan.com/shaders/grid/
            // Compute anti-aliased world-space grid lines
            float2 grid1 = abs(frac(uv1 - 0.5) - 0.5) / (fwidth(uv1) + 1e-4);
            float2 grid2 = abs(frac(uv2 - 0.5) - 0.5) / (fwidth(uv2) + 1e-4);
            float lineMix = 1-saturate(min(min(grid1.x, grid1.y), min(grid2.x, grid2.y)));

            fixed4 c = _Color; // tex2D (_MainTex, IN.uv_MainTex); // * lerp(_Color, _GridColor, lineMix);
            o.Albedo = c.rgb;
            o.Specular = _Specular;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = _EmissionColor;
        }
        ENDCG
    }
    FallBack "Standard"
}
