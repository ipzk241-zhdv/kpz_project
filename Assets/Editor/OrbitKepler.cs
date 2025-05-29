using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitMover))]
[CanEditMultipleObjects]
public class KeplerOrbitMoverEditor : Editor
{
    private OrbitMover _target;

    private void OnEnable()
    {
        _target = (OrbitMover)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawMeanAnomalySlider();
        DrawOrbitDetails();
        ValidateAndFixSettings();
    }

    private void DrawMeanAnomalySlider()
    {
        if (!_target.orbitData.IsValidOrbit || _target.orbitData.Eccentricity >= 1.0f)
        {
            EditorGUILayout.LabelField("Mean anomaly", _target.orbitData.MeanAnomaly.ToString());
            return;
        }

        float meanAnomaly = EditorGUILayout.Slider("Mean anomaly",
            (float)_target.orbitData.MeanAnomaly, 0, (float)Utils.PI_2);

        if (!Mathf.Approximately(meanAnomaly, (float)_target.orbitData.MeanAnomaly))
        {
            _target.orbitData.SetMeanAnomaly(meanAnomaly);
            _target.ForceUpdateViewFromInternalState();
            EditorUtility.SetDirty(_target);
        }
    }

    private void DrawOrbitDetails()
    {
        if (_target.orbitData == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Velocity", _target.orbitData.velocityRelativeToAttractor.magnitude.ToString("0.00000"));

        float periodInDays = _target.orbitData.Period / 60f / 60f / 24f;
        EditorGUILayout.LabelField("Period", periodInDays.ToString("0.00000"));

        if (_target.AttractorSettings?.AttractorObject != null)
        {
            float attractorRadius = _target.AttractorSettings.AttractorObject.transform.lossyScale.x / 2;
            float objectRadius = _target.transform.lossyScale.x / 2;
            float surfaceDistance = _target.orbitData.AttractorDistance - attractorRadius - objectRadius;

            EditorGUILayout.LabelField("Distance surface to surface", surfaceDistance.ToString("0.00000"));
        }

        DrawAngleField("Inclination", _target.orbitData.Inclination);
        DrawAngleField("AscendingNodeLongitude", _target.orbitData.AscendingNodeLongitude);
        DrawAngleField("ArgumentOfPerifocus", _target.orbitData.ArgumentOfPerifocus);

        EditorGUILayout.LabelField("Current Orbit Time", _target.orbitData.GetCurrentOrbitTime().ToString("0.000"));
        EditorGUILayout.LabelField("Current MeanMotion", _target.orbitData.MeanMotion.ToString("0.000"));
    }

    private void DrawAngleField(string label, double radians)
    {
        string radiansStr = radians.ToString();
        string degreesStr = (radians * Utils.Rad2Deg).ToString("0.000");
        EditorGUILayout.LabelField(label, $"{radiansStr,15} (deg={degreesStr})");
    }

    private void ValidateAndFixSettings()
    {
        if (_target.AttractorSettings != null)
        {
            if (_target.AttractorSettings.AttractorObject == _target.gameObject)
            {
                _target.AttractorSettings.AttractorObject = null;
                EditorUtility.SetDirty(_target);
            }

            if (_target.AttractorSettings.GravityConstant < 0)
            {
                _target.AttractorSettings.GravityConstant = 0;
                EditorUtility.SetDirty(_target);
            }
        }

        if (_target.orbitData.GravConst < 0)
        {
            _target.orbitData.GravConst = 0;
            EditorUtility.SetDirty(_target);
        }
    }
}
