using UnityEditor;
using UnityEngine;

/// <summary>
/// Locks the Scene View camera pivot to the selected object.
/// Use the menu item "Tools/Scene Camera Lock -> Toggle Lock" to enable/disable.
/// </summary>
[InitializeOnLoad]
public static class SceneCameraLock
{
    private static Transform _lockTarget;
    private static bool _isLocked;

    static SceneCameraLock()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Scene Camera Lock/Toggle Lock %#`")] // Ctrl/Cmd + Shift + `
    private static void ToggleLock()
    {
        if (_isLocked)
        {
            UnlockCamera();
        }
        else if (Selection.activeTransform != null)
        {
            LockToTarget(Selection.activeTransform);
        }
        else
        {
            Debug.Log("No object selected. Scene Camera remains unlocked.");
        }
    }

    private static void LockToTarget(Transform target)
    {
        _lockTarget = target;
        _isLocked = true;
        Debug.Log($"Scene Camera locked to: {_lockTarget.name}");
    }

    private static void UnlockCamera()
    {
        if (_lockTarget != null)
            Debug.Log("Scene Camera unlocked");

        _lockTarget = null;
        _isLocked = false;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (_isLocked && _lockTarget != null)
        {
            sceneView.pivot = _lockTarget.position;
            sceneView.Repaint();
        }
    }
}