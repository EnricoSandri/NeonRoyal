using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFov : MonoBehaviour
{
    public Camera mainCamera;
    private Camera thisCamera;
    private float cameraFov;

    private void Start()
    {
        thisCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        cameraFov = mainCamera.fieldOfView;
        thisCamera.fieldOfView = cameraFov;

    }
}
