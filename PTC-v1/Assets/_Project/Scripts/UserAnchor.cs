using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserAnchor
{
    public string text;
    public Vector3 position;

    UserAnchor()
    {
        this.text = "";
        this.position = new Vector3();
    }

    public UserAnchor(string text, Vector3 position)
    {
        this.text = text;
        this.position = position;
    }
}
