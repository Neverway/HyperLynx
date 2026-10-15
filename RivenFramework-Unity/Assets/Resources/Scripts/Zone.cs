//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using DG.Tweening;
using RivenFramework;

public class Zone : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public MusicTracks musicToPlayOnZoneLoaded;
    
    [Header("Layout")]
    [Tooltip("Used in the inspector to set how many layers of depth there are for this zone, each depth layer is like it's own screen full of platforms and obstacles and such")]
    public int depthCount = 5;

    [Tooltip("Used in the inspector to control how many layers of depth backwards until the background is drawn")]
    public int backgroundDrawDepth = 4;
    
    [Tooltip("Normally the layer overlapping the depth of the background is hidden, enable this to always draw the layer that overlaps the background")]
    public bool drawLayerOverlappingBackground = false;
    
    [Tooltip("If enabled, layers beyond the background layer will not be hidden")]
    public bool drawLayersBeyondBackground = false;
    
    [Tooltip("If enabled, layers behind the current layer will not be hidden")]
    public bool drawLayersBehindCurrent = false;
    
    [Tooltip("If enabled, the current depth cant go past the point where the background would exceed the layer count")]
    public bool limitDepthToBackground = false;

    [Tooltip("Used in the inspector to set the offset applied to each depth layer (for example, the default is 0,0,1, so the current layer is drawn at 0,0,0, then 0,0,1, then 0,0,2... you get the idea)")]
    public Vector3 depthLayerOffset = new Vector3(0, 0, 1);

    [Header("Background and Tinting")]
    [Tooltip("The image to draw for the background on this zone")]
    public Sprite background;

    [Tooltip("If enabled, all sprite renderers in each visible layer are affected by depth tinting")]
    public bool tintSprites = true;

    [Tooltip("How to color each depth layer, the left represents the side closest to the screen, and the right is the furthest visible layer")]
    public Gradient layerDepthTint = CreateDefaultGradient();
    
    [Tooltip("How to color each sprite on each depth layer, the left represents the side closest to the screen, and the right is the furthest visible layer")]
    public Gradient spriteDepthTint = CreateDefaultGradient();
    
    public Gradient behindLayerDepthTint = CreateDefaultGradient();
    public Gradient behindSpriteDepthTint = CreateDefaultGradient();
    
    [Header("Stepping")]
    
    public float stepDuration = 0.25f;

    public Ease stepEase = Ease.OutCubic;

    public bool allowStepInterrupt = true;

    public bool stepIgnoresTimeScale = false;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Header("Debugging")]
    [Tooltip("Controlled by the DepthController, this is what depth the player is currently at in the zone, so we know how to draw everything else")]
    public int currentDepth;
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("A list of all of the currently created depth layers, used for drawing and whatnot")]
    public List<GameObject> depthLayers = new List<GameObject>();
    private const string LayersHolderName = "Depth Layers";
    private const string OrphanHolderName = "Removed Layer Content";
    #if UNITY_EDITOR
    private bool rebuildQueued;
    #endif
    
    // Step stuffs
    private float visualDepth;
    private bool isShifting;
    private Tween stepTween;
    public bool IsShifting => isShifting;
    public event Action<int> OnDepthChanged;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Header("References")]
    [Tooltip("This is the object that is used to represent the bounds for each depth layer, a new one of these is created for each depth count in the inspector")]
    public GameObject depthLayerPrefab;
    [Tooltip("Used so we can set the draw depth of the background")] 
    public GameObject backgroundObject;
    [Tooltip("Used so we can set the background image")]
    public SpriteRenderer backgroundSpriteRenderer;



    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        ApplyLayout();
        
        GameInstance.Get<MusicList>().CrossFadeToTrack(musicToPlayOnZoneLoaded, 0.5f);
    }

    private void OnValidate()
    {
        depthCount = Mathf.Max(1, depthCount);
        backgroundDrawDepth = Mathf.Max(0, backgroundDrawDepth);
        currentDepth = Mathf.Clamp(currentDepth, 0, GetMaxDepth(depthCount));
        
        #if UNITY_EDITOR
        if (Application.isPlaying || rebuildQueued) return;
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        rebuildQueued = true;
        EditorApplication.delayCall += () =>
        {
            rebuildQueued = false;
            if (this == null || Application.isPlaying) return;
            SyncLayers();
            ApplyLayout();
        };
        #endif
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private static Gradient CreateDefaultGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.35f, 0.4f, 0.55f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        return gradient;
    }
    
    private static void UndoRecord(Object o)
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying) Undo.RecordObject(o, "Update Zone");
        #endif
    }

    private void ApplyBackground()
    {
        if (backgroundObject == null) return;
        
        int drawIndex = Mathf.Max(backgroundDrawDepth, currentDepth);
        //Vector3 localPos = depthLayerOffset * (drawIndex - currentDepth);
        Vector3 localPos = depthLayerOffset * backgroundDrawDepth;
        Vector3 worldPos = transform.TransformPoint(localPos);
        
        Transform backgroundObjectTransform = backgroundObject.transform;
        if (backgroundObjectTransform.position != worldPos)
        {
            UndoRecord(backgroundObjectTransform);
            backgroundObjectTransform.position = worldPos;
        }

        if (backgroundSpriteRenderer != null)
        {
            if (backgroundSpriteRenderer.sprite != background)
            {
                UndoRecord(backgroundSpriteRenderer);
                backgroundSpriteRenderer.sprite = background;
            }
        }
    }

