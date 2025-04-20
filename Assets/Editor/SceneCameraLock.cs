using UnityEditor;
using UnityEngine;

// This tool lets you toggle locking the Scene View camera pivot to the selected object.
// Use the menu item "Tools/Scene Camera Lock -> Toggle Lock" to enable/disable.
[InitializeOnLoad]
public static class SceneCameraLock
{
    private static Transform lockTarget;
    private static bool isLocked = false;

    static SceneCameraLock()
    {
        // Subscribe to the SceneView update
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Scene Camera Lock/Toggle Lock %#l")] // Ctrl/Cmd + Shift + L
    private static void ToggleLock()
    {
        if (Selection.activeTransform != null)
        {
            // Set or clear the lock target
            if (!isLocked)
            {
                lockTarget = Selection.activeTransform;
                isLocked = true;
                Debug.Log($"Scene Camera locked to: {lockTarget.name}");
            }
            else
            {
                isLocked = false;
                lockTarget = null;
                Debug.Log("Scene Camera unlocked");
            }
        }
        else
        {
            isLocked = false;
            lockTarget = null;
            Debug.Log("No object selected. Scene Camera unlocked.");
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (isLocked && lockTarget != null)
        {
            // Force the pivot of the Scene View to the target object's position
            sceneView.pivot = lockTarget.position;
            sceneView.Repaint();
        }
    }
}
