using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Transform _parentTransform = null;
    [SerializeField] private Animator _animator = null;

    [Header("For Player")]
    private int isAimParamID;
    private int isThrowParamID;
    private int isVictoryParamID;
    private int isDefeatedParamID;
    private bool isAim;
    private bool isThrow;
    private bool isVictory;
    private bool isDefeated;

    [Header("For Enemy")]
    private int isMovingParamID;
    private int leftTurnParamID;
    private int rightTurnParamID;
    private bool isMoving;
    [HideInInspector] public bool isTurning;

    public bool IsAim
    {
        set 
        {
            isAim = value;
            _animator.SetBool(isAimParamID, value);
        }
        get { return isAim; }
    }
    public bool IsThrow
    {
        set
        {
            isThrow = value;
            _animator.SetBool(isThrowParamID, value);
        }
        get { return isThrow; }
    }
    public bool IsVictory
    {
        set
        {
            isVictory = value;

            if (value == true)
                _animator.SetTrigger(isVictoryParamID);
        }
    }
    public bool IsDefeated
    {
        set
        {
            isDefeated = value;
            
            if(value == true)
                _animator.SetTrigger(isDefeatedParamID);
        }
    }
    public bool IsMove
    {
        set
        {
            isMoving = value;
            _animator.SetBool(isMovingParamID, value);
        }
    }
    [HideInInspector] public UnityEvent spawnShieldEvent;
    [HideInInspector] public UnityEvent throwEvent;

    [Header("For Enemy")]
    [SerializeField] private Rigidbody[] _ragdollRigidbodies = new Rigidbody[0];
    [SerializeField] private Collider[] _ragdollColliders = new Collider[0];

    private void Awake()
    {
        spawnShieldEvent = new UnityEvent();
        throwEvent = new UnityEvent();

        isVictoryParamID = Animator.StringToHash("isVictory");
        isDefeatedParamID = Animator.StringToHash("isDefeated");
        isThrowParamID = Animator.StringToHash("isThrow");
        isAimParamID = Animator.StringToHash("isAim");

        isMovingParamID = Animator.StringToHash("isMoving");
        leftTurnParamID = Animator.StringToHash("turnLeft");
        rightTurnParamID = Animator.StringToHash("turnRight");
    }

    public void ActivateRagdoll()
    {
        _animator.enabled = false;

        for (int i = 0; i < _ragdollRigidbodies.Length; i++)
        {
            _ragdollRigidbodies[i].isKinematic = false;
            _ragdollColliders[i].enabled = true;
        }
    }

    public void SpawnShield()
    {
        spawnShieldEvent.Invoke();
        IsThrow = false;
    }

    public void Throw()
    {
        throwEvent.Invoke();
    }

    private void OnAnimatorMove()
    {
        if(_parentTransform != null && _animator != null)
        {
            _parentTransform.position += _animator.deltaPosition;
            _parentTransform.forward = _animator.deltaRotation * transform.forward;
        }
    }

    public void LeftTurn()
    {
        _animator.SetTrigger(leftTurnParamID);
        isTurning = true;
    }

    public void RightTurn()
    {
        _animator.SetTrigger(rightTurnParamID);
        isTurning = true;
    }

    public void TurnEnd()
    {
        isTurning = false;
    }
}
