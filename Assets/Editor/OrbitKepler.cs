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

        if (!_target.orbitData.IsValidOrbit)
        {
            GUI.enabled = false;
        }

        //if (GUILayout.Button("Circularize orbit"))
        //{
        //    _target.SetAutoCircleOrbit();
        //}

        if (_target.orbitData.Eccentricity >= 1.0)
        {
            GUI.enabled = false;
        }

        if (_target.orbitData.Eccentricity < 1.0)
        {
            float meanAnomaly = EditorGUILayout.Slider("Mean anomaly", (float)_target.orbitData.MeanAnomaly, 0, (float)Utils.PI_2);
            if (meanAnomaly != (float)_target.orbitData.MeanAnomaly)
            {
                _target.orbitData.SetMeanAnomaly(meanAnomaly);
                _target.ForceUpdateViewFromInternalState();
                EditorUtility.SetDirty(_target);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Mean anomaly", _target.orbitData.MeanAnomaly.ToString());
        }

        if (_target.orbitData.IsValidOrbit && _target.orbitData.Eccentricity >= 1.0)
        {
            GUI.enabled = true;
        }

        EditorGUILayout.LabelField("Velocity", _target.orbitData.velocityRelativeToAttractor.magnitude.ToString("0.00000"));

        string inclinationRad = _target.orbitData.Inclination.ToString();
        string inclinationDeg = (_target.orbitData.Inclination * Utils.Rad2Deg).ToString("0.000");
        EditorGUILayout.LabelField("Inclination", string.Format("{0,15} (deg={1})", inclinationRad, inclinationDeg));

        string ascNodeRad = _target.orbitData.AscendingNodeLongitude.ToString();
        string ascNodeDeg = (_target.orbitData.AscendingNodeLongitude * Utils.Rad2Deg).ToString("0.000");
        EditorGUILayout.LabelField("AscendingNodeLongitude", string.Format("{0,15} (deg={1})", ascNodeRad, ascNodeDeg));

        string argOfPeriRad = _target.orbitData.ArgumentOfPerifocus.ToString();
        string argOfPeriDeg = (_target.orbitData.ArgumentOfPerifocus * Utils.Rad2Deg).ToString("0.000");
        EditorGUILayout.LabelField("ArgumentOfPerifocus", string.Format("{0,15} (deg={1})", argOfPeriRad, argOfPeriDeg));

        EditorGUILayout.LabelField("Current Orbit Time", _target.orbitData.GetCurrentOrbitTime().ToString("0.000"));

        EditorGUILayout.LabelField("Current MeanMotion", _target.orbitData.MeanMotion.ToString("0.000"));

        GUI.enabled = true;

        if (_target.AttractorSettings != null && _target.AttractorSettings.AttractorObject == _target.gameObject)
        {
            _target.AttractorSettings.AttractorObject = null;
            EditorUtility.SetDirty(_target);
        }

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