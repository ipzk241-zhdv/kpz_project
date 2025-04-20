using System.Collections.Generic;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [Tooltip("Ціль, відносно якої центрируємо світ — зазвичай ракета або Main Camera")]
    public Transform target;

    [Tooltip("Поріг, при якому відбувається зсув світу")]
    public float threshold = 50000f;

    private List<Rigidbody> shiftableRigidbodies = new List<Rigidbody>();

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("FloatingOrigin: Target не встановлено.");
            return;
        }

        // Автоматичне заповнення списку всіх Rigidbody, крім цільового
        Rigidbody[] allBodies = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody rb in allBodies)
        {
            if (rb.transform != target)
                shiftableRigidbodies.Add(rb);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Якщо target занадто далеко від (0, 0, 0), зсуваємо все
        if (target.position.magnitude > threshold)
        {
            Vector3 offset = target.position;

            // Переміщуємо всі об'єкти
            foreach (var rb in shiftableRigidbodies)
            {
                if (rb == null) continue;

                bool wasKinematic = rb.isKinematic;
                rb.isKinematic = true; // щоб не взаємодіяло з фізикою під час зсуву
                rb.position -= offset;
                rb.isKinematic = wasKinematic;
            }

            // Переміщуємо WorldRoot, якщо він відмінний від target
            if (transform != target)
                transform.position -= offset;

            var bodies = FindObjectsOfType<OrbitRenderer>();
            foreach (var body in bodies)
            {
                body.DrawOrbit();
            }

            Debug.Log($"FloatingOrigin: зсув світу на {offset}");
        }
    }
}
