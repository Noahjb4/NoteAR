using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager 
{
    public static AppData LoadAppDataFromFile()
    {
        string path = Application.persistentDataPath + "/appData.json";
        Debug.Log(path);
        if (File.Exists(path))
        {
            string loadString = File.ReadAllText(path);
            return JsonUtility.FromJson<AppData>(loadString);

        }
        else
        {
            Debug.Log("There was an error opening the file");
            return null;
        }
    }

    public static void SaveAppDataToFile(AppData appData)
    {
        string path = Application.persistentDataPath + "/appData.json";
        Debug.Log("Saved data to" + path);
        string json = JsonUtility.ToJson(appData);
        File.WriteAllText(path, json);
        Debug.Log("File Saved");
      
    }

}
