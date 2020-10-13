using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController : MonoBehaviour
{
    private AppData appData;
    
    // Start is called before the first frame update
    void Start()
    {
        appData = new AppData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddNewAnchor(string text, Vector3 position)
    {
        appData.AnchorList[appData.AnchorCount] = new UserAnchor(text, position);
    }

    public void AddNewTrail(Vector3 position)
    {
        appData.TrailList[appData.TrailCount] = new UserTrail(position);
    }

    public void SaveData()
    {
        DataManager.SaveAppDataToFile(appData);
    }

    public void LoadData()
    {
        appData = DataManager.LoadAppDataFromFile();
    }

}
