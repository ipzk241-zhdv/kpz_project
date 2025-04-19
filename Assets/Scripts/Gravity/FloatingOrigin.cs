using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [Tooltip("Цель, относительно которой центрируем мир (обычно это Main Camera или ракета).")]
    public Transform target;

    [Tooltip("Как далеко (в юнитах) target может уйти от (0,0,0), прежде чем мы пересместим WorldRoot.")]
    public float threshold = 10000f;

    void LateUpdate()
    {
        if (target == null) return;

        if (target.position.magnitude > threshold)
        {
            Vector3 offset = target.position;
            transform.position -= offset;

            // Уведомляем все тела, что произошел сдвиг
            var bodies = FindObjectsOfType<CelestialBodyController>();
            foreach (var body in bodies)
            {
                body.UpdateOrbitVisualization();
            }
        }
    }
}
