using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class GravityPlanet : MonoBehaviour
{
    private void Awake()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    public Vector3 GetGravityDirection(Vector3 position)
    {
        return (transform.position - position).normalized;
    }
}
