using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserTrail
{
    public Vector3 position;

    UserTrail()
    {
        this.position = new Vector3();
    }

    public UserTrail(Vector3 position)
    {
        this.position = position;
    }
}
