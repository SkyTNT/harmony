
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class URLConverter : UdonSharpBehaviour
{

    public VRCUrlInputField inputField;
    public Text urlText;

    public void ConvertURL(string urlString)
    {
        //inputField.text = "https://www.youtube.com/watch?v=wPIEsvlyAz0";
        inputField.textComponent.text = "https://www.youtube.com/watch?v=wPIEsvlyAz0";
        urlText.text = "https://www.youtube.com/watch?v=wPIEsvlyAz0";
        inputField.ForceLabelUpdate();

        Debug.Log($"urlText.text {urlText.text}  : ");
        Debug.Log($"textComponent.text {inputField.textComponent.text}  : ");
        Debug.Log($"urlText.text IsNullOrEmpty {String.IsNullOrEmpty(inputField.GetUrl().Get())} : ");
        
        //var url = new VRCUrl("https://www.youtube.com/watch?v=wPIEsvlyAz0&list=PLiMgy154-zYrVEts6bPLqT5jdrsOr8KXj&index=4&t=30s");
        
    }
    
}
