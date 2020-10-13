using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineData : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NoteData exampleNote = new NoteData();
        exampleNote.position = transform.position;
        exampleNote.fileName = "20-04-21-21-14-19-27.png";
        exampleNote.format = "image";
        //Store as text for debug purposes only
        string jsonText=JsonUtility.ToJson(exampleNote);
        Debug.Log(jsonText);
        
       NoteData loadedNote= JsonUtility.FromJson<NoteData>(jsonText);

        Debug.Log("Loaded Position:"+loadedNote.position);
        Debug.Log("Loaded Format:" + loadedNote.format);
        Debug.Log("Loaded File Name:" + loadedNote.fileName);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private class NoteData
    {
        public Vector3 position;
        public string format;
        public string fileName;

    }
}
