using LabFusion.Network;
using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Riptide.Voice
{
    public class RiptideVoiceManager : VoiceManager
    {
        public override VoiceHandler GetVoiceHandler(PlayerId id)
        {
            if (TryGetHandler(id, out var handler))
                return handler;

            var newIdentifier = new RiptideVoiceHandler(id);
            VoiceHandlers.Add(newIdentifier);
            return newIdentifier;
        }

        public override bool CanHear => false;
        public override bool CanTalk => false;
    }
}