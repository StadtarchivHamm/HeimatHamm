using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollisionEvents : MonoBehaviour
{
    public UnityEvent<Collision> CollisionEnter = new UnityEvent<Collision>();
    public UnityEvent<Collision> CollisionExit = new UnityEvent<Collision>();
    public UnityEvent<Collider> TriggerEnter = new UnityEvent<Collider>();
    public UnityEvent<Collider> TriggerExit = new UnityEvent<Collider>();

    private Collider m_collider;

    private void Awake()
    {
        GetCollider();
    }

    public void ToggleCollider(bool isOn)
    {
        GetCollider();
        m_collider.enabled = isOn;
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit?.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionExit?.Invoke(collision);
    }

    private void GetCollider()
    {
        if (m_collider == null)
        {
            m_collider = GetComponent<Collider>();
        }
    }
}