#if UNITY_EDITOR
    private void SyncLayers()
    {
        if (depthLayerPrefab == null) return;

        depthLayers.RemoveAll(l => l == null);
        Undo.RecordObject(this, "Resize Zone Depth");
        AdoptExistingLayers();
        
        Transform layersHolder = GetOrCreateLayersHolder();

        while (depthLayers.Count < depthCount)
        {
            GameObject layer = PrefabUtility.IsPartOfPrefabAsset(depthLayerPrefab)
                ? (GameObject)PrefabUtility.InstantiatePrefab(depthLayerPrefab, layersHolder)
                : Instantiate(depthLayerPrefab, layersHolder);
            Undo.RegisterCreatedObjectUndo(layer, "Add Depth Layer");
            depthLayers.Add(layer);
        }

        while (depthLayers.Count > depthCount)
        {
            RemoveLayer(depthLayers.Count - 1);
        }
    }

    private void RemoveLayer(int index)
    {
        GameObject layer = depthLayers[index];
        depthLayers.RemoveAt(index);

        var defaultNames = new HashSet<string>();
        foreach (Transform content in depthLayerPrefab.transform)
        {
            defaultNames.Add(content.name);
        }

        var userContent = new List<Transform>();
        foreach (Transform content in layer.transform)
        {
            if (!defaultNames.Contains(content.name))
            {
                userContent.Add(content);
            }
        }

        if (userContent.Count > 0)
        {
            Transform holder = GetOrCreateOrphanHolder();
            var group = new GameObject($"Removed Layer {index}");
            Undo.RegisterCreatedObjectUndo(group, "Keep Removed Layer Content");
            group.transform.SetParent(holder, false);

            foreach (Transform c in userContent)
            {
                Undo.SetTransformParent(c, group.transform, "Keep Removed Layer Content");
            }
        }

        Undo.DestroyObjectImmediate(layer);
    }

    private Transform GetOrCreateOrphanHolder()
    {
        Transform holder = transform.Find(OrphanHolderName);
        if (holder == null)
        {
            var go = new GameObject(OrphanHolderName);
            Undo.RegisterCreatedObjectUndo(go, "Create Orphan Holder");
            go.transform.SetParent(transform, false);
            holder = go.transform;
        }

        if (holder.gameObject.activeSelf)
        {
            Undo.RecordObject(holder.gameObject, "Hide Orphan Holder");
            holder.gameObject.SetActive(false);
        }

        return holder;
    }
    
    private void AdoptExistingLayers()
    {
        Transform holder = GetOrCreateLayersHolder();

        var found = new List<KeyValuePair<int, GameObject>>();
        CollectLayers(holder, found);
        CollectLayers(transform, found);

        found.Sort((a, b) => a.Key.CompareTo(b.Key));

        depthLayers.Clear();
        foreach (var pair in found)
        {
            Transform t = pair.Value.transform;
            if (t.parent != holder)
            {
                Undo.SetTransformParent(t, holder, "Move Depth Layer Into Holder");
            }
            depthLayers.Add(pair.Value);
        }
    }
    
    [ContextMenu("Refresh Zone")]
    public void RefreshZone()
    {
        if (Application.isPlaying) return;

        rebuildQueued = false;
        SyncLayers();
        ApplyLayout();
        EditorUtility.SetDirty(this);
    }
    
    private Transform GetOrCreateLayersHolder()
    {
        Transform holder = transform.Find(LayersHolderName);
        if (holder == null)
        {
            var go = new GameObject(LayersHolderName);
            Undo.RegisterCreatedObjectUndo(go, "Create Depth Layers Holder");
            go.transform.SetParent(transform, false);
            holder = go.transform;
        }
        return holder;
    }

    private static void CollectLayers(Transform parent, List<KeyValuePair<int, GameObject>> results)
    {
        const string prefix = "Depth Layer ";
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith(prefix) && int.TryParse(child.name.Substring(prefix.Length), out int n))
            {
                results.Add(new KeyValuePair<int, GameObject>(n, child.gameObject));
            }
        }
    }
