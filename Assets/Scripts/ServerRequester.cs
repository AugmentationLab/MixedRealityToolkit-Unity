// REQUEST.CS handles networking with remote servers
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ServerRequester : MonoBehaviour
{
    public Debugger debugger;
    public TMP_Text responseText;
    public TMP_Text inputText;
    // public byte[] imageBuffer;

    // FUNCTION RequestCommand takes in user input, sends it with current object references to the server, and displays the responses
    public void RequestCommand() {
        debugger.Write("Request Sent");
        debug("inputText");
        debug(inputText.text);

        StartCoroutine(GetResponse(new Dictionary<string, object> {
            {"userInput", inputText.text}
            // {"imageData", imageBuffer}
        },
        "https://jarvis.loca.lt/getResponse",
        OnResponse));
    }

    public void EmptyCallback(string response) {
        debug("EmptyCallback");
    }

    public void OnResponse(string response) {

        debug("SERVERCALL - response: " + response);

        responseText.text = response;

    }

    IEnumerator GetResponse(Dictionary<string, object> inputData, string requestURL, System.Action<string> callback)
    {
        WWWForm requestData = new WWWForm();
        foreach(var item in inputData)
        {
            requestData.AddField(item.Key, (string)item.Value);
        }

        using (UnityWebRequest www = UnityWebRequest.Post(requestURL, requestData))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                debug("NETWORKING - Request error: " + www.error);
                debugger.Write("NETWORKING - Request error: " + www.error);
                callback("oops");
            }
            else
            {
                debug("NETWORKING - Request Complete");
                debugger.Write("NETWORKING - Request Complete");
                string serverResponse = www.downloadHandler.text;
                debug("NETWORKING - Response: " + serverResponse);
                debugger.Write("NETWORKING - Response: " + serverResponse);
                responseText.text = serverResponse;
                // TODO: implement TTS
            }
        }
    }

    // FUNCTION debug takes in a debug message and prints it to the console, togglable on and off
    private void debug(string message) {
        Debug.Log(message);
    }


}