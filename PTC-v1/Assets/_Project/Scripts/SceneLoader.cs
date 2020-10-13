using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void NewLog() { SceneManager.LoadScene("Main Scene"); }
    public void Menu() { SceneManager.LoadScene("Start Scene"); }
}
