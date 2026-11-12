Shader "Custom/WeldingSDF"
{
    Properties
    {
        _MainColor("Color", Color) = (1,1,1,1)
        _Gloss("Gloss", Range(0,1)) = 0.9
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Spheres[64];
            int _SphereCount;
            fixed4 _MainColor;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); 
                UNITY_INITIALIZE_OUTPUT(v2f, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Smooth Min
            float smin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }

            // Calculate distance
            float GetDist(float3 p)
            {
                
                float globalDist = 1000.0; 
                float weldStrength = 0.015;  

                for (int i = 0; i < _SphereCount; i++)
                {
                    float4 s = _Spheres[i];
                    float sphereDist = length(p - s.xyz) - s.w;

                    // Smoothly blend this sphere
                    globalDist = smin(globalDist, sphereDist, weldStrength);
                }
                return globalDist;
                
            }

            // lighting calculation
            float3 GetNormal(float3 p)
            {
                float d = GetDist(p);
                float2 e = float2(0.01, 0);
                float3 n = d - float3(
                    GetDist(p - e.xyy),
                    GetDist(p - e.yxy),
                    GetDist(p - e.yyx));
                return normalize(n);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.worldPos - rayOrigin);

                float t = 0;
                // Raymarching Loop
                for (int j = 0; j < 64; j++)
                {
                    float3 p = rayOrigin + rayDir * t;
                    float d = GetDist(p);

                    if (d < 0.01) 
                    {
                        float3 n = GetNormal(p);
                        float3 lightDir = normalize(float3(1,1,-1));
                        float diff = max(dot(n, lightDir), 0.0);
                        return _MainColor * (diff + 0.2);
                    }

                    t += d;
                    if (t > 20.0) break; 
                }

                discard; 
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}