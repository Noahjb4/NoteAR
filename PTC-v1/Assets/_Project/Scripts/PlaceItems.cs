using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]

public class PlaceItems : MonoBehaviour
{
    public GameObject objectToSpawn;

    private GameObject spawnedObject;
    private ARRaycastManager _arRaycastManager;
    private Vector2 touchPosition;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public DataController dataController;


    // Start is called before the first frame update
    void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
        spawnedObject = null;
    }

    bool GetTouchPosition(out Vector2 touchPosition) {
        if (Input.touchCount > 0) {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetTouchPosition(out Vector2 touchPosition)) {
            return;
        }

        if (_arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) {
            var hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(objectToSpawn, hitPose.position, hitPose.rotation);
                dataController.AddNewAnchor(spawnedObject.GetComponentInChildren<TextMesh>().text,spawnedObject.transform.position);
            }
            else {
                spawnedObject.transform.position = hitPose.position;
            }
        }
    }
}
