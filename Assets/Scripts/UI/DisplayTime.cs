using UnityEngine;
using System;
using TMPro;

public class DisplayTime : MonoBehaviour
{
    private TextMeshProUGUI timeDisplay;
    void Start()
    {
        timeDisplay = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        DateTime now = DateTime.Now;
        timeDisplay.text = now.ToString("HH:mm:ss");
    }
}
