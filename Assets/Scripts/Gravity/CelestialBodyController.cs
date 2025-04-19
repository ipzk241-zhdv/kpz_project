using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CelestialBodyController : MonoBehaviour
{
    [Header("Orbital Parameters")]
    [Tooltip("Central body (e.g., Sun or planet) around which this body orbits.")]
    public Transform centralBody;

    [Tooltip("Use initial position to define orbital radius.")]
    public bool useInitialPosition = true;

    [Tooltip("Orbital radius in km (1 unit = 1 km). If useInitialPosition is true, this is overridden.")]
    public float orbitalRadius = 0f;

    [Header("Rotation (Day/Night)")]
    [Tooltip("Rotation period in hours (full 360° turn).")]
    public float rotationPeriodHours = 24f;

    [Tooltip("If true, this body will always face the centralBody (tidal lock).")]
    public bool tidalLock = false;

    [Header("Orbit Visualization")]
    public bool visualizeOrbit = true;
    [Tooltip("Частота обновления орбиты (сек). 0 = не обновлять.")]
    public float orbitUpdateInterval = 1.0f;
    private float orbitUpdateTimer = 0f;
    [Tooltip("Number of segments for orbit line.")]
    [Range(8, 360)] public int orbitSegments = 180;
    [Tooltip("Width of orbit line.")]
    public float orbitLineWidth = 0.1f;
    public Color orbitColor = Color.white;

    // Internals
    private LineRenderer lr;
    private Vector3 orbitStartOffset;
    private double orbitalPeriod;      // seconds
    private double angularSpeed;       // rad/sec
    private double elapsedOrbitTime;
    private double spinSpeed;          // deg/sec

    // Physical constants
    private const double G = 6.67430e-20; // km^3 / t / s^2
    private double centralMass = 0;

    void Start()
    {
        if (centralBody == null)
        {
            Debug.LogError("Central body not assigned.");
            enabled = false;
            return;
        }

        // Try to get mass from GravityPlanet
        var gp = centralBody.GetComponent<GravityPlanet>();
        if (gp != null) centralMass = gp.GetMass();
        else Debug.LogWarning("Central body has no GravityPlanet; orbital calculations may be invalid.");

        // Determine orbital radius
        if (useInitialPosition)
        {
            orbitalRadius = Vector3.Distance(transform.position, centralBody.position);
        }

        // Compute orbital period T = 2π * sqrt(r^3 / (G * M))
        if (centralMass > 0 && orbitalRadius > 0)
        {
            orbitalPeriod = 2.0 * Math.PI * Math.Sqrt(
                Math.Pow(orbitalRadius, 3) / (G * centralMass)
            );
            angularSpeed = 2.0 * Math.PI / orbitalPeriod;
        }

        // Spin speed (deg/sec), if not tidally locked
        if (!tidalLock && rotationPeriodHours > 0)
        {
            spinSpeed = 360.0 / (rotationPeriodHours * 3600.0);
        }

        // Starting offset vector
        orbitStartOffset = transform.position - centralBody.position;

        // Setup LineRenderer
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = visualizeOrbit ? orbitSegments : 0;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = lr.endWidth = orbitLineWidth;
        lr.startColor = lr.endColor = orbitColor;

        if (visualizeOrbit) DrawOrbitLine();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Orbit movement
        if (angularSpeed > 0)
        {
            elapsedOrbitTime += dt;
            double angle = angularSpeed * elapsedOrbitTime;
            Quaternion rot = Quaternion.Euler(0f, (float)(angle * Mathf.Rad2Deg), 0f);
            Vector3 newPos = centralBody.position + rot * orbitStartOffset.normalized * orbitalRadius;
            transform.position = newPos;
        }

        // Self-rotation
        if (tidalLock)
        {
            // Always face central body
            Vector3 dir = centralBody.position - transform.position;
            if (dir.sqrMagnitude > 0)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        else if (spinSpeed != 0)
        {
            transform.Rotate(Vector3.up, (float)(spinSpeed * dt), Space.Self);
        }
    }

    public void LateUpdate()
    {
        if (visualizeOrbit && orbitUpdateInterval > 0f)
        {
            orbitUpdateTimer += Time.deltaTime;
            if (orbitUpdateTimer >= orbitUpdateInterval)
            {
                orbitUpdateTimer = 0f;
                DrawOrbitLine();
            }
        }
    }

    /// <summary>
    /// Returns day length in hours (for tidally locked returns orbital period in hours).
    /// </summary>
    public double GetDayLengthHours()
    {
        if (tidalLock && orbitalPeriod > 0)
            return orbitalPeriod / 3600.0;
        return rotationPeriodHours;
    }

    private void DrawOrbitLine()
    {
        Vector3 center = centralBody.position;
        Vector3 axis = Vector3.up; // вращение по оси Y (орбита в XZ-плоскости)
        Vector3 start = Vector3.forward; // пусть начальная точка будет "вперед"

        for (int i = 0; i < orbitSegments; i++)
        {
            float angle = 360f * i / orbitSegments;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            Vector3 point = center + rot * start * orbitalRadius;
            lr.SetPosition(i, point);
        }
    }

    public void UpdateOrbitVisualization()
    {
        if (visualizeOrbit && lr != null)
        {
            DrawOrbitLine();
        }
    }
}
