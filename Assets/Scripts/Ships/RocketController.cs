using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketController : MonoBehaviour
{
    public float thrustPower = 1f;       // Сила тяги
    public float rotationSpeed = 0.1f;    // Швидкість обертання
    private float currentThrust = 0f;     // Поточна тяга

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Змінюємо поточну тягу
        if (Input.GetKey(KeyCode.LeftShift))
            currentThrust = thrustPower;
        else if (Input.GetKey(KeyCode.LeftControl))
            currentThrust = -thrustPower;
        else
            currentThrust = 0f;
    }

    void FixedUpdate()
    {
        // Додаємо силу вздовж локальної осі Y (вгору)
        rb.AddForce(transform.up * currentThrust);

        float rotation = 0f;

        if (Input.GetKey(KeyCode.A))
            rotation = rotationSpeed;
        else if (Input.GetKey(KeyCode.D))
            rotation = -rotationSpeed;

        rb.AddTorque(Vector3.forward * rotation);
    }
}
