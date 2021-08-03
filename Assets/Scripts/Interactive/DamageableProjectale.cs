using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableProjectale : DamageableObject
{
    [SerializeField] private float _timeToDeactivate = 1.5f;
    private float _curTimeToDeactivate;
    [SerializeField] private Rigidbody _rigidBody = null;

    private void Start()
    {
        _curTimeToDeactivate = Time.time + _timeToDeactivate;
    }

    private void FixedUpdate()
    {
        if (_rigidBody != null && _rigidBody.velocity.magnitude < 1f && _curTimeToDeactivate <= Time.time)
            Deactivate();
    }

    private void Deactivate()
    {
        Destroy(_rigidBody);
        Destroy(GetComponent<Collider>());
    }
}