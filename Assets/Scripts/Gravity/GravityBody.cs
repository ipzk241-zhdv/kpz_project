using UnityEngine;

public class GravityBody : MonoBehaviour
{
    public CelestialBody currentGravitySource;

    private Rigidbody rb;
    private GravityConfig config;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
        {
            Debug.LogError("GravityConfig is missing.");
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var gravityPlanet = other.GetComponent<CelestialBody>();
        if (gravityPlanet != null)
        {
            currentGravitySource = gravityPlanet;
        }
    }

    void FixedUpdate()
    {
        if (currentGravitySource == null || config == null) return;

        Vector3 direction = currentGravitySource.GetGravityDirection(transform.position);
        float distance = Vector3.Distance(transform.position, currentGravitySource.transform.position);

        float m1 = rb.mass;
        float m2 = currentGravitySource.mass; // беремо масу напряму

        float forceMagnitude = config.gravitationalConstant * (m1 * m2) / (distance * distance);
        Vector3 force = direction * forceMagnitude;

        rb.AddForce(force);
    }
}
