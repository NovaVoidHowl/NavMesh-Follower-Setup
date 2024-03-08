// this whole file is only to be used in edit mode
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

using Constants = uk.novavoidhowl.dev.navmeshfollowersetup.packagecore.Constants;
using static uk.novavoidhowl.dev.navmeshfollowersetup.packagecore.CoreUtils;

namespace uk.novavoidhowl.dev.navmeshfollowersetup.symbolmgr
{
  [InitializeOnLoad]
  public class NMFS_FinalIK_Init
  {
    static NMFS_FinalIK_Init()
    {
      AddFinalIKSymbolIfNeeded();
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
      AddFinalIKSymbolIfNeeded();
    }

    private static void AddFinalIKSymbolIfNeeded()
    {
      // check if final ik is installed, by looking for a folder at path 'Assets/Plugins/RootMotion/FinalIK'
      if (AssetDatabase.IsValidFolder("Assets/Plugins/RootMotion/FinalIK") || AssetDatabase.IsValidFolder("Assets/VRLabs/Final IK Stub/RootMotion/FinalIK"))
      { // final ik is installed
        string symbol = "NVH_FIK_EXISTS";
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
          EditorUserBuildSettings.selectedBuildTargetGroup
        );
        if (!defines.Contains(symbol))
        {
          PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            (defines + ";" + symbol)
          );
          CoreLog("[FinalIK] Added NVH_FIK_EXISTS Scripting Symbol.");
        }
      }
      else
      {
        // final ik is not installed
        CoreLog("[FinalIK] Final IK is not installed, removing NVH_FIK_EXISTS Scripting Symbol.");
        string symbol = "NVH_FIK_EXISTS";
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
          EditorUserBuildSettings.selectedBuildTargetGroup
        );
        if (defines.Contains(symbol))
        {
          defines = defines.Replace(symbol, "");
          PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
          CoreLog("[FinalIK] Removed NVH_FIK_EXISTS Scripting Symbol.");
        }
      }
    }
  }
}
#endif
