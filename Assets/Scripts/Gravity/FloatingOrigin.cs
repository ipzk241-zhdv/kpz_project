using System.Collections.Generic;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [Tooltip("����, ������������ ������� ���������� ��� (������ ��� Main Camera ��� ������).")]
    public Transform target;

    [Tooltip("��� ������ (� ������) target ����� ���� �� (0,0,0), ������ ��� �� ����������� WorldRoot.")]
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
                // ����� ����������� �� ����, ��� ������ � ������ WorldRoot
                if (rb == null) continue;

                // ��������� ���������� ������ ������ � �����
                rb.isKinematic = true;
                // velocity ��������� ��� ���������, ����� �� ������ �� ����������
            }

            // ���������� ��� ����, ��� ��������� �����
            var bodies = FindObjectsOfType<CelestialBodyController>();
            foreach (var body in bodies)
            {
                body.UpdateOrbitVisualization();
            }

            foreach (var rb in _allBodies)
            {
                // ����� ����������� �� ����, ��� ������ � ������ WorldRoot
                if (rb == null) continue;

                // ��������� ���������� ������ ������ � �����
                rb.position -= offset;
                rb.isKinematic = false;
                // velocity ��������� ��� ���������, ����� �� ������ �� ����������
            }
        }
    }
}
