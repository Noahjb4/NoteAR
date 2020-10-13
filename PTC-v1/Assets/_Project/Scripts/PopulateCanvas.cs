using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateCanvas : MonoBehaviour
{
    public GameObject inputText;
    //private TextDemo AzureTextDemo;
    public TextMesh objectText;
    public GameObject Camera;
    // Start is called before the first frame update
    void Start()
    {
        //AzureTextDemo = (TextDemo)GameObject.FindObjectOfType(typeof(TextDemo));
        //AzureTextDemo.label = GetComponentInChildren<Text>();
        //AzureTextDemo.TappedLoadText();

        inputText = GameObject.Find("Input Text");
        Debug.Log(inputText);
        objectText = GetComponentInChildren<TextMesh>();
        Camera = GameObject.Find("AR Camera");

    }

    // Update is called once per frame
    void Update()
    {
        objectText.text = inputText.GetComponent<Text>().text;
        //Debug.Log(inputText.text);
        transform.LookAt(2*transform.position - Camera.transform.position);
    }
}
