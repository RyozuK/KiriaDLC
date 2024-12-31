using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;

namespace Mod_KiriaDLC;

[HarmonyPatch]
public class KiriaEntries
{
    public const int CorpseID = 118;

    public static void OnStartCore()
    {
        var sources = Core.Instance.sources;
        string LoaderGuid = "dk.elinplugins.customdialogloader";
        
        var loader = BaseModManager.Instance.packages
            .FirstOrDefault(p => p.activated && p.id == LoaderGuid);
        if (loader is not null) {
            // use loader
            KiriaDLCPlugin.LogWarning("KiriaEntries::OnStartCore", "Using Custom Whatever Loader");
            return;
        }
        
        throw new Exception("Custom Whatever Loader not found! Please install or update CWL");
    }

}
