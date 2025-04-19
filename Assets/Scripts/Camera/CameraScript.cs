using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position;

            transform.position = new Vector3(
                targetPosition.x,
                targetPosition.y,
                transform.position.z // залишаємо поточну Z позицію
            );
        }
    }
}
