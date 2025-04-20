using System.Collections.Generic;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [Tooltip("÷ель, относительно которой центрируем мир (обычно это Main Camera или ракета).")]
    public Transform target;

    [Tooltip(" ак далеко (в юнитах) target может уйти от (0,0,0), прежде чем мы пересместим WorldRoot.")]
    public float threshold = 10000f;

    private List<Rigidbody> _allBodies;

    private void Start()
    {
        _allBodies = new List<Rigidbody>(FindObjectsOfType<Rigidbody>());
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (target.position.magnitude > threshold)
        {
            Vector3 offset = target.position;
            transform.position -= offset;

            foreach (var rb in _allBodies)
            {
                // можно фильтровать по тому, что объект Ч ребЄнок WorldRoot
                if (rb == null) continue;

                // переносим физический объект вместе с миром
                rb.isKinematic = true;
                // velocity оставл€ем без изменений, чтобы не вли€ть на траекторию
            }

            // ”ведомл€ем все тела, что произошел сдвиг
            var bodies = FindObjectsOfType<CelestialBodyController>();
            foreach (var body in bodies)
            {
                body.UpdateOrbitVisualization();
            }

            foreach (var rb in _allBodies)
            {
                // можно фильтровать по тому, что объект Ч ребЄнок WorldRoot
                if (rb == null) continue;

                // переносим физический объект вместе с миром
                rb.position -= offset;
                rb.isKinematic = false;
                // velocity оставл€ем без изменений, чтобы не вли€ть на траекторию
            }
        }
    }
}
