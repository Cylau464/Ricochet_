using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Header("Shield Properties")]
    [SerializeField] private GameObject _shieldPrefab = null;
    private GameObject _shield;
    [SerializeField] private Transform _shieldPivotPoint = null;
    private int _curNumberOfThrows;
    private Vector3 _targetPoint;
    private Vector3 throwDirection = Vector3.zero;
    private bool _inputDisabled;

    [Header("Aim Properties")]
    [SerializeField] private Transform _modelTransform = null;
    [SerializeField] private CharacterAnimation _animation = null;
    [SerializeField] private LineRenderer _lineRenderer = null;
    private Material _lineMaterial;
    private float _aimRayOffset;
    [SerializeField] private float _aimYOffset = .2f;
    [SerializeField] private LayerMask _aimLayers = -1;
    private bool _aimActivated;

    [Header("Skin Properties")]
    [SerializeField] private Renderer _meshRenderer = null;

    [SerializeField] private AudioClip[] _throwClips = new AudioClip[0];

    public static UnityEvent<Vector3, Vector3, bool> throwEvent;

    private void OnEnable()
    {
        throwEvent = new UnityEvent<Vector3, Vector3, bool>();
        _inputDisabled = false;
    }

    private void OnDisable()
    {
        throwEvent = null;
    }

    private void Start()
    {
        _lineMaterial = _lineRenderer.material;
        _aimRayOffset = _shieldPrefab.GetComponent<Shield>().capsuleCollider.radius;
        _aimActivated = false;
        SpawnShield();
        GameManager.levelCompletedEvent.AddListener(DisableInput);
        GameManager.levelCompletedEvent.AddListener(VictoryAnimation);
        GameManager.gameOverEvent.AddListener(DisableInput);
        GameManager.gameOverEvent.AddListener(DefeatAnimation);
        _animation.spawnShieldEvent.AddListener(SpawnShield);
        _animation.throwEvent.AddListener(Throw);

        _meshRenderer.material = ItemStorage.selectedCharacterSkin.material;
    }

    void Update()
    {
        if (_inputDisabled == false)
        {
            if (Input.touches.Length > 0 && ScoreHandler.curNumberOfThrows > 0)
            {
                Touch t = Input.GetTouch(0);

                switch(t.phase)
                {
                    case TouchPhase.Began:
                        if (EventSystem.current.IsPointerOverGameObject(t.fingerId) == false)
                        {
                            _aimActivated = true;
                            //_animation.IsThrow = false;
                            _animation.IsAim = true;

                            if (_shield == null)
                                SpawnShield();
                        }
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_aimActivated == true)
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            Aim(ray);
                        }
                        break;
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        if (_aimActivated == true && _targetPoint != Vector3.zero)
                        {
                            _aimActivated = false;
                            _animation.IsAim = false;
                            _animation.IsThrow = true;
                        }
                        break;
                }
            }
            else
            {
                if(Input.GetMouseButtonDown(0) && ScoreHandler.curNumberOfThrows > 0 && EventSystem.current.IsPointerOverGameObject() == false)
                {
                    _aimActivated = true;
                    //_animation.IsThrow = false;
                    _animation.IsAim = true;

                    if (_shield == null)
                        SpawnShield();
                }
                if (Input.GetMouseButton(0) && _aimActivated == true)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Aim(ray);
                }
                else if (Input.GetMouseButtonUp(0) && _targetPoint != Vector3.zero && _aimActivated == true)
                {
                    _aimActivated = false;
                    _animation.IsAim = false;
                    _animation.IsThrow = true;
                }
            }
        }

        if(_animation.IsThrow == false && _animation.IsAim == false/*_aimActivated == false*/ && _modelTransform.rotation.eulerAngles != Vector3.zero)
        {
            _modelTransform.rotation = Quaternion.Lerp(_modelTransform.rotation, Quaternion.identity, Time.deltaTime * 4f);
        }
    }

    private void Aim(Ray ray)
    {
        _lineRenderer.enabled = true;
        RaycastHit hit, hit2, hit3, hit4;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            Vector3 position = transform.position;
            position.y += _aimYOffset;
            Vector3 leftPosition = position + Vector3.Cross(throwDirection.normalized, Vector3.up) * _aimRayOffset;
            Vector3 rightPosition = position - Vector3.Cross(throwDirection.normalized, Vector3.up) * _aimRayOffset;
            _targetPoint = hit.point;
            _targetPoint.y = position.y;
            throwDirection = _targetPoint - position;

            // Lock aim
            //if (throwDirection.z < 0f)
            //    throwDirection.z = 0f;

            Vector3[] linePoints;
            List<Vector3> endPoints = new List<Vector3>();

            if (Physics.Raycast(position, throwDirection.normalized, out hit2, 100f, _aimLayers))
                endPoints.Add(hit2.point); 

            if (Physics.Raycast(leftPosition, throwDirection.normalized, out hit3, 100f, _aimLayers))
                endPoints.Add(hit3.point);

            if (Physics.Raycast(rightPosition, throwDirection.normalized, out hit4, 100f, _aimLayers))
                endPoints.Add(hit4.point);
            
            float distance = Mathf.Infinity;
            Vector3 endPoint;

            if (endPoints.Count > 0)
            {
                foreach (Vector3 point in endPoints)
                {
                    float distance2 = Vector3.Distance(position, point);

                    if (distance - distance2 > 1f)
                    {
                        distance = distance2;
                    }
                }
            }
            else
            {
                endPoints.Add(new Vector3(hit.point.x, _aimYOffset, hit.point.z));
            }
            Debug.DrawRay(position, throwDirection.normalized * distance, Color.green);
            Debug.DrawRay(leftPosition, throwDirection.normalized * distance, Color.green);
            Debug.DrawRay(rightPosition, throwDirection.normalized * distance, Color.green);

            endPoint = position + throwDirection.normalized * Mathf.Clamp(distance, 0f, 100f);
            
            linePoints = new[] {
                position,
                endPoint
            };

            _lineRenderer.SetPositions(linePoints);
            // Material tilling
            float scale = Mathf.Round(Vector3.Distance(linePoints[0], linePoints[1]) * 2f) / 2f;
            _lineMaterial.SetVector("_mainTextureScale", new Vector2(scale, 1f));
            //_lineMaterial.mainTextureOffset = new Vector2(-(scale - 1), 0f);

            _modelTransform.forward = hit2.point - _modelTransform.position;
            Vector3 rot = _modelTransform.rotation.eulerAngles;
            rot.x = 0f;
            rot.z = 0f;
            _modelTransform.rotation = Quaternion.Euler(rot);
        }
    }

    private void Throw()
    {
        ScoreHandler.curNumberOfThrows--;
        _lineRenderer.enabled = false;
        _shield = null;
        Vector3 pivotPoint = _shieldPivotPoint.position;
        pivotPoint.y = 0f;
        Vector3 playerPos = transform.position;
        playerPos.y = 0f;
        float length = (playerPos - pivotPoint).magnitude;
        Vector3 shieldSpawnPos = transform.position + throwDirection.normalized * length;

        if (ScoreHandler.curNumberOfThrows > 0)
        {
            throwEvent.Invoke(throwDirection.normalized, shieldSpawnPos, false);
        }
        else
        {
            throwEvent.Invoke(throwDirection.normalized, shieldSpawnPos, true);
        }

        AudioManager.PlayClipAtPosition(_throwClips[Random.Range(0, _throwClips.Length)], transform.position, 1f, 10f, Random.Range(.9f, 1.2f));
        UIMain.DestroyThrowImage();
        _animation.IsThrow = false;
    }

    private void DisableInput()
    {
        _inputDisabled = true;
    }

    private void VictoryAnimation()
    {
        _animation.IsVictory = true;
    }

    private void DefeatAnimation()
    {
        _animation.IsDefeated = true;
    }

    private void SpawnShield()
    {
        if (_shield != null || ScoreHandler.curNumberOfThrows <= 0) return;

        Vector3 spawnPos = _shieldPivotPoint.position;
        _shield = Instantiate(_shieldPrefab, spawnPos, Quaternion.identity);
        _shield.transform.parent = _shieldPivotPoint;
        _shield.transform.localRotation = _shieldPrefab.transform.rotation;
    }
}
