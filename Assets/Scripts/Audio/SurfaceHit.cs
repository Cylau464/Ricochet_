using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceHit : MonoBehaviour
{
    [SerializeField] private AudioClip _contactSound = null;
    [SerializeField] private LayerMask _contactLayer = 0;
    [SerializeField] private GameObject _contactParticle = null;

    private void OnCollisionEnter(Collision collision)
    {
        if ((1 << collision.gameObject.layer & _contactLayer.value) != 0)
        {
            float impulse = 0f;
            Rigidbody parentRigibody = collision.gameObject.transform.parent?.GetComponent<Rigidbody>();

            if (parentRigibody != null)
                impulse = parentRigibody.mass * Vector3.Scale(parentRigibody.velocity, collision.contacts[0].normal).magnitude;
            else
                impulse = collision.relativeVelocity.magnitude;

            if (impulse < 1f) return;

            if (_contactSound != null)
                AudioManager.PlayClipAtPosition(_contactSound, transform.position, 1f, 10f, Random.Range(.8f, 1.1f));

            if (_contactParticle != null)
                Instantiate(_contactParticle, collision.contacts[0].point, Quaternion.identity);
        }
    }
}
