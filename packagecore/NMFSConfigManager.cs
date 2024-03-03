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
  public class NMFSConfigManager
  {
    
    // Add menu item
    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Re-prompt config version choice", false, 0)]
    public static void ClearConfigChoice()
    {
      // clear the config choice seen so that the user is prompted again
      EditorPrefs.SetBool("NMFSConfigSelectUserOptionSeen", false);
    }

    // Validate menu item
    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Re-prompt config version choice", true)]
    public static bool ValidateClearConfigChoice()
    {
      // Show the menu item only if 'NMFSConfigSelectUserOptionSeen' is true
      return EditorPrefs.GetBool("NMFSConfigSelectUserOptionSeen", true);
    }

  }
}
#endif