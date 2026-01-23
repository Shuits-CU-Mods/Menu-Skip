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
            AccessTools.Field(typeof(PreRunScript), "didIntro").SetValue(__instance, true);
            return false;
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
}