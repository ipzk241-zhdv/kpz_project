using UnityEngine;

public class OnOrbitState : IRocketState
{
    private RocketController _rocket;

    public void Enter(RocketController rocket)
    {
        _rocket = rocket;
    }

    public void Exit(RocketController rocket) { }

    public void Update()
    {
        HandleThrustInput();
    }

    public void FixedUpdate()
    {
        ApplyThrust();
        ApplyRotation();
    }

    public void OnGUI()
    {
        int thrustPercent = Mathf.RoundToInt(_rocket.currentThrust * 100f);
        GUI.Label(new Rect(10, 10, 200, 20), $"Thrust: {thrustPercent}%");
    }

    void HandleThrustInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            _rocket.currentThrust += _rocket.thrustChangeSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftControl))
            _rocket.currentThrust -= _rocket.thrustChangeSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Z))
            _rocket.currentThrust = 1f;

        if (Input.GetKeyDown(KeyCode.X))
            _rocket.currentThrust = 0f;

        _rocket.currentThrust = Mathf.Clamp01(_rocket.currentThrust);
    }

    void ApplyThrust()
    {
        if (_rocket.orbitMover == null || !_rocket.orbitMover.orbitData.IsValidOrbit)
            return;

        float thrustForce = _rocket.currentThrust * _rocket.maxThrust;
        if (thrustForce <= 0f)
            return;

        Vector3 thrustDirection = _rocket.transform.forward;

        Vector3d deltaVelocity = new Vector3d(thrustDirection.x, thrustDirection.y, thrustDirection.z) * thrustForce * Time.fixedDeltaTime;
        _rocket.orbitMover.orbitData.velocityRelativeToAttractor += deltaVelocity;

        _rocket.orbitMover.orbitData.CalculateOrbitStateFromOrbitalVectors();
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

        Vector3 torque = _rocket.transform.right * pitch * _rocket.pitchSpeed
                       + _rocket.transform.up * yaw * _rocket.yawSpeed
                       + _rocket.transform.forward * roll * _rocket.rollSpeed;

        _rocket.rb.AddTorque(torque);
    }
}

