using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform defaultTarget; // Главная цель (ракета игрока)
    public float rotationSpeed = 5f;
    public float zoomSpeed = 10f;
    public float minDistance = 5f;
    public float maxDistance = 100f;
    public float focusMoveSpeed = 10f;
    public KeyCode focusKey = KeyCode.F;
    public KeyCode resetKey = KeyCode.BackQuote;

    private Transform currentTarget;
    private float distanceToTarget = 20f;
    private Vector2 rotationAngles = new Vector2(30f, 0f); // X (вверх/вниз), Y (влево/вправо)

    void Start()
    {
        currentTarget = defaultTarget;
        if (currentTarget != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void LateUpdate()
    {
        HandleZoom();
        HandleRotation();
        HandleFocus();
        UpdateCameraPosition();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distanceToTarget -= scroll * zoomSpeed;
        distanceToTarget = Mathf.Clamp(distanceToTarget, minDistance, maxDistance);
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1) && currentTarget != null)
        {
            rotationAngles.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
            rotationAngles.y += Input.GetAxis("Mouse X") * rotationSpeed;
            rotationAngles.x = Mathf.Clamp(rotationAngles.x, -89f, 89f); // ограничим вверх/вниз
        }
    }

    void HandleFocus()
    {
        if (Input.GetKeyDown(resetKey) && defaultTarget != null)
        {
            currentTarget = defaultTarget;
        }

        if (Input.GetKeyDown(focusKey))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                currentTarget = hit.transform;
            }
        }
    }

    void UpdateCameraPosition()
    {
        if (currentTarget == null) return;

        Quaternion rotation = Quaternion.Euler(rotationAngles.x, rotationAngles.y, 0);
        Vector3 direction = rotation * Vector3.back * distanceToTarget;
        Vector3 desiredPosition = currentTarget.position + direction;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * focusMoveSpeed);
        transform.LookAt(currentTarget.position);
    }
}
