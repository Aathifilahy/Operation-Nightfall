using System;
using UnityEngine;

/// <summary>
/// Attach to any throwable object that should notify the IS module when moved.
/// Works with PlayerObjectInteraction.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DynamicBlockerEvent : MonoBehaviour
{
    // Event triggered whenever the object is dropped or thrown
    public static event Action<Vector3> OnBlockerMoved;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Call this method when the object is dropped
    public void TriggerDrop()
    {
        OnBlockerMoved?.Invoke(transform.position);
        Debug.Log("DynamicBlocker dropped at: " + transform.position);
    }

    // Call this method when the object is thrown
    public void TriggerThrow()
    {
        OnBlockerMoved?.Invoke(transform.position);
        Debug.Log("DynamicBlocker thrown to: " + transform.position);
    }
}