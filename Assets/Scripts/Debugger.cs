using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Debugger : MonoBehaviour
{
    public TMP_Text debugText;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Write(string text) {
        debugText.text += "\n" + text;
    }
}
