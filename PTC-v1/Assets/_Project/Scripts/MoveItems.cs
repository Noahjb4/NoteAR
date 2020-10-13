using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;
using System;
using TMPro;
//Allows the user to reposition a Note/object once it is in the scene.
public class MoveItems : MonoBehaviour
{
    public Camera arCamera;
    private ARRaycastManager arRaycastManager;
    //the position and rotation of the point
    private Pose placementPosition;
    private bool isValidPlacement = false;
    public GameObject selectedObject;
    private Vector3 centerOfScreen;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool onTouchHold;
    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        centerOfScreen = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
    }
    void Update()
    {
        //SelectObject();
        if (Input.touchCount > 0)
        {
            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    SelectObject();
                    break;
                case TouchPhase.Stationary:
                    MoveObject();
                    break;
                case TouchPhase.Ended:
                    UnselectObject();
                    break;
                case TouchPhase.Moved:
                    RotateObject();
                    break;
                default:
                    break;
            }
        }
    }
    void MoveObject()
    {
        centerOfScreen = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        //get a flat Surface to place the object
        arRaycastManager.Raycast(centerOfScreen, hits, TrackableType.Planes);
        isValidPlacement = hits.Count > 0;
        if (isValidPlacement && selectedObject != null)
        {
            placementPosition = hits[0].pose;
            var cameraDirection = arCamera.transform.forward;
            var cameraRotation = new Vector3(cameraDirection.x, 0f, cameraDirection.z).normalized;
            placementPosition.rotation = Quaternion.LookRotation(cameraRotation);
            selectedObject.transform.SetPositionAndRotation(placementPosition.position, placementPosition.rotation);
        }
    }
    public void SelectObject()
    {
        Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            selectedObject = hit.transform.gameObject;
            TMP_Text txt = selectedObject.GetComponentInChildren<TMP_Text>();
            txt.color = Color.red;
        }
    }
    public void UnselectObject()
    {
        if (selectedObject != null)
        {
            TMP_Text txt = selectedObject.GetComponentInChildren<TMP_Text>();
            txt.color = Color.white;
            selectedObject = null;
        }
    }
    public void RotateObject()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.Rotate(0.0f, -Input.GetTouch(0).deltaPosition.x * .5f, 0.0f, Space.World);
        }
    }
}



