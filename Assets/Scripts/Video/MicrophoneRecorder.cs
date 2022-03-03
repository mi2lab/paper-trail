using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace pmjo.NextGenRecorder
{
    [AddComponentMenu("Next Gen Recorder/Microphone Recorder")]
    public class MicrophoneRecorder : Recorder.AudioRecorderBase
    {
        // ***** Singleton *****
        public static MicrophoneRecorder Instance { get; private set; }

        int SampleRate = 16000;
        const int RecordingLength = 10;

        public AudioSource audioSource;
        string micName;

        private bool isAudioRecording;

        private void Awake()
        {
            Recorder.RecordingStarted += Recorder_RecordingStarted;
            Recorder.RecordingStopped += Recorder_RecordingStopped;

            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (gCHandle.IsAllocated)
                gCHandle.Free();

            if (ListenCoroutine != null) StopCoroutine(ListenCoroutine);

            Recorder.RecordingStarted -= Recorder_RecordingStarted;
            Recorder.RecordingStopped -= Recorder_RecordingStopped;
        }

        private void Recorder_RecordingStopped(long sessionId)
        {
            Microphone.End(micName);
            Debug.Log("audioClip length: " + audioSource.clip.length);
        }

        private void Recorder_RecordingStarted(long sessionId)
        {
            AudioConfiguration configuration = AudioSettings.GetConfiguration();
            if (configuration.sampleRate < 8000)
            {
                Debug.Log("Only sample rate >= 8000 is supported");
                return;
            }
            else
            {
                SampleRate = configuration.sampleRate;
            }

            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("No Microphone Available");
                return;
            }

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            micName = Microphone.devices[0];

            Debug.Log("Starting Microphone: " + micName);


            audioSource.clip = Microphone.Start(micName, true, RecordingLength, SampleRate);
            audioSource.loop = true;
            audioSource.mute = true;


            while (!(Microphone.GetPosition(micName) > 0)) { }

            audioSource.Play();

            Debug.Log("Microphone Initialized");

            ListenCoroutine = StartCoroutine(Listen(audioSource.clip.channels, 2));
        }

        Coroutine ListenCoroutine = null;
        GCHandle gCHandle;

        int readSize;
        bool AwaitingFilterRead;
        IEnumerator Listen(int readChannels, int writeChannels)
        {
            int MaxSamples = SampleRate * RecordingLength;

            float[] fReadData = new float[MaxSamples * readChannels];
            float[] fWriteData = new float[MaxSamples * writeChannels];

            int unwrappedReadHead = 0;
            int readHead = 0;
            readSize = 0;

            gCHandle = GCHandle.Alloc(fWriteData, GCHandleType.Pinned);
            if (gCHandle.IsAllocated == false)
            {
                Debug.LogError("Failed to allocated GC Handle");
                yield break;
            }

            while (true)
            {
                yield return null;
                if (AwaitingFilterRead) continue;

                int writeHead = Microphone.GetPosition(micName);

                if (writeHead < unwrappedReadHead) writeHead += MaxSamples;

                int positionDelta = writeHead - unwrappedReadHead;

                if (positionDelta == 0) continue;

                audioSource.clip.GetData(fReadData, readHead);

                for (int wc = 0; wc < writeChannels; wc++)
                {
                    var rc = wc % readChannels;
                    for (int i = 0; i < positionDelta; i++)
                        fWriteData[(i * writeChannels) + wc] = fReadData[(i * readChannels) + rc];
                }

                unwrappedReadHead = readHead + positionDelta;
                readHead = unwrappedReadHead % audioSource.clip.samples;
                readSize = positionDelta * writeChannels;
                AwaitingFilterRead = true;
            }
        }

        private void OnAudioFilterRead(float[] d, int c)
        {
            if (readSize > 0)
            {
                base.AppendInterleavedAudio(gCHandle.AddrOfPinnedObject(), readSize, c, SampleRate);
                readSize = 0;
                AwaitingFilterRead = false;
            }
        }

        //#region Audio Recording
        //public void OnAudioRecordButtonPressed()
        //{
        //    if (isAudioRecording)   // stop recording
        //    {
        //        Recorder_RecordingStopped(-1);   // invalid session id, hope this doesn't break something :P 
        //    }

        //    else   // start recording
        //    {
        //        Recorder_RecordingStarted(-1);   // invalid session id, hope this doesn't break something :P 
        //    }
        //}

        
        //#endregion
    }
}