using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer), typeof(OrbitalMotion))]
public class OrbitRenderer : MonoBehaviour
{
    [Header("Orbit Visualization")]
    public OrbitalMotion orbitalMotion;
    [Tooltip("Number of line segments for the orbit ellipse")] public int segments = 180;
    [Tooltip("Thickness of the orbit line")] public float lineWidth = 20000f;
    [Tooltip("Orbit line color")] public Color orbitColor = Color.cyan;

    [Header("Auto Redraw (for tidal-locked satellites)")]
    public bool autoRedrawForTidalLock = true;
    [Tooltip("Distance (units) planet can move before orbit is redrawn")] public float redrawThresholdDistance = 1000f;

    private LineRenderer lineRenderer;
    private float redrawTimer;
    private float redrawInterval;

    void OnEnable()
    {
        orbitalMotion = orbitalMotion ?? GetComponent<OrbitalMotion>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(orbitColor, 0f),
                new GradientColorKey(orbitColor, 0.02f),
                new GradientColorKey(orbitColor, 0.5f),
                new GradientColorKey(orbitColor, 0.98f),
                new GradientColorKey(orbitColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(1f, 0.98f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        lineRenderer.colorGradient = gradient;

        lineRenderer.positionCount = segments;
        DrawOrbit();

        if (autoRedrawForTidalLock && orbitalMotion.tidalLock)
        {
            var centralBody = orbitalMotion.centralBody.GetComponent<CelestialBody>();
            float speed = centralBody.orbitalVelocity; // units per sec
            redrawInterval = redrawThresholdDistance / Mathf.Max(speed, 0.0001f);
            redrawTimer = 0f;
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (orbitalMotion != null)
                DrawOrbit();
        }
    }

    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        if (autoRedrawForTidalLock && orbitalMotion.tidalLock)
        {
            redrawTimer += Time.deltaTime;
            if (redrawTimer >= redrawInterval)
            {
                DrawOrbit();
                redrawTimer = 0f;
            }
        }
    }

    public void DrawOrbit()
    {
        if (orbitalMotion == null || orbitalMotion.centralBody == null) return;
        var body = orbitalMotion.GetComponent<CelestialBody>();
        float period = body.orbitalPeriod;

        Vector3[] pts = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float t = (i / (float)segments) * period;
            pts[i] = orbitalMotion.GetWorldPositionAtTime(t);
        }

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetPositions(pts);
        Debug.Log($"Redraw orbit for {name}");
    }
}