#endif
    
    private int GetMaxDepth(int layerCount)
    {
        int max = layerCount - 1;
        if (limitDepthToBackground) max = Mathf.Min(max, layerCount - backgroundDrawDepth);
        return Mathf.Max(0, max);
    }

    public int MaxDepth => GetMaxDepth(depthLayers.Count);

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void ApplyLayout()
    {
        depthLayers.RemoveAll(l => l == null);
        int count = depthLayers.Count;
        if (count == 0) return;

        currentDepth = Mathf.Clamp(currentDepth, 0, GetMaxDepth(count));
        if (!isShifting) visualDepth = currentDepth;

        int lowDepth = Mathf.FloorToInt(visualDepth);
        int highDepth = Mathf.CeilToInt(visualDepth);
        int lastVisibleLayerIndex = highDepth + (drawLayerOverlappingBackground ? backgroundDrawDepth : backgroundDrawDepth - 1);
        int gradientSpan = backgroundDrawDepth - 1;

        for (int i = 0; i < count; i++)
        {
            GameObject layer = depthLayers[i];
            Transform layerTransform = layer.transform;

            // Name
            string layerName = $"Depth Layer {i}";
            if (layer.name != layerName)
            {
                UndoRecord(layer);
                layer.name = layerName;
            }

            // Position
            float relativeDepth = i - visualDepth;
            Vector3 position = depthLayerOffset * relativeDepth;
            if (layerTransform.localPosition != position)
            {
                UndoRecord(layerTransform);
                layerTransform.localPosition = position;
            }

            // Visibility
            bool passesCurrentCheck = i >= lowDepth || drawLayersBehindCurrent;
            bool passesBackgroundCheck = i <= lastVisibleLayerIndex || drawLayersBeyondBackground;
            bool visible = passesCurrentCheck && passesBackgroundCheck;
            if (layer.activeSelf != visible)
            {
                UndoRecord(layer);
                layer.SetActive(visible);
            }

            // Tint
            if (visible && tintSprites)
            {
                Color layerTint;
                Color spriteTint;

                if (relativeDepth >= 0f)
                {
                    float t = gradientSpan <= 0 ? (relativeDepth > 0f ? 1f : 0f) : Mathf.Clamp01(relativeDepth / gradientSpan);
                    layerTint = layerDepthTint.Evaluate(t);
                    spriteTint = spriteDepthTint.Evaluate(t);
                }
                else
                {
                    float distanceBehind = -relativeDepth;
                    float t = gradientSpan <= 0 ? 1f : Mathf.Clamp01(distanceBehind / gradientSpan);

                    float blend = Mathf.Clamp01(distanceBehind);
                    layerTint = Color.Lerp(layerDepthTint.Evaluate(0f), behindLayerDepthTint.Evaluate(t), blend);
                    spriteTint = Color.Lerp(spriteDepthTint.Evaluate(0f), behindSpriteDepthTint.Evaluate(t), blend);
                }

                foreach (SpriteRenderer renderer in layer.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    Color tint = (renderer.transform == layerTransform) ? layerTint : spriteTint;
                    if (renderer.color != tint)
                    {
                        UndoRecord(renderer);
                        renderer.color = tint;
                    }
                }
            }
        }

        ApplyBackground();
    }
    
    public bool StepDepth(int direction)
    {
        return GoToDepth(currentDepth + direction);
    }
    
    public bool GoToDepth(int targetDepth)
    {
        if (depthLayers.Count == 0) return false;

        targetDepth = Mathf.Clamp(targetDepth, 0, GetMaxDepth(depthLayers.Count));
        if (targetDepth == currentDepth) return false;
        if (isShifting && !allowStepInterrupt) return false;

        currentDepth = targetDepth;
        OnDepthChanged?.Invoke(currentDepth);

        stepTween?.Kill();

        float distance = Mathf.Abs(targetDepth - visualDepth);
        float duration = stepDuration * distance;

        if (!Application.isPlaying || duration <= 0f)
        {
            isShifting = false;
            ApplyLayout();
            return true;
        }

        isShifting = true;
        stepTween = DOTween.To(() => visualDepth, value => { visualDepth = value; ApplyLayout(); }, (float)targetDepth, duration)
            .SetEase(stepEase)
            .SetUpdate(stepIgnoresTimeScale)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                isShifting = false;
                ApplyLayout();
            });

        return true;
    }

    #endregion
}
