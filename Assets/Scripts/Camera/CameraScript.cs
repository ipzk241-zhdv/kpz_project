using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 20f;
    public float rotationSpeed = 5f;
    public float zoomSpeed = 10f;
    public float minDistance = 5f;
    public float maxDistance = 100f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Update()
    {
        HandleTargetSelection();

        if (target == null)
            return;

        HandleInput();
        UpdateCameraPosition();
    }

    void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SphereCollider sphere = hit.collider as SphereCollider;
                OrbitMover orbitMover = hit.collider.GetComponent<OrbitMover>();

                if (sphere != null && orbitMover != null)
                {
                    target = hit.collider.transform;
                }
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.forward;
        transform.position = target.position - direction * distance;
        transform.LookAt(target.position);
    }
}
