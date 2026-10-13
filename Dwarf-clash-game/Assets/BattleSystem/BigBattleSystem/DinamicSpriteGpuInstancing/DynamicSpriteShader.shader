
Shader "Unlit/DynamicSpriteShader3"

{

    Properties

    {

        _MainTex ("Texture Atlas", 2D) = "white" {}

    }

    SubShader

    {

        // Используем теги для "вырезания" альфа-каналом. Это решает проблемы сортировки.

        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }

        LOD 100

        Blend Off // Блендинг не нужен для "вырезания"

        Cull Off

        ZWrite On // Включаем запись в Z-буфер, чтобы объекты правильно перекрывали друг друга



        Pass

        {

            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #pragma target 3.5



            #include "UnityCG.cginc"



            struct ParticleData {

                float3 position;

                float4 uvRect; // x, y, width, height

                float size;

                float flip;   // 0 for normal, 1 for flipped

                float active; // 1 for active, 0 for inactive

            };



            StructuredBuffer<ParticleData> _Particles;



            struct appdata

            {

                float4 vertex : POSITION;

                float2 uv : TEXCOORD0;

                uint instanceID : SV_InstanceID;

            };



            struct v2f

            {

                float2 uv : TEXCOORD0;

                float4 vertex : SV_POSITION;

            };



            sampler2D _MainTex;

            float4 _MainTex_ST;



            v2f vert (appdata v)

            {

                v2f o;



                ParticleData particle = _Particles[v.instanceID];



                // Если частица неактивна, схлопываем ее вершины, чтобы она не рисовалась

                if (particle.active < 0.5) {

                    o.vertex = float4(0, 0, 0, 0);

                    o.uv = 0;

                    return o;

                }



                // --- Correct Billboarding ---

                float3 centerWorldPos = particle.position;

                float3 vertexLocalPos = v.vertex.xyz * particle.size;



                float3 camRight = UNITY_MATRIX_I_V[0].xyz;

                float3 camUp = UNITY_MATRIX_I_V[1].xyz;



                float3 worldPos = centerWorldPos + (camRight * vertexLocalPos.x) + (camUp * vertexLocalPos.y);



                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));



                // --- UV Mapping with Flipping ---

                float2 modelUV = v.uv;

                if (particle.flip > 0.5) {

                    modelUV.x = 1.0 - modelUV.x; // Flip horizontally

                }



                o.uv.x = particle.uvRect.x + modelUV.x * particle.uvRect.z;

                o.uv.y = particle.uvRect.y + modelUV.y * particle.uvRect.w;



                return o;

            }



            fixed4 frag (v2f i) : SV_Target

            {

                fixed4 col = tex2D(_MainTex, i.uv);

                // Отбрасываем пиксели, у которых альфа-канал почти нулевой

                clip(col.a - 0.01);

                return col;

            }

            ENDCG

        }

    }

    FallBack "Transparent/VertexLit"

}


