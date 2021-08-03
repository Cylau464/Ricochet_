using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Zoom : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _targetCamera = null;
    [SerializeField] private CinemachineVirtualCamera _panoramaCamera = null;
    [SerializeField] private float _slowMoScale = .3f;
    public static bool isSlowMoActivated;

    public static Zoom current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
    }

    private void Start()
    {
        GameManager.levelCompletedEvent.AddListener(TurnOffSlowMo);
        _targetCamera.transform.position = Camera.main.transform.position;
        _panoramaCamera.transform.position = Camera.main.transform.position;
    }

    public static void Activate(Transform cameraTarget, bool panorama = false)
    {
        current.SlowMo(cameraTarget, panorama);
    }

    private void SlowMo(Transform cameraTarget, bool panorama = false)
    {
        if (isSlowMoActivated == true) return;

        CinemachineVirtualCamera camera = panorama == true ? _panoramaCamera : _targetCamera;
        Time.timeScale = _slowMoScale;
        Time.fixedDeltaTime = .02f * Time.timeScale;
        camera.Priority = 100;
        camera.LookAt = cameraTarget;
        camera.Follow = cameraTarget;
        isSlowMoActivated = true;
    }

    private void TurnOffSlowMo()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = .02f;
        _panoramaCamera.Priority = 0;
        _targetCamera.Priority = 0;
        isSlowMoActivated = false;
    }

    private void OnDestroy()
    {
        TurnOffSlowMo();
    }
}
