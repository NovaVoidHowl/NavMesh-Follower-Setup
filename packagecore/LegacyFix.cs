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
  public class NMFLegacyFix
  {
    static NMFLegacyFix()
    {
      EditorApplication.update += CheckLegacyFolder;
    }

    private static void CheckLegacyFolder()
    {
      // Remove the callback once we've done the check
      EditorApplication.update -= CheckLegacyFolder;

      string legacyAppComponentsPath = Constants.LEGACY_APP_COMPONENTS_PATH;
      if (AssetDatabase.IsValidFolder(legacyAppComponentsPath))
      {
        // Trigger the fix action
        FixLegacyFolder();
      }
    }

    // Add menu item
    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Fix Legacy Folder", false, 0)]
    public static void FixLegacyFolder()
    {
      FixLegacyAppComponentsStore();
    }

    // Validate the menu item
    [MenuItem("NVH/" + Constants.PROGRAM_DISPLAY_NAME + "/Fix Legacy Folder", true)]
    public static bool FixLegacyFolderValidate()
    {
      // Return true if the legacy folder exists, false otherwise
      return AssetDatabase.IsValidFolder(Constants.LEGACY_APP_COMPONENTS_PATH);
    }

    private static void FixLegacyAppComponentsStore()
    {
      string legacyAppComponentsPath = Constants.LEGACY_APP_COMPONENTS_PATH;
      if (AssetDatabase.IsValidFolder(legacyAppComponentsPath))
      {
        if (EditorUtility.DisplayDialog(
          "NMF Setup - Legacy App Components Folder Detected",
          "The legacy app components folder was detected.\n" +
          "This folder needs to be removed to prevent version conflicts\n\n" +
          "Please ensure any files you may have put in \n\n" +
          legacyAppComponentsPath + "\n\nare backed up before proceeding.\n\n" +
          "Remove folder now ?",
          "Yes",
          "No"
        ))
        {
          AssetDatabase.DeleteAsset(legacyAppComponentsPath);
          CoreLog("[Core] Removed Legacy App Components Folder.");
        }
        else
        {
          CoreLog("[Core] User chose not to remove Legacy App Components Folder.");
        }
      }
    }
  }
}
#endif