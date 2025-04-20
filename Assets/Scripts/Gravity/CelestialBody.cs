using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    [Header("Orbital Characteristics")]
    public float semiMajorAxis = 13599840256f;
    public float eccentricity = 0f;
    public float inclination = 0f;
    public float argumentOfPeriapsis = 0f;
    public float longitudeOfAscendingNode = 0f;
    public float meanAnomalyAtEpoch = 3.14f;
    public float orbitalPeriod = 9203545f;
    public float orbitalVelocity = 9285f;
    public float sphereOfInfluence = 84159286f;

    [Header("Physical Characteristics")]
    public float equatorialRadius = 600000f;
    public float mass = 5.2915158e22f;
    public float surfaceGravity = 9.81f;
    public float escapeVelocity = 3431.03f;
    public float rotationPeriod = 21549.425f;
    public float atmosphereHeight = 70000f;
    public float atmosphericPressure = 101.325f;
    public bool hasAtmosphere = true;
    public bool hasOxygen = true;

    [HideInInspector] public Rigidbody rb;

    public Vector3 GetGravityDirection(Vector3 position)
    {
        return (transform.position - position).normalized;
    }

    public float GetGravityForce(Vector3 position, float otherMass, float G)
    {
        float distance = Vector3.Distance(position, transform.position);
        return G * mass * otherMass / (distance * distance);
    }
}
