// this whole file is only to be used in edit mode
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  [InitializeOnLoad]
  public class NavMeshFollowerSetup_Init
  {
    static NavMeshFollowerSetup_Init()
    {
      AddSymbolIfNeeded();
      AddFinalIKSymbolIfNeeded();
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
      AddSymbolIfNeeded();
      AddFinalIKSymbolIfNeeded();
    }

    private static void AddSymbolIfNeeded()
    {
      string symbol = "NVH_NMFS_EXISTS";
      string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup
      );
      if (!defines.Contains(symbol))
      {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
          EditorUserBuildSettings.selectedBuildTargetGroup,
          (defines + ";" + symbol)
        );
        Debug.Log("[NavMeshFollowerSetup_Ini] Added NVH_NMFS_EXISTS Scripting Symbol.");
      }
      else
      {
        Debug.Log("[NavMeshFollowerSetup_Ini] NVH_NMFS_EXISTS Scripting Symbol already exists.");
      }
    }

    private static void AddFinalIKSymbolIfNeeded()
    {
      // check if final ik is installed, by looking for a folder at path 'Assets/Plugins/RootMotion/FinalIK'
      if (AssetDatabase.IsValidFolder("Assets/Plugins/RootMotion/FinalIK"))
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
          Debug.Log("[NavMeshFollowerSetup_Ini] Added NVH_FIK_EXISTS Scripting Symbol.");
        }
      }
      else
      {
        // final ik is not installed
        Debug.Log("[NavMeshFollowerSetup_Ini] Final IK is not installed, removing NVH_FIK_EXISTS Scripting Symbol.");
        string symbol = "NVH_FIK_EXISTS";
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
          EditorUserBuildSettings.selectedBuildTargetGroup
        );
        if (defines.Contains(symbol))
        {
          defines = defines.Replace(symbol, "");
          PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
          Debug.Log("[NavMeshFollowerSetup_Ini] Removed NVH_FIK_EXISTS Scripting Symbol.");
        }
      }
    }
  }
}
#endif
