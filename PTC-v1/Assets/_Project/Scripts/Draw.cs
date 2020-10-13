using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour
{
    public bool mouseLookTesting;
    public GameObject stroke;
    public GameObject spacePenPoint;

    public static bool drawing = false;

    private float pitch = 0;
    private float yaw = 0;

    public DataController dataController;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mouseLookTesting)
        {
            yaw += 2 * Input.GetAxis("Mouse X");
            pitch -= 2 * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }

    public void StartStroke()
    {
        GameObject currentStroke;
        drawing = true;
        currentStroke = Instantiate(stroke, spacePenPoint.transform.position, spacePenPoint.transform.rotation) as GameObject;
        dataController.AddNewTrail(currentStroke.transform.position);
    }

    public void EndStroke()
    {
        drawing = false;
    }

}