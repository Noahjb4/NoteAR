using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;
using System;

public class ARTapToPlace : MonoBehaviour
{

    private ARRaycastManager arRaycastManager;
    //the position and rotation of the point
    private Pose placementPosition;
    private bool isValidPlacement = false;
    public GameObject placementMark;
    public GameObject objectToPlace;
    public Camera arCamera;

    List<GameObject> patientPanels = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementMarkPosition();
        if (isValidPlacement && Input.touchCount>0 && Input.GetTouch(0).phase==TouchPhase.Began)
        {
            PlaceObject();
        }
    }

    void UpdatePlacementPose()
    {
        var centerOfScreen = arCamera.ViewportToScreenPoint(new Vector3(0.5f,0.33f, 0));
        var hitList = new List<ARRaycastHit>();
        //get a flat Surface to place the object
        arRaycastManager.Raycast(centerOfScreen,hitList,TrackableType.Planes);
        isValidPlacement = hitList.Count > 0;

        if(isValidPlacement)
        {
            placementPosition= hitList[0].pose;
            var cameraDirection = arCamera.transform.forward;
            var cameraRotation = new Vector3(cameraDirection.x,0f,cameraDirection.z).normalized;
            placementPosition.rotation= Quaternion.LookRotation(cameraRotation);
        }
    }

    private void UpdatePlacementMarkPosition()
    {
        if (isValidPlacement)
        {
            placementMark.SetActive(true);
            placementMark.transform.SetPositionAndRotation(placementPosition.position,placementPosition.rotation);
         
        }
        else
        {
            placementMark.SetActive(false);
        }
    }

    public void PlaceObject()
    {
        //GameObject newPanel = (GameObject)Instantiate (objectToPlace,placementPosition.position,placementPosition.rotation);
        GameObject patientPanel=Instantiate(objectToPlace,placementPosition.position,placementPosition.rotation);
    }
}
