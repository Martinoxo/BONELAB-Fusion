using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using static UnityEngine.AudioClip;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using LabFusion.Preferences;
using LabFusion.Utilities;
using UnhollowerBaseLib;

namespace LabFusion.Riptide.Voice
{
    public class RiptideVoiceHandler : VoiceHandler
    {
        private const float _defaultVolumeMultiplier = 10f;

        private readonly MemoryStream _compressedVoiceStream = new();
        private readonly MemoryStream _decompressedVoiceStream = new();
        private readonly Queue<float> _streamingReadQueue = new();

        public RiptideVoiceHandler(PlayerId id)
        {
            // Save the id
            _id = id;
            OnContactUpdated(ContactsList.GetContact(id));

            // Hook into contact info changing
            ContactsList.OnContactUpdated += OnContactUpdated;

            // Create the audio source and clip
            CreateAudioSource();
            Source.clip = AudioClip.Create("RiptideVoice", UnityVoiceIntegration.sampleRate,
                        1, UnityVoiceIntegration.sampleRate, true, (PCMReaderCallback)PcmReaderCallback);

            _source.Play();

            // Set the rep's audio source
            VerifyRep();
        }

        private void OnContactUpdated(Contact contact)
        {
            Volume = contact.volume;
        }

        public override void Cleanup()
        {
            // Unhook contact updating
            ContactsList.OnContactUpdated -= OnContactUpdated;

            base.Cleanup();
        }

        public override void OnVoiceBytesReceived(byte[] bytes)
        {
            if (MicrophoneDisabled)
            {
                return;
            }

            VerifyRep();

            if (bytes == null)
            {
                FusionLogger.Log($"No voice data to handle");
                return;
            }
            FusionLogger.Log($"Handling voice data: {bytes[0]}{bytes[1]}{bytes[2]}{bytes[3]}{bytes[4]}{bytes[5]}");

            // Convert byte array to float array
            float[] floatData = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, floatData, 0, bytes.Length);

            // Add the data to the streamingReadQueue
            foreach (var data in floatData)
            {
                _streamingReadQueue.Enqueue(data);
            }
        }

        private float GetVoiceMultiplier()
        {
            float mult = _defaultVolumeMultiplier * FusionPreferences.ClientSettings.GlobalVolume * Volume;

            // If we are loading or the audio is 2D, lower the volume
            if (FusionSceneManager.IsLoading() || _source.spatialBlend <= 0f)
            {
                mult *= 0.25f;
            }

            return mult;
        }

        private void PcmReaderCallback(Il2CppStructArray<float> data)
        {
            float mult = GetVoiceMultiplier();

            for (int i = 0; i < data.Length; i++)
            {
                if (_streamingReadQueue.Count > 0)
                {
                    data[i] = _streamingReadQueue.Dequeue() * mult;
                }
                else
                {
                    data[i] = 0.0f;  // Nothing in the queue means we should just play silence
                }
            }
        }
    }
}