Shader "Custom/Sprites/HexGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        [Header(Master)]
        _Glitch ("Glitch Amount", Range(0,1)) = 0.6
        _Rate ("Glitch Steps Per Second", Range(1,60)) = 12
        _Burstiness ("Burstiness (0 = constant)", Range(0,1)) = 0.7
        _BurstRate ("Burst Checks Per Second", Range(0.1,10)) = 2
        _BurstChance ("Burst Chance", Range(0,1)) = 0.35

        [Header(Slice Shift)]
        _SliceRows ("Slice Rows", Range(2,64)) = 24
        _SliceChance ("Slice Chance", Range(0,1)) = 0.25
        _SliceShift ("Slice Max Shift", Range(0,0.5)) = 0.15

        [Header(Block Scramble)]
        _BlockGrid ("Block Grid (x, y)", Vector) = (8,8,0,0)
        _ScrambleChance ("Scramble Chance", Range(0,1)) = 0.2

        [Header(Chroma Split)]
        _ChromaSplit ("Chroma Split", Range(0,0.2)) = 0.04

        [Header(Hex Overlay)]
        _HexCells ("Hex Cells (cols, rows)", Vector) = (8,8,0,0)
        _HexAmount ("Hex Coverage", Range(0,1)) = 0.25
        _HexRate ("Hex Changes Per Second", Range(1,30)) = 8
        _HexColor ("Hex Color", Color) = (0.2,1,0.4,1)
        [Toggle] _HexOnSprite ("Hex Only On Sprite Pixels", Float) = 0

        [Header(Scanlines)]
        _Scanline ("Scanline Strength", Range(0,1)) = 0.15
        _ScanlineCount ("Scanline Count", Range(8,256)) = 64

        [HideInInspector][PerRendererData] _SpriteRect ("Sprite UV Rect (minU, minV, maxU, maxV)", Vector) = (0,0,1,1)
        [HideInInspector][PerRendererData] _Seed ("Seed", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _Glitch, _Rate, _Burstiness, _BurstRate, _BurstChance;
            float _SliceRows, _SliceChance, _SliceShift;
            float4 _BlockGrid;
            float _ScrambleChance;
            float _ChromaSplit;
            float4 _HexCells;
            float _HexAmount, _HexRate;
            fixed4 _HexColor;
            float _HexOnSprite;
            float _Scanline, _ScanlineCount;
            float4 _SpriteRect;
            float _Seed;

            static const uint FONT[16] =
            {
                0x7B6Fu, 0x2C97u, 0x73E7u, 0x73CFu,
                0x5BC9u, 0x79CFu, 0x79EFu, 0x7252u,
                0x7BEFu, 0x7BCFu, 0x2BEDu, 0x6BAEu,
                0x3923u, 0x6B6Eu, 0x79A7u, 0x79A4u
            };

            uint pcg(uint v)
            {
                uint s = v * 747796405u + 2891336453u;
                uint w = ((s >> ((s >> 28u) + 4u)) ^ s) * 277803737u;
                return (w >> 22u) ^ w;
            }

            float rand(uint a, uint b, uint c)
            {
                return pcg(a ^ pcg(b + pcg(c))) * (1.0 / 4294967295.0);
            }

            fixed4 SampleLocal(float2 p)
            {
                float2 rsize = max(_SpriteRect.zw - _SpriteRect.xy, 1e-5);
                float inside = step(0.0, p.x) * step(p.x, 1.0) * step(0.0, p.y) * step(p.y, 1.0);
                float2 uv = _SpriteRect.xy + saturate(p) * rsize;
                return SampleSpriteTexture(uv) * inside;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 rsize = max(_SpriteRect.zw - _SpriteRect.xy, 1e-5);
                float2 p0 = (IN.texcoord - _SpriteRect.xy) / rsize;
                float2 p = p0;

                uint tt = (uint)floor(_Time.y * _Rate) + (uint)(_Seed * 977.0);
                uint tb = (uint)floor(_Time.y * _BurstRate) + (uint)(_Seed * 313.0);
                float gate = step(rand(tb, 1u, 99u), _BurstChance);
                float act = _Glitch * lerp(1.0, gate, _Burstiness);

                uint row = (uint)floor(saturate(p.y) * _SliceRows);
                if (rand(row, tt, 11u) < act * _SliceChance)
                {
                    p.x += (rand(row, tt, 12u) * 2.0 - 1.0) * _SliceShift;
                    p.x = frac(p.x);
                }

                float2 grid = max(_BlockGrid.xy, 1.0);
                float2 bid = floor(saturate(p) * grid);
                uint bi = (uint)bid.x + (uint)bid.y * 131u;
                float swapped = 0.0;
                if (rand(bi, tt, 21u) < act * _ScrambleChance)
                {
                    float2 nb = floor(float2(rand(bi, tt, 22u), rand(bi, tt, 23u)) * grid);
                    p = (nb + frac(p * grid)) / grid;
                    swapped = 1.0;
                }

                float off = _ChromaSplit * act * (rand(tt, 3u, 31u) * 2.0 - 1.0);
                fixed4 cR = SampleLocal(p + float2(off, 0));
                fixed4 cG = SampleLocal(p);
                fixed4 cB = SampleLocal(p - float2(off, 0));
                fixed4 col = fixed4(cR.r, cG.g, cB.b, max(cR.a, max(cG.a, cB.a)));

                if (swapped > 0.5)
                {
                    float r = rand(bi, tt, 24u);
                    if (r < 0.25)      col.rgb = 1.0 - col.rgb;
                    else if (r < 0.5)  col.rgb = col.gbr;
                }

                col *= IN.color;

                col.rgb *= 1.0 - _Scanline * step(0.5, frac(p0.y * _ScanlineCount));

                float2 hc = max(_HexCells.xy, 1.0);
                float2 hp = p0 * hc;
                float2 cid = floor(hp);
                float2 cf = frac(hp);
                uint th = (uint)floor(_Time.y * _HexRate) + (uint)(_Seed * 571.0);
                uint ci = (uint)cid.x + (uint)cid.y * 257u;

                float hexOn = 0.0;
                float inGrid = step(0.0, p0.x) * step(p0.x, 1.0) * step(0.0, p0.y) * step(p0.y, 1.0);
                if (inGrid > 0.5 && rand(ci, th, 41u) < act * _HexAmount)
                {
                    uint gi = (uint)(rand(ci, th, 42u) * 16.0) & 15u;
                    uint2 q = (uint2)floor(cf * float2(4.0, 6.0));
                    if (q.x < 3u && q.y >= 1u)
                    {
                        uint bit = (q.y - 1u) * 3u + (2u - q.x);
                        hexOn = (float)((FONT[gi] >> bit) & 1u);
                    }
                }

                float onSprite = step(0.01, col.a);
                float hexMask = hexOn * lerp(1.0, onSprite, _HexOnSprite);
                col.rgb = lerp(col.rgb, _HexColor.rgb, hexMask);
                col.a = max(col.a, hexMask * _HexColor.a);

                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
