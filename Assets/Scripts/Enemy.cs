using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _health = 1;
    private int _curHealth;

    [Header("Layers")]
    [SerializeField] private LayerMask _hitLayers = 0;
    [SerializeField] private LayerMask _notCollideLayer = 0;
    [SerializeField] private LayerMask _deadEnemyLayer = 0;
    [Space]
    [SerializeField] private float _notCollideDelay = .3f;
    [SerializeField] private Collider _collider = null;
    [SerializeField] private CharacterAnimation _animation = null;
    [SerializeField] private Rigidbody _rigidBody = null;
    public Transform zoomCameraTarget;

    [Header("Movable Properties")]
    [SerializeField] private bool _movableEnemy = false;
    [SerializeField] private Transform[] _moveTargets = null;
    [SerializeField] private float _rotateSpeed = 10f;

    [HideInInspector] public Rigidbody[] ragdollRigidbodies;

    [Header("Hit Properties")]
    [SerializeField] private AudioClip _hitSound = null;
    [SerializeField] private GameObject _hitParticle = null;
    [SerializeField] private float _hitParticleSpawnHeight = 1f;

    private bool _isDead;

    void Start()
    {
        _curHealth = _health;
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();

        _animation.IsMove = _movableEnemy;

        if(_movableEnemy == true)
            StartCoroutine(Patroling());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_isDead == false && ((1 << collision.gameObject.layer & _hitLayers.value) != 0))
        {
            GetHit();
        }
    }

    private void TurnOffCollide()
    {
        //gameObject.layer = (int)Mathf.Log(_notCollideLayer, 2);
        ChangeLayersRecursively(transform, (int)Mathf.Log(_notCollideLayer, 2));
    }

    private void ChangeLayersRecursively(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;

        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, layer);
        }
    }

    public void GetHit()
    {
        if (_isDead == true) return;

        _curHealth--;

        if (_curHealth <= 0)
        {
            CheckForLastEnemy();
            SetLayer(transform, (int)Mathf.Log(_deadEnemyLayer, 2));
            _collider.enabled = false;
            _rigidBody.isKinematic = true;
            _animation.ActivateRagdoll();
            _isDead = true;
            GameManager.EnemyIsDead();
            Invoke(nameof(TurnOffCollide), _notCollideDelay);

            if (_hitSound != null)
                AudioManager.PlayClipAtPosition(_hitSound, transform.position, 1f, 10f, Random.Range(.8f, 1.1f));

            if (_hitParticle != null)
            {
                Vector3 spawnPos = transform.position;
                spawnPos.y += _hitParticleSpawnHeight;
                Instantiate(_hitParticle, spawnPos, Quaternion.identity);
            }
        }
    }

    private void SetLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        foreach (Transform child in root)
            SetLayer(child, layer);
    }

    private void CheckForLastEnemy()
    {
        if (GameManager.current.EnemyCount == 1 && Zoom.isSlowMoActivated == false)
        {
            Zoom.Activate(zoomCameraTarget);
        }
    }

    private IEnumerator Patroling()
    {
        if (_moveTargets.Length == 0) yield break;

        int targetIndex = 1;
        transform.rotation = Quaternion.LookRotation((_moveTargets[targetIndex].position - transform.position).normalized);
        bool startMoving = true;

        while (true)
        {
            for(; targetIndex < _moveTargets.Length; targetIndex++)
            {
                if (startMoving == false)
                {
                    Vector3 dir = _moveTargets[targetIndex].position - transform.position;
                    float angle = Vector3.Angle(dir, transform.forward);
                    Debug.Log(angle + " FORWARD " + transform.forward + " NAME " + transform.name);
                    angle = Mathf.Abs(Mathf.Round(angle / 10f) * 10f);
                    
                    if (/*angle == 0f || */angle >= 150f && angle <= 210f)
                    {
                        int random = Random.Range(0, 100);

                        if (random > 50)
                            _animation.LeftTurn();
                        else
                            _animation.RightTurn();
                    }
                }

                while ((transform.position - _moveTargets[targetIndex].position).sqrMagnitude > .1f)
                {
                    if (_animation.isTurning == false)
                    {
                        Vector3 dir = _moveTargets[targetIndex].position - transform.position;
                        dir.y = 0f;
                        Quaternion rotation = Quaternion.LookRotation(dir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _rotateSpeed * Time.fixedDeltaTime);
                    }
                    yield return new WaitForFixedUpdate();
                }

                startMoving = false;
            }

            targetIndex = 0;
        }
    }
}
