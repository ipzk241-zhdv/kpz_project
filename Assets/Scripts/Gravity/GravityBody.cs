using UnityEngine;

public class GravityBody : MonoBehaviour
{
    public GravityPlanet currentGravitySource;

    private Rigidbody rb;
    private GravityConfig config;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        config = GravityConfig.Instance ?? Resources.Load<GravityConfig>("Scripts/Configs/GravityConfig");

        if (config == null)
            Debug.LogError("GravityConfig is missing.");

        config = GravityConfig.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        GravityPlanet gravityPlanet = other.GetComponent<GravityPlanet>();
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
        float m2 = currentGravitySource.GetComponent<Rigidbody>().mass;

        float forceMagnitude = config.gravitationalConstant * (m1 * m2) / (distance * distance);
        Vector3 force = direction * forceMagnitude;

        rb.AddForce(force);
        Debug.DrawRay(transform.position, force.normalized * 100f, Color.red);
    }
}

