using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
  
    [Header("Positioning")]
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject rotationPoint;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private float maxViewingAngle;
    [SerializeField] private float minviewingAngle;
    [SerializeField] private float rotationSensitivity;

    [Header("Zooming")]
    [SerializeField] private float zoomOutFOV;
    [SerializeField] private float zoomInFOV;

    //properties
    public Vector3 FollowOffset { get { return followOffset; } }
    public GameObject Target { set { target = value; } }
    public GameObject RotationPoint { set { rotationPoint = value; } }
    //references 
    private float verticalRotationAngle;

    
    //check if camera is zoomed in
    public bool IsZoomedIn { get { return  Mathf.RoundToInt(GetComponent<Camera>().fieldOfView) == Mathf.RoundToInt(zoomInFOV); } }

    private void FixedUpdate()
    {
        if(target != null)
        {
            //Make the camera look at the target.
            float yAngle = target.transform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, yAngle, 0);
            transform.position = target.transform.position - (rotation * followOffset);
            transform.LookAt(target.transform.position + offset);

            //Make the camera look up and down.
           // verticalRotationAngle += Input.GetAxis("Mouse Y") * rotationSensitivity;
            // Add min and max viewing angles to the mouse view.
            verticalRotationAngle = Mathf.Clamp(verticalRotationAngle + Input.GetAxis("Mouse Y") * rotationSensitivity, minviewingAngle, maxViewingAngle);
            
            // rotate around the rotation point.
            transform.RotateAround(rotationPoint.transform.position, rotationPoint.transform.right, -verticalRotationAngle);
        }
    }

    public void ZoomIn()
    {
        GetComponent<Camera>().fieldOfView = zoomInFOV;
        //Calibrate the cameras 
        followOffset.y = -1.48f;
        followOffset.z = 1.77f;
    }
    public void ZoomOut()
    {
        GetComponent<Camera>().fieldOfView = zoomOutFOV;
        //Calibrate the cameras 
        followOffset.y = -1.9f;
        followOffset.z = 3.54f;
    }

    public void TriggerZoom()
    {
        if(IsZoomedIn)
        {
            ZoomOut();
        }
        else
        {
            ZoomIn();
        }
    }
}
