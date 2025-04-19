using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Налаштування візуалізації")]
    [Tooltip("Скільки точок малювати")]
    public int steps = 300;
    [Tooltip("Час між точками (в секундах)")]
    public float timeStep = 0.1f;
    [Tooltip("Ширина лінії")]
    public float lineWidth = 0.1f;
    [Tooltip("Колір траєкторії")]
    public Color lineColor = Color.green;

    [Header("Гравітаційне джерело")]
    [Tooltip("Гравітаційне тіло (планета/сонце)")]
    public GravityPlanet gravitySource;

    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private GravityConfig config;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        // Лінія
        lineRenderer.positionCount = steps;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        // Конфіг
        config = GravityConfig.Instance
               ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");
        if (config == null)
            Debug.LogError("GravityConfig is missing.");
    }

    void LateUpdate()
    {
        if (gravitySource == null || config == null)
            return;

        SimulateTrajectory();
    }

    void SimulateTrajectory()
    {
        Vector3[] positions = new Vector3[steps];

        Vector3 position = transform.position;
        Vector3 velocity = rb.velocity;

        // Масса источника (тонн)
        double M = gravitySource.GetMass();
        // G в km^3 / t / s^2
        double G = config.gravitationalConstant;

        for (int i = 0; i < steps; i++)
        {
            positions[i] = position;

            // Напрямок і відстань до центру гравітації
            Vector3 dir = gravitySource.GetGravityDirection(position);
            float dist = Vector3.Distance(position, gravitySource.transform.position);

            if (dist > 0f)
            {
                // Прискорення: a = G*M / r^2
                double accel = G * M / (dist * dist);
                Vector3 gravityAccel = dir * (float)accel;

                // Інтегруємо
                velocity += gravityAccel * timeStep;
            }

            position += velocity * timeStep;
        }

        lineRenderer.SetPositions(positions);
    }
}
