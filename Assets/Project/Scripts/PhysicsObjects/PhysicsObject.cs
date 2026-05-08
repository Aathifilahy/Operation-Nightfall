using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
}