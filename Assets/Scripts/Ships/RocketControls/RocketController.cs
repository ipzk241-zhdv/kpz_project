using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OrbitMover))]
public class RocketController : MonoBehaviour
{
    [Header("Thrust Settings")]
    public float maxThrust = 50;
    public float thrustChangeSpeed = 0.1f;

    [Header("Rotation Settings")]
    public float pitchSpeed = 0.1f;
    public float yawSpeed = 0.1f;
    public float rollSpeed = 0.1f;

    [Range(0f, 1f)]
    public float currentThrust = 0f;

    public Rigidbody rb;
    public OrbitMover orbitMover;

    private IRocketState currentState;

    public IRocketState OnOrbit = new OnOrbitState();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        orbitMover = GetComponent<OrbitMover>();
        TransitionTo(OnOrbit);
    }

    void Update()
    {
        currentState.Update();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate();
    }

    void OnGUI()
    {
        currentState.OnGUI();
    }

    public void TransitionTo(IRocketState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }
}
