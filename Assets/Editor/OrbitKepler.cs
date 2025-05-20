using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitMover))]
[CanEditMultipleObjects]
public class KeplerOrbitMoverEditor : Editor
{
    private OrbitMover _target;

    private void OnEnable()
    {
        _target = target as OrbitMover;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ApplyOrbitEditability();

        DrawMeanAnomalySection();

        DrawOrbitStatistics();

        GUI.enabled = true;

        ValidateAttractorSettings();
        ValidateOrbitDataConstants();
    }

    /// <summary>Вмикає/вимикає GUI в залежності від валідності орбіти та її ексцентриситету.</summary>
    private void ApplyOrbitEditability()
    {
        if (!_target.orbitData.IsValidOrbit || _target.orbitData.Eccentricity >= 1.0)
            GUI.enabled = false;
    }

    /// <summary>Малює слайдер Mean Anomaly або просто лейбл, якщо ексцентриситет ≥ 1.</summary>
    private void DrawMeanAnomalySection()
    {
        if (_target.orbitData.Eccentricity < 1.0)
        {
            float meanAnomaly = EditorGUILayout.Slider(
                "Mean anomaly",
                (float)_target.orbitData.MeanAnomaly,
                0f,
                (float)Utils.PI_2
            );

            if (meanAnomaly != (float)_target.orbitData.MeanAnomaly)
            {
                _target.orbitData.SetMeanAnomaly(meanAnomaly);
                _target.ForceUpdateViewFromInternalState();
                EditorUtility.SetDirty(_target);
            }
        }
        else
        {
            EditorGUILayout.LabelField(
                "Mean anomaly",
                _target.orbitData.MeanAnomaly.ToString()
            );
        }
    }

    /// <summary>Малює решту інформації про орбіту (швидкість, період, нахил, тощо).</summary>
    private void DrawOrbitStatistics()
    {
        EditorGUILayout.LabelField(
            "Velocity",
            _target.orbitData.velocityRelativeToAttractor.magnitude
                .ToString("0.00000")
        );
        EditorGUILayout.LabelField(
            "Period (days)",
            (_target.orbitData.Period / 3600.0 / 24.0).ToString("0.00000")
        );

        if (_target.AttractorSettings.AttractorObject != null)
        {
            double surfaceDist = _target.orbitData.AttractorDistance
                - _target.AttractorSettings.AttractorObject.transform.lossyScale.x / 2
                - _target.transform.lossyScale.x / 2;
            EditorGUILayout.LabelField(
                "Distance surface-to-surface",
                surfaceDist.ToString("0.00000")
            );
        }

        DrawAngleField(
            "Inclination",
            _target.orbitData.Inclination
        );
        DrawAngleField(
            "AscendingNodeLongitude",
            _target.orbitData.AscendingNodeLongitude
        );
        DrawAngleField(
            "ArgumentOfPerifocus",
            _target.orbitData.ArgumentOfPerifocus
        );

        EditorGUILayout.LabelField(
            "Current Orbit Time",
            _target.orbitData.GetCurrentOrbitTime().ToString("0.000")
        );
        EditorGUILayout.LabelField(
            "Current MeanMotion",
            _target.orbitData.MeanMotion.ToString("0.000")
        );
    }

    /// <summary>Допоміжний метод для відображення кута в радіанах і градусах.</summary>
    private void DrawAngleField(string label, double radians)
    {
        string radStr = radians.ToString("0.00000");
        string degStr = (radians * Utils.Rad2Deg).ToString("0.000");
        EditorGUILayout.LabelField(
            label,
            string.Format("{0,15} (deg={1})", radStr, degStr)
        );
    }

    /// <summary>Скидає AttractorObject, якщо він вказує на себе самого.</summary>
    private void ValidateAttractorSettings()
    {
        if (_target.AttractorSettings != null
            && _target.AttractorSettings.AttractorObject == _target.gameObject)
        {
            _target.AttractorSettings.AttractorObject = null;
            EditorUtility.SetDirty(_target);
        }
    }

    /// <summary>Гарантує, що константи гравітації не від’ємні.</summary>
    private void ValidateOrbitDataConstants()
    {
        if (_target.AttractorSettings.GravityConstant < 0)
        {
            _target.AttractorSettings.GravityConstant = 0;
            EditorUtility.SetDirty(_target);
        }
        if (_target.orbitData.GravConst < 0)
        {
            _target.orbitData.GravConst = 0;
            EditorUtility.SetDirty(_target);
        }
    }
}