using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AppData
{
    public UserAnchor[] AnchorList;
    public UserTrail[] TrailList;
    public int AnchorCount;
    public int TrailCount;

    public AppData()
    {
        this.AnchorList = new UserAnchor[10];
        this.TrailList = new UserTrail[50];
        this.AnchorCount = 0;
        this.TrailCount = 0;

        
    }
}
