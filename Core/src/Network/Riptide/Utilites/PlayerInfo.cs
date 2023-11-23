using BoneLib;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using LabFusion.Representation;
using Il2CppSystem;
using LabFusion.Utilities;
using Steamworks;
using MelonLoader;
using UnityEngine.Networking;
using Il2CppNewtonsoft.Json;

namespace LabFusion.Network.Riptide.Utilities
{
    public static class PlayerInfo
    {
        internal static string PlayerIpAddress;

        #region USERNAME
        public static void InitPlayerUsername()
        {
            if (!HelperMethods.IsAndroid())
            {
                if (System.IO.Path.GetFileName(UnityEngine.Application.dataPath) == "BONELAB_Oculus_Windows64_Data")
                {
                    Oculus.Platform.Core.Initialize("5088709007839657");
                    Oculus.Platform.Users.GetLoggedInUser().OnComplete((Message.Callback)GetLoggedInUserCallback);
                } 
                else
                {
                    try
                    {
                        if (!Steamworks.SteamClient.IsValid)
                            Steamworks.SteamClient.Init(480, false);

                        PlayerIdManager.SetUsername(new Friend(SteamClient.SteamId.Value).Name);

                        Steamworks.SteamClient.Shutdown();
                    }
                    catch (System.Exception e)
                    {
                        FusionLogger.Error($"Failed to initialize Steam Username! \n{e}");
                    }
                }
            } 
            else
            {
                Oculus.Platform.Core.Initialize("4215734068529064");
                Oculus.Platform.Users.GetLoggedInUser().OnComplete((Message.Callback)GetLoggedInUserCallback);
            }
        }

        private static void GetLoggedInUserCallback(Message msg)
        {
            if (!msg.IsError && msg.GetUser().DisplayName != string.Empty)
            {
                PlayerIdManager.SetUsername(msg.GetUser().DisplayName);
            } 
            else
            {
                PlayerIdManager.SetUsername("UNKNOWN USER");
            }
        }
        #endregion

        internal static void InitializePlayerIPAddress()
        {
            string ip = "";
            try
            {
                string link = "https://api.ipify.org";
                UnityWebRequest httpWebRequest = UnityWebRequest.Get(link);
                var requestSent = httpWebRequest.SendWebRequest();

                requestSent.m_completeCallback += new System.Action<AsyncOperation>((op) =>
                {
                    ip = httpWebRequest.downloadHandler.text;
                    if (httpWebRequest.result == UnityWebRequest.Result.ConnectionError || httpWebRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        FusionLogger.Error(httpWebRequest.error);
                        PlayerIpAddress = ip;
                    }
                    PlayerIpAddress = ip;
                });

            }
            catch (System.Exception e)
            {
                FusionLogger.Error($"Error when fetching IP address:");
                FusionLogger.Error(e);
            }
        }
    }
}
