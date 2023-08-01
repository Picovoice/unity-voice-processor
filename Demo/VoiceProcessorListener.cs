//
// Copyright 2021 Picovoice Inc.
//
// You may not use this file except in compliance with the license. A copy of the license is located in the "LICENSE"
// file accompanying this source.
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//

using UnityEngine;

using Pv.Unity;

public class VoiceProcessorListener : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Available Devices: " + string.Join(",", VoiceProcessor.Instance.Devices.ToArray()));

        VoiceProcessor.Instance.AddFrameListener(_onFrameCaptured);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (VoiceProcessor.Instance.IsRecording)
            {
                VoiceProcessor.Instance.StopRecording();
            }
            else
            {
                VoiceProcessor.Instance.StartRecording(512, 16000);
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

    private void _onFrameCaptured(short[] frame)
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
