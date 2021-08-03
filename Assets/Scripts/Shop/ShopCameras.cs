using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCameras : MonoBehaviour
{
    public CinemachineVirtualCamera[] virtualCameras;
    public bool[] busyCameras;
    public Camera[] cameras;

    public static ShopCameras current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            //...destroy this. There can be only one AudioManager
            Destroy(gameObject);
            return;
        }

        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        busyCameras = new bool[virtualCameras.Length];

        for(int i = 0; i < cameras.Length; i++)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(512, 512, 24, RenderTextureFormat.Default);
            cameras[i].targetTexture = renderTex;
            cameras[i].enabled = true;
        }
    }

    public static void SetCameraTarget(int cameraIndex, Transform target)
    {
        if(current.busyCameras[cameraIndex] == true)
        {
            Debug.Log("BUSY");
            return;
        }
        current.virtualCameras[cameraIndex].Follow = target;
        current.virtualCameras[cameraIndex].LookAt = target;
        current.busyCameras[cameraIndex] = true;
    }
}
