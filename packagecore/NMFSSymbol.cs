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
  public class NavMeshFollowerSetup_Init
  {
    static NavMeshFollowerSetup_Init()
    {
      AddSymbolIfNeeded();
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
      AddSymbolIfNeeded();
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
        CoreLog("[Core] Added NVH_NMFS_EXISTS Scripting Symbol.");
      }
      else
      {
        // if the symbol already exists, do nothing
      }
    }
  }
}
#endif
