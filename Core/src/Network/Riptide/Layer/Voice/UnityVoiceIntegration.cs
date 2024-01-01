using Oculus.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace LabFusion.Riptide.Voice
{
    internal class UnityVoiceIntegration
    {
        private static AudioClip microphoneInput;
        public const int sampleRate = 44100; // Sample Rate
        public const int frameSize = 2048; // Size of the frame
        private static int position = 0; // Position in the audio clip
        public static string _microphone = Microphone.devices[0];

        public static void StartMicrophone()
        {
            microphoneInput = Microphone.Start(_microphone, true, 20, sampleRate);
        }

        public static void StopMicrophone()
        {
            Microphone.End(_microphone);
        }

        public static void ChangeMicrohpone(string microphoneName)
        {
            StopMicrophone();
            _microphone = microphoneName;
            StartMicrophone();
        }

        public static byte[] GetCompressedVoiceData()
        {
            // Get data from microphone into our buffer
            int microphonePosition = Microphone.GetPosition(null);

            byte[] data = null;
            if (microphonePosition < position)
            {
                // Microphone has looped, process remaining data
                data = ProcessData(position, microphoneInput.samples - position);
                position = 0;
            }
            if (microphonePosition > position)
            {
                // Process new data
                if (microphonePosition - position > 0)
                {
                    data = ProcessData(position, microphonePosition - position);
                    position = microphonePosition;
                }
            }

            return data;
        }

        private static byte[] ProcessData(int start, int length)
        {
            if (length > 0)
            {
                float[] dataStream = new float[length];
                microphoneInput.GetData(dataStream, start);

                // Convert to PCM and send
                byte[] pcmData = ConvertToPCM(dataStream);
                if (pcmData != null && pcmData.Length > 0)
                {
                    return pcmData;
                }
                else return null;
            }
            else return null;
        }

        private static byte[] ConvertToPCM(float[] dataStream)
        {
            Int16[] intData = new Int16[dataStream.Length];
            //converting in 2 steps : float[] to Int16[], //then Int16[] to Byte[]
            Byte[] bytesData = new Byte[dataStream.Length * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            int rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < dataStream.Length; i++)
            {
                intData[i] = (short)(dataStream[i] * rescaleFactor);
                Byte[] byteArr = new Byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var decompressedStream = new MemoryStream())
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                zipStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }
    }
}
