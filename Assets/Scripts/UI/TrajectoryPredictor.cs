using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(Rigidbody))]
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("������������ ����������")]
    [Tooltip("������ ����� ��������")]
    public int steps = 300;
    [Tooltip("��� �� ������� (� ��������)")]
    public float timeStep = 0.1f;
    [Tooltip("������ ��")]
    public float lineWidth = 0.1f;
    [Tooltip("���� �������")]
    public Color lineColor = Color.green;

    [Header("����������� �������")]
    [Tooltip("����������� ��� (�������/�����)")]
    public GravityPlanet gravitySource;

    private LineRenderer lineRenderer;
    private Rigidbody rb;
    private GravityConfig config;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        // ˳��
        lineRenderer.positionCount = steps;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        // ������
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

        // ����� ��������� (����)
        double M = gravitySource.GetMass();
        // G � km^3 / t / s^2
        double G = config.gravitationalConstant;

        for (int i = 0; i < steps; i++)
        {
            positions[i] = position;

            // �������� � ������� �� ������ ���������
            Vector3 dir = gravitySource.GetGravityDirection(position);
            float dist = Vector3.Distance(position, gravitySource.transform.position);

            if (dist > 0f)
            {
                // �����������: a = G*M / r^2
                double accel = G * M / (dist * dist);
                Vector3 gravityAccel = dir * (float)accel;

                // ���������
                velocity += gravityAccel * timeStep;
            }

            position += velocity * timeStep;
        }

        lineRenderer.SetPositions(positions);
    }
}
