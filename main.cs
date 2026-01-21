using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HarmonyLib;
using static MenuSkipper.MenuSkipper;
using static MenuSkipper.SharedState;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MenuSkipper
{
    public static class SharedState
    {
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MenuSkipper : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public const string pluginGuid = "shushu.casualtiesunknown.menuskipper";
        public const string pluginName = "Menu Skipper";
        public const string pluginVersion = "1.12.25";

        public static MenuSkipper Instance;

        public void Awake()
        {
            Instance = this;
            logger = Logger;

            logger.LogInfo("Awake() ran - mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);

            var TryLore_OG = AccessTools.Method(typeof(PreRunScript), "TryLore");
            var TryLore_Patch = typeof(MyPatches).GetMethod("PreRunScript_TryLore_MyPatches");

            harmony.Patch(TryLore_OG, prefix: new HarmonyMethod(TryLore_Patch));
            Log("Patched TryLore_OG.TryLore");

            var Start_OG = AccessTools.Method(typeof(PreRunScript), "Start");
            var Store_Patch = typeof(MyPatches).GetMethod("PreRunScript_Start_MyPatches");

            harmony.Patch(Start_OG, prefix: new HarmonyMethod(Store_Patch));
            Log("Patched Start_OG.TryLore");
        }

        public static void Log(string message)
        {
            logger.LogInfo(message);
        }
    }

    public class MyPatches
    {
        [HarmonyPatch(typeof(PreRunScript))]
        [HarmonyPatch("TryLore")]
        [HarmonyPrefix]
        public static bool PreRunScript_TryLore_MyPatches(PreRunScript __instance)
        {
            //AccessTools.Field(typeof(PreRunScript), "didIntro").SetValue(__instance, false);
            //return false;
            return true;
        }

        [HarmonyPatch(typeof(PreRunScript))]
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PreRunScript_Start_MyPatches(PreRunScript __instance)
        {
            var warningObj = GameObject.Find("Warning");
            warningObj?.SetActive(false);
        }
    }

    public static class UIDebugHelper
    {
        public static void DumpAllUIElements()
        {
            var allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            StringBuilder sb = new StringBuilder();

            foreach (var go in allGameObjects)
            {
                // Check if this GameObject has a Canvas or any UI-related components
                var canvas = go.GetComponent<Canvas>();
                var button = go.GetComponent<Button>();
                var eventTrigger = go.GetComponent<EventTrigger>();

                if (canvas || button || eventTrigger)
                {
                    sb.AppendLine($"GameObject: {go.name}");

                    if (canvas != null)
                        sb.AppendLine("  - Has Canvas");

                    if (button != null)
                    {
                        sb.AppendLine("  - Has Button");
                        DumpUnityEvent(sb, "    OnClick", button.onClick);
                    }

                    if (eventTrigger != null)
                    {
                        sb.AppendLine("  - Has EventTrigger");
                        foreach (var entry in eventTrigger.triggers)
                        {
                            sb.AppendLine($"    - Event: {entry.eventID}");
                            DumpUnityEvent(sb, "      Callbacks", entry.callback);
                        }
                    }

                    // You can add other UI components and their events here as needed
                }
            }

            Debug.Log(sb.ToString());
        }

        private static void DumpUnityEvent(StringBuilder sb, string label, UnityEngine.Events.UnityEventBase unityEvent)
        {
            if (unityEvent == null)
            {
                sb.AppendLine($"{label}: null");
                return;
            }

            try
            {
                // Use reflection to get persistent event count safely
                var getCountMethod = unityEvent.GetType().GetMethod("GetPersistentEventCount", BindingFlags.Instance | BindingFlags.Public);
                if (getCountMethod == null)
                {
                    sb.AppendLine($"{label}: Unable to get event count");
                    return;
                }

                int count = (int)getCountMethod.Invoke(unityEvent, null);
                sb.AppendLine($"{label}: {count} persistent listeners");

                var getTargetMethod = unityEvent.GetType().GetMethod("GetPersistentTarget", BindingFlags.Instance | BindingFlags.Public);
                var getMethodNameMethod = unityEvent.GetType().GetMethod("GetPersistentMethodName", BindingFlags.Instance | BindingFlags.Public);

                if (getTargetMethod == null || getMethodNameMethod == null)
                {
                    sb.AppendLine($"{label}: Unable to get listeners' targets or method names");
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    var target = getTargetMethod.Invoke(unityEvent, new object[] { i });
                    var methodName = getMethodNameMethod.Invoke(unityEvent, new object[] { i }) as string;

                    string targetName = target != null ? target.ToString() : "null";
                    methodName = methodName ?? "null";

                    sb.AppendLine($"      Listener {i}: Target={targetName}, Method={methodName}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{label}: Exception during reflection: {ex.Message}");
            }
        }
    }
}