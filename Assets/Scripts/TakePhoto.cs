using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Text;

public class TakePhoto : MonoBehaviour {

    public Debugger debugger;
    UnityEngine.Windows.WebCam.PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;
    public ServerRequester serverRequester;

    // Use this for initialization
    void Start() {
        cameraResolution = UnityEngine.Windows.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

    }

    public void TakePhotoNow() {
        debugger.Write("TakePhotoNow Began");
        UnityEngine.Windows.WebCam.PhotoCapture.CreateAsync(false, delegate (UnityEngine.Windows.WebCam.PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            UnityEngine.Windows.WebCam.CameraParameters cameraParameters = new UnityEngine.Windows.WebCam.CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = UnityEngine.Windows.WebCam.CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result) {
            // Take a picture
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            debugger.Write("TakePhotoNow Finished");
            });
        });
    }

    void OnCapturedPhotoToMemory(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.Windows.WebCam.PhotoCaptureFrame photoCaptureFrame) {
        debugger.Write("OnCapturedPhotoToMemory");
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Create a GameObject to which the texture can be applied
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Renderer quadRenderer = quad.GetComponent<Renderer>() as Renderer;
        // quadRenderer.material = new Material(Shader.Find("Custom/Unlit/UnlitTexture"));

        quad.transform.parent = this.transform;
        quad.transform.localPosition = new Vector3(0.0f, -0.5f, 0.8f);

        quadRenderer.material.SetTexture("_MainTex", targetTexture);

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

        if (result.success)
        {
            // Convert the photoCaptureFrame into a byte array
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            byte[] imageBuffer = imageBufferList.ToArray();

            if (imageBuffer == null || imageBuffer.Length == 0) {
                debugger.Write("Image buffer is null or empty!");
                return;
            }

            // Send the byte array in a web request
            StartCoroutine(SendPhoto(imageBuffer));
            // serverRequester.imageBuffer = imageBuffer;
            debugger.Write("Saved image to global imageBuffer");
        }
        else
        {
            Debug.LogError("Failed to capture photo to memory!");
        }
    }

    IEnumerator SendPhoto(byte[] imageBuffer)
    {
        // Create a new WWWForm
        WWWForm form = new WWWForm();

        // Add the image buffer to the form
        form.AddBinaryData("image_data", imageBuffer);

        // Add the camera dimensions to the form
        form.AddField("width", cameraResolution.width);
        form.AddField("height", cameraResolution.height);

        // Create a new UnityWebRequest
        UnityWebRequest request = UnityWebRequest.Post("https://jarvis.loca.lt/saveImage", form);

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            debugger.Write("Error sending image: " + request.error);
        }
        else
        {
            debugger.Write("Image sent successfully!");
        }
    }

    void OnStoppedPhotoMode(UnityEngine.Windows.WebCam.PhotoCapture.PhotoCaptureResult result) {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}