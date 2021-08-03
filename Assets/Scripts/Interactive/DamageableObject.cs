using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableObject : InteractableObject
{
    //[SerializeField] protected Rigidbody _rigidBody = null;
    [SerializeField] protected LayerMask _enemyLayer = 0;
    [SerializeField] private float _velocityToStayHit = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1f && collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            CheckForLastEnemy(collision.gameObject);
            enemy.GetHit();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > _velocityToStayHit && collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            CheckForLastEnemy(collision.gameObject);
            enemy.GetHit();
        }
    }

    protected void CheckForLastEnemy(GameObject enemy, Transform cameraTarget = null)
    {
        if (enemy.layer != Mathf.Log(_enemyLayer, 2)) return;

        if(GameManager.current.EnemyCount == 1 && Zoom.isSlowMoActivated == false)
        {
            if(cameraTarget == null)
                Zoom.Activate(enemy.GetComponent<Enemy>().zoomCameraTarget);
            else
                Zoom.Activate(cameraTarget, true);
        }
    }
}
