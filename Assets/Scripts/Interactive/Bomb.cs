using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : DamageableObject
{
    [Header("Delayed Explosion")]
    [SerializeField] private bool _delayedExplosion = false;
    [SerializeField] private AudioClip _activationClip = null;
    [SerializeField] private AudioSource _activationAudioSource = null;
    private float _explosionDelay;
    [Space]
    [SerializeField] private GameObject _unbrokenVersion = null;
    [SerializeField] private GameObject _brokenVersion = null;
    [SerializeField] private LayerMask _contactLayer = 0;
    [SerializeField] private Collider _triggerCollider = null;
    [SerializeField] private float _explosionForce = 5f;

    [SerializeField] private AudioClip _explosionClip = null;
    private Rigidbody[] _projectilesRigidbody;


    private void Start()
    {
        _projectilesRigidbody = _brokenVersion.GetComponentsInChildren<Rigidbody>();

        if(_activationClip != null)
            _explosionDelay = _activationClip.length;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude > 3f && ((1 << collision.gameObject.layer & _contactLayer.value) != 0))
        {
            if (_delayedExplosion == false)
            {
                CancelInvoke(nameof(Activate));
                Activate();
            }
            else
            {
                _delayedExplosion = false;
                _activationAudioSource.clip = _activationClip;
                _activationAudioSource.Play();
                Invoke(nameof(Activate), _explosionDelay);
            }
        }
    }

    public override void Activate()
    {
        if(_activationAudioSource != null)
            _activationAudioSource.Stop();

        _unbrokenVersion.SetActive(false);
        _brokenVersion.SetActive(true);
        _brokenVersion.transform.parent = null;
        AudioManager.PlayClipAtPosition(_explosionClip, transform.position, 1f, 10f);

        // Bomb projectales explosion
        foreach(Rigidbody rb in _projectilesRigidbody)
        {
            rb.AddExplosionForce(_explosionForce, transform.position, (_triggerCollider as SphereCollider).radius);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, (_triggerCollider as SphereCollider).radius, _contactLayer);

        foreach (Collider col in colliders)
        {
            Bomb bomb = col.GetComponentInParent<Bomb>();
            
            if (bomb != null)
            {
                bomb.Invoke(nameof(bomb.Activate), .1f);
            }

            Glass glass = col.GetComponentInParent<Glass>();

            if (glass != null)
            {
                Rigidbody[] rbs = glass.Break();

                foreach(Rigidbody glassRB in rbs)
                {
                    glassRB.AddExplosionForce(_explosionForce, transform.position, (_triggerCollider as SphereCollider).radius);
                }
            }

            if (col.TryGetComponent(out Rigidbody rb))
            {
                if(col.TryGetComponent(out Enemy enemy))
                {
                    CheckForLastEnemy(col.gameObject, _brokenVersion.transform); //Must be before GetHit
                    enemy.GetHit();
                    Rigidbody[] rbs = enemy.ragdollRigidbodies;

                    foreach(Rigidbody enemyRB in rbs)
                    {
                        enemyRB.AddExplosionForce(_explosionForce, transform.position, (_triggerCollider as SphereCollider).radius);
                    }
                }
                else
                {
                    rb.AddExplosionForce(_explosionForce, transform.position, (_triggerCollider as SphereCollider).radius);
                }
            }
        }
    }
}