using System.Collections;
using UnityEngine;

public class MovingWall : InteractableObject
{
    [SerializeField] private Vector3 _moveDistance;
    [SerializeField] private float _moveDuration = .2f;
    [SerializeField] private int _requiredButtonsNumber = 1;
    private int _curButtonsPressed;

    public override void Activate()
    {
        _curButtonsPressed++;

        if(_curButtonsPressed >= _requiredButtonsNumber)
            StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        Vector3 targetPos = transform.position + _moveDistance;
        float duration = _moveDuration + Time.time;

        while (duration > Time.time)
        {
            transform.Translate(_moveDistance / _moveDuration  * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPos;
    }
}