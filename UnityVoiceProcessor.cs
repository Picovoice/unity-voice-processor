﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Pv.Unity
{
    /// <summary>
    /// Class that records audio and delivers frames for real-time audio processing
    /// </summary>
    public class UnityVoiceProcessor : MonoBehaviour
    {
        /// <summary>
        /// Mixer to manager microphone audio
        /// </summary>
        public AudioMixerGroup unityVoiceProcessorMixer;

        /// <summary>
        /// Indicates whether microphone is capturing or not
        /// </summary>
        public bool IsRecording { get; private set; }

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
        /// Available audio recording devices
        /// </summary>
        public List<string> Devices { get; private set; }

        /// <summary>
        /// Index of selected audio recording device
        /// </summary>
        public int CurrentDeviceIndex { get; private set; } = -1;
                
        AudioSource _audioSource;        
        short[] _pcmBuffer;

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
        static UnityVoiceProcessor _instance;
        public static UnityVoiceProcessor Instance
        {
            get {
                if (_instance == null) FindObjectOfType<UnityVoiceProcessor>();
                if (_instance == null)
                {
                    _instance = new GameObject("Pv.UnityVoiceProcessor").AddComponent<UnityVoiceProcessor>();                    
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_audioSource == null) GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            UpdateDevices();
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
            if (Microphone.IsRecording(CurrentDeviceName))
                StopRecording();
            else
                Microphone.End(CurrentDeviceName);

            CurrentDeviceIndex = deviceIndex;
            StartRecording(SampleRate, FrameLength);
        }

        /// <summary>
        /// Start recording audio
        /// </summary>
        /// <param name="sampleRate">Sample rate to record at</param>
        /// <param name="frameSize">Size of audio frames to be delivered</param>
        public void StartRecording(int sampleRate = 16000, int frameSize = 512)
        {
            StopRecording();

            IsRecording = true;
            SampleRate = sampleRate;
            FrameLength = frameSize;
            
            _audioSource.clip = Microphone.Start(CurrentDeviceName, true, 1, sampleRate);            
            _audioSource.outputAudioMixerGroup = unityVoiceProcessorMixer;
            _pcmBuffer = new short[frameSize];
           
            StartCoroutine(RecordData());
        }

        /// <summary>
        /// Stops recording audio
        /// </summary>
        public void StopRecording()
        {
            if (!Microphone.IsRecording(CurrentDeviceName)) 
                return;

            IsRecording = false;

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
                for (int i = 0; i < FrameLength; i++)
                {
                    _pcmBuffer[i] = (short)Math.Floor(sampleBuffer[i] * short.MaxValue);
                }

                // raise buffer event
                OnFrameCaptured?.Invoke(_pcmBuffer);                                                
            }
        }
    }
}