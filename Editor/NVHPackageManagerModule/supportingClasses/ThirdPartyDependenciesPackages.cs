// editor only script to manage the dependencies
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Only need to change the following line, in the following files:
//
// DepManager.cs
// render1stPartyDeps.cs
// render3rdPartyDeps.cs
// renderAppComponents.cs
// renderCoreError.cs
// Validation.cs
// AppInternalPackages.cs
// DepManagerConfig.cs
// PrimaryDependenciesPackages.cs
// ThirdPartyDependenciesPackages.cs
//
// and the asmdef, to bind to project specific constants

using Constants = uk.novavoidhowl.dev.navmeshfollowersetup.packagecore.Constants;

namespace uk.novavoidhowl.dev.navmeshfollowersetup.nvhpmm
{
  [InitializeOnLoad]
  public class ThirdPartyDependenciesPackages
  {
    static ThirdPartyDependenciesPackages()
    {
      EditorApplication.delayCall += refreshThirdPartyDependencies;
    }

    public static void refreshThirdPartyDependencies()
    {
      EditorApplication.delayCall -= refreshThirdPartyDependencies;

      TextAsset jsonFile = Resources.Load<TextAsset>(
        Constants.PROGRAM_DISPLAY_NAME + "/nvhpmm/Dependencies/ThirdPartyDependencies"
      );

      if (jsonFile == null)
      {
        Debug.LogError("File not found: Assets/Resources/Dependencies/ThirdPartyDependencies.json");
        SharedData.ThirdPartyDependencies = new List<ThirdPartyPackageDependency>(); // Set to empty list
        return;
      }

      try
      {
        var jsonArray = Newtonsoft.Json.Linq.JArray.Parse(jsonFile.text);
      }
      catch (Newtonsoft.Json.JsonReaderException ex)
      {
        Debug.LogError("Invalid JSON: " + ex.Message);
        return;
      }

      SharedData.ThirdPartyDependencies = JsonConvert.DeserializeObject<List<ThirdPartyPackageDependency>>(
        jsonFile.text
      );
    }
  }
}
#endif
