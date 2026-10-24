
using DG.DOTweenEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutscene))]
public class CutsceneEditor : Editor
{
    private static Cutscene previewing;
    private static double lastTime;

    public override void OnInspectorGUI()
    {
        var cutscene = (Cutscene)target;

        DrawDefaultInspector();
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (previewing == cutscene)
            {
                string label = cutscene.IsPlaying ? "■ Stop & Reset Preview" : "■ Reset";
                if (GUILayout.Button(label, GUILayout.Height(28))) StopPreview();
            }
            else if (GUILayout.Button("▶ Preview Cutscene", GUILayout.Height(28)))
            {
                StartPreview(cutscene);
            }
        }
    }

    private static void StartPreview(Cutscene _cutscene)
    {
        if (previewing != null) StopPreview();
        previewing = _cutscene;

        DOTweenEditorPreview.Start();
        var context = new CutsceneContext
        {
            IsPreview = true,
            PrepareTween = t => DOTweenEditorPreview.PrepareTweenForPreview(t)
        };

        lastTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += PreviewUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        AssemblyReloadEvents.beforeAssemblyReload += StopPreview;

        _cutscene.Begin(context);
    }

    private static void PreviewUpdate()
    {
        if (previewing == null)
        {
            StopPreview();
            return;
        }

        double now = EditorApplication.timeSinceStartup;
        float dt = (float)(now - lastTime);
        lastTime = now;

        previewing.Tick(dt);

        foreach (var animator in previewing.Context.PreviewAnimators)
            if (animator) animator.Update(dt);

        if (!previewing.IsPlaying) EditorApplication.update -= PreviewUpdate;

        SceneView.RepaintAll();
    }

    private static void OnPlayModeChanged(PlayModeStateChange _state)
    {
        if (_state == PlayModeStateChange.ExitingEditMode) StopPreview();
    }

    private static void StopPreview()
    {
        EditorApplication.update -= PreviewUpdate;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        AssemblyReloadEvents.beforeAssemblyReload -= StopPreview;

        DOTweenEditorPreview.Stop();
        if (previewing != null) previewing.Stop();
        previewing = null;

        SceneView.RepaintAll();
    }
}