using LabFusion.Network.Riptide.Utilities;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.UI
{
    public class KeyboardCanvas : MonoBehaviour
    {
        public KeyboardCanvas(IntPtr intPtr) : base(intPtr) { }

        private bool isCaps = false;
        private TMP_Text DisplayTMP;
        public Keyboard Keyboard;

        private void Awake()
        {
            UIMachineUtilities.OverrideFonts(transform);
        }

        public void SetupReferences()
        {
            DisplayTMP = transform.Find("Display").Find("Display Text").GetComponent<TMP_Text>();
            foreach (var button in transform.GetComponentsInChildren<Button>())
            {
                switch (button.gameObject.name)
                {
                    case "button_Q":
                        button.AddClickEvent(() => OnClickKey("Q"));
                        break;
                    case "button_W":
                        button.AddClickEvent(() => OnClickKey("W"));
                        break;
                    case "button_E":
                        button.AddClickEvent(() => OnClickKey("E"));
                        break;
                    case "button_R":
                        button.AddClickEvent(() => OnClickKey("R"));
                        break;
                    case "button_T":
                        button.AddClickEvent(() => OnClickKey("T"));
                        break;
                    case "button_Y":
                        button.AddClickEvent(() => OnClickKey("Y"));
                        break;
                    case "button_U":
                        button.AddClickEvent(() => OnClickKey("U"));
                        break;
                    case "button_I":
                        button.AddClickEvent(() => OnClickKey("I"));
                        break;
                    case "button_O":
                        button.AddClickEvent(() => OnClickKey("O"));
                        break;
                    case "button_P":
                        button.AddClickEvent(() => OnClickKey("P"));
                        break;
                    case "button_A":
                        button.AddClickEvent(() => OnClickKey("A"));
                        break;
                    case "button_S":
                        button.AddClickEvent(() => OnClickKey("S"));
                        break;
                    case "button_D":
                        button.AddClickEvent(() => OnClickKey("D"));
                        break;
                    case "button_F":
                        button.AddClickEvent(() => OnClickKey("F"));
                        break;
                    case "button_G":
                        button.AddClickEvent(() => OnClickKey("G"));
                        break;
                    case "button_H":
                        button.AddClickEvent(() => OnClickKey("H"));
                        break;
                    case "button_J":
                        button.AddClickEvent(() => OnClickKey("J"));
                        break;
                    case "button_K":
                        button.AddClickEvent(() => OnClickKey("K"));
                        break;
                    case "button_L":
                        button.AddClickEvent(() => OnClickKey("L"));
                        break;
                    case "button_Z":
                        button.AddClickEvent(() => OnClickKey("Z"));
                        break;
                    case "button_X":
                        button.AddClickEvent(() => OnClickKey("X"));
                        break;
                    case "button_C":
                        button.AddClickEvent(() => OnClickKey("C"));
                        break;
                    case "button_V":
                        button.AddClickEvent(() => OnClickKey("V"));
                        break;
                    case "button_B":
                        button.AddClickEvent(() => OnClickKey("B"));
                        break;
                    case "button_N":
                        button.AddClickEvent(() => OnClickKey("N"));
                        break;
                    case "button_M":
                        button.AddClickEvent(() => OnClickKey("M"));
                        break;
                }
            }
        }

        private void OnClickKey(string key)
        {
            if (DisplayTMP == null)
            {
                FusionLogger.Error("DisplayTMP is null!");
                return;
            }

            string text = DisplayTMP.text;
            if (isCaps)
                text.Append(key.ToUpper()[0]);
            else
                text.Append(key.ToLower()[0]);

            DisplayTMP.SetText(text);
        }

        private void OnCycleCaps()
        {

        }

        private void OnCycle()
        {

        }
    }
}
