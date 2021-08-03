using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallButton : MonoBehaviour
{
    [SerializeField] private float _activationTime = .1f;
    [SerializeField] private InteractableObject _relatedObject = null;
    [SerializeField] private LayerMask _shieldLayer = 0;
    [SerializeField] private Collider _collider = null;
    [SerializeField] private MeshFilter _mesh = null;
    [SerializeField] private Transform _button = null;

    private void OnCollisionEnter(Collision collision)
    {
        if ((1 << collision.gameObject.layer & _shieldLayer) != 0)
        {
            _relatedObject?.Activate();
            StartCoroutine(PressButton());
        }
    }

    private IEnumerator PressButton()
    {
        float duration = _activationTime + Time.time;
        float buttonSize = /*_collider.bounds.size.z*/_mesh.mesh.bounds.size.z * .7f;

        while (duration > Time.time)
        {
            _button.transform.Translate(Vector3.forward * Time.deltaTime * buttonSize / _activationTime);
            yield return new WaitForEndOfFrame();
        }

        _collider.isTrigger = true;
    }
}
