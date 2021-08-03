using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Shield : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private float _lifeTime = 3f;
    private float _curLifeTime;
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _flyHeight = .75f;
    [SerializeField] private LayerMask _obstaclesLayers = 0;
    private Vector3 _velocity, _lastVelocity;
    private bool _lastThrow;
    private bool _collideInThisFrame;

    [Header("Check Last Target")]
    [SerializeField] private LayerMask _enemyLayer = 0;
    [SerializeField] private float _checkDistance = 3f;

    [Header("References")]
    [SerializeField] private Rigidbody _rigidBody = null;
    [SerializeField] private Collider _collider = null;
    public CapsuleCollider capsuleCollider = null;

    //[Header("Destroy Properties")]
    //[SerializeField] private GameObject _destroyedVersionPrefab = null;
    //[SerializeField] private float _explosionForce = 5f;
    //[SerializeField] private float _explosionRadius = 5f;

    [Header("Skin Properties")]
    [SerializeField] private Renderer _meshRenderer = null;

    private UnityAction<Vector3, Vector3, bool> _throwAction;

    private GameObject _lastStuckedCollider;
    private int _doubleStuckCount;

    private void OnEnable()
    {
        _throwAction = Throw;
        PlayerController.throwEvent.AddListener(_throwAction);
        GameManager.levelCompletedEvent.AddListener(SelfDestroy);

        if (ItemStorage.selectedShieldSkin.model != null)
        {
            Destroy(_meshRenderer.gameObject);
            Vector3 spawnPos = transform.position;
            spawnPos.y += ItemStorage.selectedShieldSkin.spawnHeight;
            Instantiate(ItemStorage.selectedShieldSkin.model, spawnPos, Quaternion.identity, transform);
        }
        else
        {
            _meshRenderer.material = ItemStorage.selectedShieldSkin.material;
        }
    }

    private void Update()
    {
        if (_rigidBody.velocity.magnitude > 0f && _curLifeTime > 0f)
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        _lastVelocity = _rigidBody.velocity;
        _collideInThisFrame = false;

        if(_curLifeTime <= 0f)
        {
            _rigidBody.velocity = Vector3.zero;
        }
        else if (_rigidBody.velocity.magnitude > _speed || (_rigidBody.velocity.magnitude > 1f && _rigidBody.velocity.magnitude < _speed))
        {
            _rigidBody.velocity = _rigidBody.velocity.normalized * _speed;
        }
        // If shield stuck in wall
        else if(_rigidBody.velocity.magnitude < 1f && _curLifeTime > 0f)
        {
            RaycastHit hit;
            Transform closestCollider = null;
            float distanceToCollider = Mathf.Infinity;
            Collider[] colliders = Physics.OverlapSphere(transform.position, capsuleCollider.radius + .1f, _obstaclesLayers);
            bool isDoubleStuck = false;

            foreach(Collider collider in colliders)
            {
                if (collider.gameObject == _lastStuckedCollider)
                {
                    isDoubleStuck = true;
                    _doubleStuckCount++;
                    continue;
                }

                float distance = Vector3.Distance(collider.transform.position, transform.position);

                if (distance < distanceToCollider)
                {
                    closestCollider = collider.transform;
                    distanceToCollider = distance;
                }
            }

            _lastStuckedCollider = closestCollider.gameObject;
            Physics.Raycast(transform.position, (closestCollider.position - transform.position).normalized, out hit, Mathf.Infinity, _obstaclesLayers);
            _rigidBody.velocity = Vector3.Reflect((closestCollider.position - transform.position).normalized, hit.normal).normalized * _speed;
            Debug.LogWarning("WALL STUCK " + hit.normal + " NAME " + closestCollider.name + " DOUBLE STUCK " + isDoubleStuck.ToString());

            if (_doubleStuckCount > 1)
                AppMetricaEvents.Instance.ShieldStuck(_doubleStuckCount);
        }

        CheckLastTarget();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ricochet
        if ((1 << collision.gameObject.layer & _obstaclesLayers.value) != 0 && _collideInThisFrame == false)
        {
            Vector3 normal = collision.contacts[0].normal;

            float xSignCol = Mathf.Round(normal.x * 10f) / 10f;
            xSignCol = xSignCol == 0f ? 0f : Mathf.Sign(xSignCol);
            float zSignCol = Mathf.Round(normal.z * 10f) / 10f;
            zSignCol = zSignCol == 0f ? 0f : Mathf.Sign(zSignCol);
            float xSignVel = Mathf.Round(_lastVelocity.normalized.x * 10f) / 10f;
            xSignVel = xSignVel == 0f ? 0f : Mathf.Sign(xSignVel);
            float zSignVel = Mathf.Round(_lastVelocity.normalized.z * 10f) / 10f;
            zSignVel = zSignVel == 0f ? 0f : Mathf.Sign(zSignVel);
            //Debug.Log(xSignCol + " X Z " + zSignCol + " X " + xSignVel + " Z " + zSignVel + " CONTACT " + collision.contacts[0].normal + " VEL " + _lastVelocity.normalized);

            Vector3 dir = Vector3.Reflect(_lastVelocity.normalized, normal);
            Debug.DrawRay(transform.position, dir * 2f, Color.green, .1f);
            Debug.DrawRay(transform.position, _lastVelocity.normalized * 2f, Color.red, .1f);
            Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.cyan, .1f);

            if (xSignCol != 0 && xSignVel != 0 && xSignCol == xSignVel)
            {
                _lastVelocity.x *= -1f;
                Debug.Log("X REFLECTION INVERT");
                //Debug.Break();
            }

            if (zSignCol != 0 && zSignVel != 0 && zSignCol == zSignVel)
            {
                _lastVelocity.z *= -1f;
                Debug.Log("Z REFLECTION INVERT");
                //Debug.Break();
            }

            dir = Vector3.Reflect(_lastVelocity.normalized, normal);
            Debug.DrawRay(transform.position, dir * 2f, Color.blue, .1f);

            _velocity = dir * _speed;
            _rigidBody.velocity = _velocity;

            //Debug.Log("COLLIDE " + _velocity);
            if (_curLifeTime > 0f && _curLifeTime <= Time.time)
                SelfDestroy();

            _collideInThisFrame = true;
        }
    }

    private void Throw(Vector3 direction, Vector3 spawnPos, bool lastThrow)
    {
        transform.rotation = Quaternion.identity;
        transform.parent = null;
        spawnPos.y = _flyHeight;
        transform.position = spawnPos;//new Vector3(transform.position.x, _flyHeight, transform.position.z);
        _rigidBody.velocity = direction * _speed;
        _collider.isTrigger = false;
        _lastThrow = lastThrow;
        //Invoke(nameof(SelfDestroy), _lifeTime);
        _curLifeTime = Time.time + _lifeTime;
        PlayerController.throwEvent.RemoveListener(_throwAction);
    }

    public void SelfDestroy()
    {
        if (_lastThrow && Zoom.isSlowMoActivated == false)
            GameManager.GameOver();

        //GameObject destroyedVersion = Instantiate(_destroyedVersionPrefab, transform.position, _destroyedVersionPrefab.transform.rotation);
        //Rigidbody[] rbs = destroyedVersion.GetComponentsInChildren<Rigidbody>();

        //foreach (Rigidbody rb in rbs)
        //{
        //    rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
        //}

        Debug.Log("SHIELD DESREOT");
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        if (_curLifeTime <= Time.time)
            SelfDestroy();
        else
            Invoke(nameof(SelfDestroy), _curLifeTime - Time.time);
    }

    private void CheckLastTarget()
    {
        if (GameManager.current.EnemyCount == 1 && Zoom.isSlowMoActivated == false)
        {
            RaycastHit hit1 = default, hit2 = default, hit3 = default;
            Vector3 middlePos = transform.position - _rigidBody.velocity.normalized * capsuleCollider.radius;
            Vector3 rightRayPos = middlePos + Vector3.Cross(_rigidBody.velocity.normalized, Vector3.up) * capsuleCollider.radius;
            Vector3 leftRayPos = middlePos + Vector3.Cross(_rigidBody.velocity.normalized, Vector3.down) * capsuleCollider.radius;
            Ray middleRay = new Ray(middlePos, _rigidBody.velocity.normalized);
            Ray rightRay = new Ray(rightRayPos, _rigidBody.velocity.normalized);
            Ray leftRay = new Ray(leftRayPos, _rigidBody.velocity.normalized);
            List<RaycastHit> enemyHit = new List<RaycastHit>();
            List<RaycastHit> wallHit = new List<RaycastHit>();
            bool activateSlowMo = false;
            Transform enemyCameraTarget = null;
            LayerMask targetLayers = _enemyLayer | _obstaclesLayers;

            Color middleRayColor = Color.green, leftRayColor = Color.green, rightRayColor = Color.green;

            if (Physics.Raycast(middleRay, out hit1, _checkDistance, targetLayers))
            {
                if ((1 << hit1.collider.gameObject.layer & _enemyLayer.value) != 0)
                {
                    enemyHit.Add(hit1);
                    middleRayColor = Color.red;
                }
                else if ((1 << hit1.collider.gameObject.layer & _obstaclesLayers.value) != 0)
                {
                    wallHit.Add(hit1);
                    middleRayColor = Color.cyan;
                }
            }

            if (Physics.Raycast(rightRay, out hit2, _checkDistance, targetLayers))
            {
                if ((1 << hit2.collider.gameObject.layer & _enemyLayer.value) != 0)
                {
                    enemyHit.Add(hit2);
                    rightRayColor = Color.red;
                }
                else if ((1 << hit2.collider.gameObject.layer & _obstaclesLayers.value) != 0)
                {
                    wallHit.Add(hit2);
                    rightRayColor = Color.cyan;
                }
            }

            if (Physics.Raycast(leftRay, out hit3, _checkDistance, targetLayers))
            {
                if ((1 << hit3.collider.gameObject.layer & _enemyLayer.value) != 0)
                {
                    enemyHit.Add(hit3);
                    leftRayColor = Color.red;
                }
                else if ((1 << hit3.collider.gameObject.layer & _obstaclesLayers.value) != 0)
                {
                    wallHit.Add(hit3);
                    leftRayColor = Color.cyan;
                }
            }

            Enemy enemy = null;

            foreach (RaycastHit hit in enemyHit)
            {
                activateSlowMo = true;
                if (hit.collider.TryGetComponent(out enemy))
                    enemyCameraTarget = enemy.zoomCameraTarget;
                else
                    enemyCameraTarget = hit.collider.GetComponentInParent<Enemy>()?.zoomCameraTarget;

                foreach (RaycastHit hitW in wallHit)
                {
                    if (hitW.distance < hit.distance)
                    {
                        activateSlowMo = false;
                        return;
                    }
                }
            }

            if (activateSlowMo == true)
            {
                // Calculate the point where the shield will be when it reaches the enemy
                float distanceToTarget = (enemy.transform.position - transform.position).magnitude - (_collider as CapsuleCollider).radius;
                float time = distanceToTarget / _rigidBody.velocity.magnitude;
                Vector3 destinationPoint = transform.position + distanceToTarget * _rigidBody.velocity.normalized;

                // Calculate the point where the enemy will be when the shield reaches the current position of the enemy
                Animator animator = enemy.GetComponentInChildren<Animator>();
                Vector3 enemyMoveDistance = animator.velocity * time;
                Vector3 enemyDestinationPoint = enemy.transform.position + enemyMoveDistance;
                enemyDestinationPoint.y += destinationPoint.y;

                float r1 = (_collider as CapsuleCollider).radius;
                float r2 = enemy.GetComponent<CapsuleCollider>().radius;

                // Calculate the distance between two capsule colliders and check if they intersect
                if (Vector3.Distance(destinationPoint, enemyDestinationPoint) <= r1 + r2)
                {
                    Zoom.Activate(enemyCameraTarget);
                    Debug.Log("INTERSECT " + Vector3.Distance(destinationPoint, enemyDestinationPoint) + " DIST RAD 1:" + r1 + " RAD 2: " + r2);
                }
                else
                {
                    activateSlowMo = false;
                    Debug.Log("NOT INTERSECT " + Vector3.Distance(destinationPoint, enemyDestinationPoint) + " DIST RAD 1:" + r1 + " RAD 2: " + r2);
                }
            }

            Debug.DrawRay(middleRay.origin, middleRay.direction * _checkDistance, middleRayColor);
            Debug.DrawRay(rightRay.origin, rightRay.direction * _checkDistance, rightRayColor);
            Debug.DrawRay(leftRay.origin, leftRay.direction * _checkDistance, leftRayColor);
        }
    }
}
