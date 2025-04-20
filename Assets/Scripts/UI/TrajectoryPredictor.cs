using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class TrajectoryPredictor : MonoBehaviour
{
    public int steps = 300;
    public float timeStep = 0.1f;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.green;
    public GravityConfig config;

    public Transform gravitySource;

    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private Rigidbody planetRb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = steps;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        planetRb = gravitySource.GetComponent<Rigidbody>();
        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
            Debug.LogError("GravityConfig is missing.");
    }

    void LateUpdate()
    {
        SimulateTrajectory();
    }

    void SimulateTrajectory()
    {
        Vector3[] positions = new Vector3[steps];
        Vector3 position = transform.position;
        Vector3 velocity = rb.velocity - planetRb.velocity; // відносна швидкість

        for (int i = 0; i < steps; i++)
        {
            positions[i] = position;

            Vector3 dir = (gravitySource.position - position).normalized;
            float distance = Vector3.Distance(position, gravitySource.position);
            float forceMag = config.gravitationalConstant * planetRb.mass / (distance * distance);
            Vector3 gravity = dir * forceMag;

            velocity += gravity * timeStep;
            position += velocity * timeStep;
        }

        lineRenderer.positionCount = steps;
        lineRenderer.SetPositions(positions);
    }
}
