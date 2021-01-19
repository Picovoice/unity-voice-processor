using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Pv.Unity
{
    /// <summary>
    /// Class that records audio and delivers frames for real-time audio processing
    /// </summary>
    public class VoiceProcessor : MonoBehaviour
    {
        /// <summary>
        /// Mixer to manager microphone audio
        /// </summary>
        public AudioMixerGroup VoiceProcessorMixer;

        /// <summary>
        /// Indicates whether microphone is capturing or not
        /// </summary>
        public bool IsRecording
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentDeviceName))
                    return false;
                else
                    return _audioSource.clip != null && Microphone.IsRecording(CurrentDeviceName);
            }
        }

        /// <summary>
        /// Sample rate of recorded audio
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Size of audio frames that are delivered
        /// </summary>
        public int FrameLength { get; private set; }

        /// <summary>
        /// Event where frames of audio are delivered
        /// </summary>
        public event Action<short[]> OnFrameCaptured;

        /// <summary>
        /// Event when audio capture thread stops
        /// </summary>
        public event Action OnRecordingStop;

        /// <summary>
        /// Event when audio capture thread starts
        /// </summary>
        public event Action OnRecordingStart;

        /// <summary>
        /// Available audio recording devices
        /// </summary>
        public List<string> Devices { get; private set; }

        /// <summary>
        /// Index of selected audio recording device
        /// </summary>
        public int CurrentDeviceIndex { get; private set; } = -1;

        /// <summary>
        /// Name of selected audio recording device
        /// </summary>
        public string CurrentDeviceName
        {
            get
            {
                if (CurrentDeviceIndex < 0 || CurrentDeviceIndex >= Microphone.devices.Length)
                    return string.Empty;
                return Devices[CurrentDeviceIndex];
            }
        }

        /// <summary>
        /// Singleton access
        /// </summary>
        static VoiceProcessor _instance;
        public static VoiceProcessor Instance
        {
            get
            {
                if (_instance == null) FindObjectOfType<VoiceProcessor>();
                if (_instance == null)
                {
                    _instance = new GameObject("Pv.Unity.VoiceProcessor").AddComponent<VoiceProcessor>();
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        AudioSource _audioSource;
        private event Action RestartRecording;

        void Awake()
        {
            if (_audioSource == null) GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            UpdateDevices();
            if (Devices == null || Devices.Count == 0)
            {
                Debug.LogError($"There is no valid recording device connected");
                return;
            }

            CurrentDeviceIndex = 0;
        }

        /// <summary>
        /// Updates list of available audio devices
        /// </summary>
        public void UpdateDevices()
        {
            Devices = new List<string>();
            foreach (var device in Microphone.devices)
                Devices.Add(device);
        }

        /// <summary>
        /// Change audio recording device
        /// </summary>
        /// <param name="deviceIndex">Index of the new audio capture device</param>
        public void ChangeDevice(int deviceIndex)
        {
            if (deviceIndex < 0 || deviceIndex >= Devices.Count)
            {
                Debug.LogError($"Specified device index {deviceIndex} is not a valid recording device");
                return;
            }

            if (IsRecording)
            {
                // one time event to restart recording with the new device 
                // the moment the last session has completed
                RestartRecording += () =>
                {
                    CurrentDeviceIndex = deviceIndex;
                    StartRecording(SampleRate, FrameLength);
                    RestartRecording = null;
                };
                StopRecording();
            }
            else
            {
                CurrentDeviceIndex = deviceIndex;
            }
        }

        /// <summary>
        /// Start recording audio
        /// </summary>
        /// <param name="sampleRate">Sample rate to record at</param>
        /// <param name="frameSize">Size of audio frames to be delivered</param>
        public void StartRecording(int sampleRate = 16000, int frameSize = 512)
        {
            if (IsRecording)
            {
                // one time event to restart recording with the parameters
                // the moment the last session has completed
                RestartRecording += () =>
                {
                    StartRecording(sampleRate, frameSize);
                    RestartRecording = null;
                };

                StopRecording();
                return;
            }

            SampleRate = sampleRate;
            FrameLength = frameSize;

            _audioSource.clip = Microphone.Start(CurrentDeviceName, true, 1, sampleRate);
            _audioSource.outputAudioMixerGroup = VoiceProcessorMixer;

            StartCoroutine(RecordData());
        }

        /// <summary>
        /// Stops recording audio
        /// </summary>
        public void StopRecording()
        {
            if (!IsRecording)
                return;

            Microphone.End(CurrentDeviceName);
            Destroy(_audioSource.clip);
            _audioSource.clip = null;

            StopCoroutine(RecordData());
        }

        /// <summary>
        /// Loop for buffering incoming audio data and delivering frames
        /// </summary>
        IEnumerator RecordData()
        {
            float[] sampleBuffer = new float[FrameLength];
            int startReadPos = 0;

            OnRecordingStart?.Invoke();

            while (_audioSource.clip != null && Microphone.IsRecording(CurrentDeviceName))
            {
                int curClipPos = Microphone.GetPosition(CurrentDeviceName);
                if (curClipPos < startReadPos)
                    curClipPos += _audioSource.clip.samples;

                int samplesAvailable = curClipPos - startReadPos;
                if (samplesAvailable < FrameLength)
                {
                    yield return null;
                    continue;
                }

                int endReadPos = startReadPos + FrameLength;
                if (endReadPos > _audioSource.clip.samples)
                {
                    // fragmented read (wraps around to beginning of clip)
                    // read bit at end of clip
                    int numSamplesClipEnd = _audioSource.clip.samples - startReadPos;
                    float[] endClipSamples = new float[numSamplesClipEnd];
                    _audioSource.clip.GetData(endClipSamples, startReadPos);

                    // read bit at start of clip
                    int numSamplesClipStart = endReadPos - _audioSource.clip.samples;
                    float[] startClipSamples = new float[numSamplesClipStart];
                    _audioSource.clip.GetData(startClipSamples, 0);

                    // combine to form full frame
                    Buffer.BlockCopy(endClipSamples, 0, sampleBuffer, 0, numSamplesClipEnd);
                    Buffer.BlockCopy(startClipSamples, 0, sampleBuffer, numSamplesClipEnd, numSamplesClipStart);
                }
                else
                {
                    _audioSource.clip.GetData(sampleBuffer, startReadPos);
                }

                startReadPos = endReadPos % _audioSource.clip.samples;

                // converts to 16-bit int samples
                short[] pcmBuffer = new short[sampleBuffer.Length];
                for (int i = 0; i < FrameLength; i++)
                {
                    pcmBuffer[i] = (short)Math.Floor(sampleBuffer[i] * short.MaxValue);
                }

                // raise buffer event
                OnFrameCaptured?.Invoke(pcmBuffer);
            }

            OnRecordingStop?.Invoke();
            RestartRecording?.Invoke();
        }
    }
}