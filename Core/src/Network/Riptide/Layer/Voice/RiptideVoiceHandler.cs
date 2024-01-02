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
        private readonly Queue<byte[]> _streamingReadQueue = new();

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

            bytes = UnityVoiceIntegration.Decompress(bytes);
            _streamingReadQueue.Enqueue(bytes);
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

            // Check if there is any voice data in the queue
            if (_streamingReadQueue.Count > 0)
            {
                // Dequeue the voice data
                byte[] voiceData = _streamingReadQueue.Dequeue();

                // Convert the voice data to float and add it to the data array
                for (int i = 0; i < voiceData.Length / 2; i++)
                {
                    data[i] = (BitConverter.ToInt16(voiceData, i * 2) / 32767f) * mult;
                }
            }
        }
    }
}