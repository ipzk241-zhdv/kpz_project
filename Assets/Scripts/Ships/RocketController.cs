using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketController : MonoBehaviour
{
    [Header("Thrust Settings")]
    public float maxThrust = 1;               // Максимальна сила двигуна
    public float thrustChangeSpeed = 0.1f;       // Швидкість зміни дроселя (в секунду)

    [Header("Rotation Settings")]
    public float pitchSpeed = 0.1f;
    public float yawSpeed = 0.1f;
    public float rollSpeed = 0.1f;

    [Range(0f, 1f)]
    private float currentThrust = 0f;            // Дросель: 0.0 - 1.0

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleThrustInput();
    }

    void FixedUpdate()
    {
        ApplyThrust();
        ApplyRotation();
    }

    void OnGUI()
    {
        int thrustPercent = Mathf.RoundToInt(currentThrust * 100f);
        GUI.Label(new Rect(10, 10, 200, 20), $"Thrust: {thrustPercent}%");
    }

    void HandleThrustInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            currentThrust += thrustChangeSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftControl))
            currentThrust -= thrustChangeSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Z))
            currentThrust = 1f;

        if (Input.GetKeyDown(KeyCode.X))
            currentThrust = 0f;

        currentThrust = Mathf.Clamp01(currentThrust);
    }

    void ApplyThrust()
    {
        float thrustForce = currentThrust * maxThrust;
        rb.AddForce(transform.forward * thrustForce);
    }

    void ApplyRotation()
    {
        float pitch = 0f;
        float yaw = 0f;
        float roll = 0f;

        if (Input.GetKey(KeyCode.S)) pitch = 1f;
        if (Input.GetKey(KeyCode.W)) pitch = -1f;
        if (Input.GetKey(KeyCode.E)) roll = -1f;
        if (Input.GetKey(KeyCode.Q)) roll = 1f;
        if (Input.GetKey(KeyCode.D)) yaw = 1f;
        if (Input.GetKey(KeyCode.A)) yaw = -1f;

        Vector3 torque = transform.right * pitch * pitchSpeed
                       + transform.up * yaw * yawSpeed
                       + transform.forward * roll * rollSpeed;

        rb.AddTorque(torque);
    }
}
