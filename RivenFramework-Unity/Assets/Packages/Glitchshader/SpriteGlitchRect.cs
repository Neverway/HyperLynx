using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Feeds the current sprite's UV bounds to the HexGlitch shader so it can work in
/// sprite-local 0..1 space (needed when sprites live in an atlas / sprite sheet).
/// Add next to a SpriteRenderer that uses a material with Custom/Sprites/HexGlitch.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteGlitchRect : MonoBehaviour
{
    static readonly int RectId = Shader.PropertyToID("_SpriteRect");
    static readonly int SeedId = Shader.PropertyToID("_Seed");
    static readonly int GlitchId = Shader.PropertyToID("_Glitch");
    static readonly Dictionary<Sprite, Vector4> RectCache = new Dictionary<Sprite, Vector4>();

    [Tooltip("Per-renderer glitch override. Leave off to use the material's value.")]
    public bool overrideGlitch;
    [Range(0f, 1f)] public float glitch = 0.6f;

    SpriteRenderer sr;
    MaterialPropertyBlock mpb;
    Sprite lastSprite;
    float seed;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        seed = Mathf.Abs(GetInstanceID() % 1000) * 0.001f;
        lastSprite = null;
        Apply();
    }

    void OnDisable()
    {
        if (sr == null) return;
        sr.SetPropertyBlock(null);
    }

    void LateUpdate()
    {
        if (sr.sprite != lastSprite || overrideGlitch) Apply();
    }

    void Apply()
    {
        lastSprite = sr.sprite;
        if (lastSprite == null) return;

        sr.GetPropertyBlock(mpb);
        mpb.SetVector(RectId, GetRect(lastSprite));
        mpb.SetFloat(SeedId, seed);
        if (overrideGlitch) mpb.SetFloat(GlitchId, glitch);
        sr.SetPropertyBlock(mpb);
    }

    static Vector4 GetRect(Sprite s)
    {
        if (RectCache.TryGetValue(s, out var r)) return r;

        Vector2[] uv = s.uv;
        Vector2 min = uv[0], max = uv[0];
        for (int i = 1; i < uv.Length; i++)
        {
            min = Vector2.Min(min, uv[i]);
            max = Vector2.Max(max, uv[i]);
        }
        r = new Vector4(min.x, min.y, max.x, max.y);
        RectCache[s] = r;
        return r;
    }
}
