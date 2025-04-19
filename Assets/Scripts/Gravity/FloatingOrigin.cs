using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [Tooltip("����, ������������ ������� ���������� ��� (������ ��� Main Camera ��� ������).")]
    public Transform target;

    [Tooltip("��� ������ (� ������) target ����� ���� �� (0,0,0), ������ ��� �� ����������� WorldRoot.")]
    public float threshold = 10000f;

    void LateUpdate()
    {
        if (target == null) return;

        if (target.position.magnitude > threshold)
        {
            Vector3 offset = target.position;
            transform.position -= offset;

            // ���������� ��� ����, ��� ��������� �����
            var bodies = FindObjectsOfType<CelestialBodyController>();
            foreach (var body in bodies)
            {
                body.UpdateOrbitVisualization();
            }
        }
    }
}
