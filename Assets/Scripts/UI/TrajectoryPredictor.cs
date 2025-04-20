using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Визуализация")]
    public int resolution = 100;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.green;

    [Header("Гравитационное тело")]
    public GravityPlanet gravitySource;

    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private GravityConfig config;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = resolution;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        config = GravityConfig.Instance
               ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");
        if (config == null)
            Debug.LogError("GravityConfig is missing.");
    }

    void LateUpdate()
    {
        if (gravitySource == null || config == null)
            return;

        DrawOrbit();
    }

    void DrawOrbit()
    {
        Vector3 r = transform.localPosition;
        Vector3 v = transform.parent.InverseTransformDirection(rb.velocity);

        float mu = (float)(config.gravitationalConstant * gravitySource.GetMass());

        float speed2 = v.sqrMagnitude;
        float rMag = r.magnitude;
        float energy = speed2 / 2f - mu / rMag;
        float a = -mu / (2f * energy);

        if (a < 0 || float.IsInfinity(a)) return; // Неэллиптическая орбита

        Vector3 h = Vector3.Cross(r, v);
        Vector3 eVec = Vector3.Cross(v, h) / mu - r.normalized;
        float e = eVec.magnitude;
        if (e >= 1f) return; // Неэллиптическая орбита

        Vector3 pericenterDir = eVec.normalized;
        float b = a * Mathf.Sqrt(1 - e * e);

        Vector3 focus = Vector3.zero;
        Vector3 center = -pericenterDir * (a * e);

        Vector3[] points = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float theta = (2 * Mathf.PI * i) / resolution;
            float x = a * Mathf.Cos(theta);
            float y = b * Mathf.Sin(theta);

            Vector3 point = new Vector3(x, y, 0);
            point += center; // смещение от фокуса к центру

            Quaternion rot = Quaternion.FromToRotation(Vector3.right, pericenterDir);
            point = rot * point;

            points[i] = transform.parent.TransformPoint(point);
        }

        lineRenderer.positionCount = resolution;
        lineRenderer.SetPositions(points);
    }
}
