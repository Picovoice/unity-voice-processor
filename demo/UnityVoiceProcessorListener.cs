using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pv.Unity;

public class UnityVoiceProcessorListener : MonoBehaviour
{        
    void Start()
    {
        Debug.Log("Available Devices: " + string.Join(",", UnityVoiceProcessor.Instance.Devices));
    }
    
    void Update()
    {
        // while space is held, we record audio
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);        
        if (isSpaceHeld && !UnityVoiceProcessor.Instance.IsRecording)
        {            
            UnityVoiceProcessor.Instance.OnFrameCaptured += VoiceProcessor_OnFrameCaptured;
            UnityVoiceProcessor.Instance.StartRecording(16000, 512);            
        }
        else if(!isSpaceHeld && UnityVoiceProcessor.Instance.IsRecording) 
        {
            UnityVoiceProcessor.Instance.StopRecording();
            UnityVoiceProcessor.Instance.OnFrameCaptured -= VoiceProcessor_OnFrameCaptured;
        }
    }

    // demo effect
    private void VoiceProcessor_OnFrameCaptured(short[] audioFrame)
    {
        // measure rms power of incoming frame of audio
        float rmsSum = 0;
        for (int i = 0; i < audioFrame.Length; i++)
        {
            rmsSum += Mathf.Pow(audioFrame[i], 2);
        }
        float rms = Mathf.Sqrt(rmsSum / audioFrame.Length);

        // convert to dB level
        float dBFS = 20 * Mathf.Log10(rms);
        
        // scale cube using this value
        gameObject.transform.localScale = new Vector3(1, (dBFS - 30) / 5, 1);

    }
}
