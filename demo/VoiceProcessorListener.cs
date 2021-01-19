using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pv.Unity;

public class VoiceProcessorListener : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Available Devices: " + string.Join(",", VoiceProcessor.Instance.Devices.ToArray()));

        VoiceProcessor.Instance.OnRecordingStart += () => { Debug.Log("Recording started"); };
        VoiceProcessor.Instance.OnRecordingStop += () => { Debug.Log("Recording stopped"); };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (VoiceProcessor.Instance.IsRecording)
            {
                VoiceProcessor.Instance.OnFrameCaptured -= VoiceProcessor_OnFrameCaptured;
                VoiceProcessor.Instance.StopRecording();
            }
            else
            {
                VoiceProcessor.Instance.OnFrameCaptured += VoiceProcessor_OnFrameCaptured;
                VoiceProcessor.Instance.StartRecording();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            VoiceProcessor.Instance.ChangeDevice(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            VoiceProcessor.Instance.ChangeDevice(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            VoiceProcessor.Instance.ChangeDevice(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            VoiceProcessor.Instance.ChangeDevice(3);
        }
    }

    private void VoiceProcessor_OnFrameCaptured(short[] frame)
    {
        float rmsSum = 0;
        for (int i = 0; i < frame.Length; i++)
        {
            rmsSum += Mathf.Pow(frame[i], 2);
        }
        float rms = Mathf.Sqrt(rmsSum / frame.Length);

        float dBFS = 20 * Mathf.Log10(rms);
        if (float.IsInfinity(dBFS) || float.IsNaN(dBFS))
        {
            return;
        }
        float scale = (dBFS - 30) / 5;
        if (scale < 0)
        {
            return;
        }
        gameObject.transform.localScale = new Vector3(1, scale, 1);
    }
}
