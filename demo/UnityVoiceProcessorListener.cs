using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pv.Unity;

public class UnityVoiceProcessorListener : MonoBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Available Devices: " + string.Join(",", UnityVoiceProcessor.Instance.Devices));
    }

    private void VoiceProcessor_OnFrameCaptured(short[] frame)
    {
        float rmsSum = 0;
        for(int i = 0; i < frame.Length; i++)
        {
            rmsSum += Mathf.Pow(frame[i], 2);
        }
        float rms = Mathf.Sqrt(rmsSum / frame.Length);

        float dBFS = 20 * Mathf.Log10(rms);
        if(float.IsInfinity(dBFS) || float.IsNaN(dBFS))
        { 
            return; 
        }    
        gameObject.transform.localScale = new Vector3(1, (dBFS - 30) / 5, 1);
    }

    // Update is called once per frame
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
                UnityVoiceProcessor.Instance.StartRecording(16000, 512);
            }
        }        
    }
}
