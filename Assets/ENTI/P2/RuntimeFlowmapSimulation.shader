Shader "Hidden/RuntimeFlowmapSimulation"
{
    Properties
    {
        _PreviousFrame ("Previous Frame", 2D) = "white" {}

        _PlayerUV ("Player UV", Vector) = (0,0,0,0)
        _VelocityUV ("Velocity UV", Vector) = (0,0,0,0)

        _SimulationSize ("Simulation Size", Vector) = (20,0,20,0)

        _PlayerRadius ("Player Radius", Float) = 5
        _PlayerHardness ("Player Hardness", Float) = 0.3

        _BaseFlowDirection ("Base Flow Direction", Vector) = (1,0.25,0,0)
        _BaseFlowStrength ("Base Flow Strength", Float) = 1
        _Dissipation ("Dissipation", Float) = 0.025
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _PreviousFrame;

            float4 _PlayerUV;
            float4 _VelocityUV;

            float4 _SimulationSize;

            float _PlayerRadius;
            float _PlayerHardness;

            float4 _BaseFlowDirection;
            float _BaseFlowStrength;
            float _Dissipation;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                float4 previous = tex2D(_PreviousFrame, uv);

                float2 baseDir = _BaseFlowDirection.xy;

                if (length(baseDir) > 0.001)
                {
                    baseDir = normalize(baseDir);
                }
                else
                {
                    baseDir = float2(1.0, 0.0);
                }

                float2 encodedBase = (-baseDir * saturate(_BaseFlowStrength)) * 0.5 + 0.5;

                float4 neutral = float4(encodedBase.x, encodedBase.y, 0.0, 1.0);

                previous = lerp(previous, neutral, _Dissipation);

                float2 playerUV = _PlayerUV.xy;

                float2 simulationSize = max(_SimulationSize.xz, float2(0.001, 0.001));

                float2 diffUV = uv - playerUV;

                float2 diffWorld = diffUV * simulationSize;

                float distToPlayer = length(diffWorld);

                float radius = max(_PlayerRadius, 0.001);
                float innerRadius = radius * saturate(_PlayerHardness);

                float mask = 1.0 - smoothstep(innerRadius, radius, distToPlayer);

                float2 dir = _VelocityUV.xy;

                if (length(dir) > 0.001)
                {
                    dir = normalize(dir);
                }
                else
                {
                    dir = baseDir;
                }

                float2 encodedPlayer = (-dir) * 0.5 + 0.5;

                float4 paint = float4(encodedPlayer.x, encodedPlayer.y, 1.0, 1.0);

                float4 result = lerp(previous, paint, mask);

                return result;
            }
            ENDCG
        }
    }
}