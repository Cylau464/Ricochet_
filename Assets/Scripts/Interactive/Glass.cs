using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Glass : MonoBehaviour
{
    [SerializeField] private GameObject _unbrokenVersion = null;
    [SerializeField] private GameObject _brokenVersion = null;
    [SerializeField] private float _explosionForce = 1f;
    [SerializeField] private float _explosionRadius = 1f;
    private Collision _collision;
    private bool _isBroken = false;
    private Rigidbody[] _projectilesRigidbody;

    private void Start()
    {
        _projectilesRigidbody = _brokenVersion.GetComponentsInChildren<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1f && _isBroken == false)
        {
            _collision = collision;
            Break(collision);
        }
    }

    private void Break(Collision collision)
    {
        _brokenVersion.SetActive(true);
        Vector3 pos = collision.contacts[0].point + (collision.transform.position - transform.position);
        pos.y = collision.contacts[0].point.y;
        float force = Mathf.Clamp(collision.relativeVelocity.magnitude, 1f, Mathf.Infinity) * _explosionForce;

        foreach (Rigidbody rb in _projectilesRigidbody)
        {
            //rb.transform.parent = null;
            rb.AddExplosionForce(force, pos, _explosionRadius);
        }

        _unbrokenVersion.SetActive(false);
        _isBroken = true;
    }

    public Rigidbody[] Break()
    {
        _brokenVersion.SetActive(true);
        _unbrokenVersion.SetActive(false);
        _isBroken = true;
        return _projectilesRigidbody;
    }
}