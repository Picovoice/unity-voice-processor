using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pv.Unity;

public class UnityVoiceProcessorListener : MonoBehaviour
{
    int sampleRate = 16000;
    int frameLength = 512;

    void Start()
    {
        Debug.Log("Available Devices: " + string.Join(",", UnityVoiceProcessor.Instance.Devices));
    }

    void Update()
    {                
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (UnityVoiceProcessor.Instance.IsRecording)
            {
                UnityVoiceProcessor.Instance.OnFrameCaptured -= VoiceProcessor_OnFrameCaptured;
                UnityVoiceProcessor.Instance.StopRecording();                
            }
            else
            {
                UnityVoiceProcessor.Instance.OnFrameCaptured += VoiceProcessor_OnFrameCaptured;
                UnityVoiceProcessor.Instance.StartRecording(sampleRate, frameLength);
            }
        }        
    }

    private void VoiceProcessor_OnFrameCaptured(short[] audioFrame)
    {
        float rmsSum = 0;
        for (int i = 0; i < audioFrame.Length; i++)
        {
            rmsSum += Mathf.Pow(audioFrame[i], 2);
        }
        float rms = Mathf.Sqrt(rmsSum / audioFrame.Length);

        float dBFS = 20 * Mathf.Log10(rms);
        if (float.IsInfinity(dBFS) || float.IsNaN(dBFS))
        {
            return;
        }
        gameObject.transform.localScale = new Vector3(1, (dBFS - 30) / 5, 1);
    }
}
