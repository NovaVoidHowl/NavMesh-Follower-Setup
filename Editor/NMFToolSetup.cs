// this whole file is only to be used in edit mode
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  [ExecuteInEditMode]
  public class NMFToolSetup : EditorWindow
  {
    private Vector2 scrollPosition;
    private const float MIN_WIDTH = 700f;
    private const float MIN_HEIGHT = 600f;

    List<string> appComponentsList = new List<string>()
    {
      "NavMeshFollowerSetupEditor-Checks.cs",
      "NavMeshFollowerSetupEditor-ComponentSetup-Core.cs",
      "NavMeshFollowerSetupEditor-ComponentSetup-FinalIK.cs",
      "NavMeshFollowerSetupEditor-ComponentSetup-LevelOne.cs",
      "NavMeshFollowerSetupEditor-ComponentSetup-LevelThree.cs",
      "NavMeshFollowerSetupEditor-ComponentSetup-LevelTwo.cs",
      "NavMeshFollowerSetupEditor-Constants.cs",
      "NavMeshFollowerSetupEditor-Lookups.cs",
      "NavMeshFollowerSetupEditor-UI-Gizmos.cs",
      "NavMeshFollowerSetupEditor-UI.cs",
      "NavMeshFollowerSetupEditor-Validate.cs",
      "NavMeshFollowerSetupEditor.cs"
    };

    private List<PackageDependency> predefinedDependencies = new List<PackageDependency>
    {
      new PackageDependency(
        "uk.novavoidhowl.dev.common",
        "https://github.com/NovaVoidHowl/Common-Unity-Resources.git#1.0.1"
      ),
      // Add more dependencies as needed
    };

    private void OnEnable()
    {
      minSize = new Vector2(MIN_WIDTH, position.height);
    }

    [MenuItem("NVH/NavMeshFollower Setup/Tool Setup")]
    public static void ShowWindow()
    {
      Rect rect = new Rect(0, 0, 800, 600);
      NMFToolSetup window = (NMFToolSetup)
        EditorWindow.GetWindowWithRect(typeof(NMFToolSetup), rect, true, "NavMeshFollower Tool Setup");
      window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
    }

    private void OnGUI()
    {
      string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup
      );

      scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

      EditorGUILayout.Space(10);

      //show a warning box if there are any mismatches
      if (CheckVersionsMismatch())
      {
        EditorGUILayout.HelpBox(
          "There are version mismatches in the dependencies. Please update/install the dependencies.",
          MessageType.Error
        );
        EditorGUILayout.Space(10);
      }

      if (predefinedDependencies.Count == 0)
      {
        EditorGUILayout.HelpBox("No dependencies found", MessageType.Info);
        return;
      }
      else
      {
        EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);
      }
      foreach (var dependency in predefinedDependencies)
      {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical("box");

        // Display only the part after the '#' character in the version
        string displayedVersion = dependency.Version.Contains("#")
          ? dependency.Version.Split('#')[1]
          : dependency.Version;
        string displayedInstalledVersion = dependency.InstalledVersion.Contains("#")
          ? dependency.InstalledVersion.Split('#')[1]
          : dependency.InstalledVersion;

        EditorGUILayout.LabelField(
          "Name                     : "
            + dependency.Name
            + "\nVersion                  : "
            + displayedVersion
            + "\nInstalled Version : "
            + displayedInstalledVersion,
          EditorStyles.wordWrappedLabel
        );

        EditorGUILayout.EndVertical();
      }

      EditorGUILayout.Space(10);

      if (GUILayout.Button("Update/Install Dependencies"))
      {
        ApplyDependencies();
        //refresh the window to show the updated versions
        Repaint();
      }

      EditorGUILayout.Space(20);

      // label for app components
      EditorGUILayout.LabelField("App Components", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);

      // start box
      EditorGUILayout.BeginVertical("box");

      bool blockInstall = false;

      if (!scriptingDefines.Contains("CVR_CCK_EXISTS"))
      {
        // CVR CCK is not installed
        //message to say that CVR CCK must be installed first
        EditorGUILayout.HelpBox("CVR CCK must be installed first.", MessageType.Info);
        blockInstall = true;
      }

      if (!scriptingDefines.Contains("KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS"))
      { // navmesh follower is not installed
        //message to say that navmesh follower must be installed first
        EditorGUILayout.HelpBox("NavMeshFollower CCK plugin must be installed first.", MessageType.Info);
        blockInstall = true;
      }

      if (!scriptingDefines.Contains("NVH_FIK_EXISTS"))
      { // final ik is not installed
        //message to say that final ik must be installed first
        EditorGUILayout.HelpBox("Final IK (or the stub) must be installed first.", MessageType.Info);
        blockInstall = true;
      }

      if (blockInstall)
      {
        //disable gui
        GUI.enabled = false;
      }

      // check if the folder 'Assets/NovaVoidHowl/NavMeshFollowerSetup exists
      if (AssetDatabase.IsValidFolder("Assets/NovaVoidHowl/NavMeshFollowerSetup"))
      { // folder exists
        EditorGUILayout.LabelField("NavMeshFollowerSetup core folder state: installed.", EditorStyles.wordWrappedLabel);
      }
      else
      { // folder does not exist
        EditorGUILayout.LabelField(
          "NavMeshFollowerSetup core folder state: not installed.",
          EditorStyles.wordWrappedLabel
        );
      }
      // check if the folder 'Assets/NovaVoidHowl/NavMeshFollowerSetup/Editor exists
      if (AssetDatabase.IsValidFolder("Assets/NovaVoidHowl/NavMeshFollowerSetup/Editor"))
      {
        // folder exists
      }
      else
      {
        // folder does not exist
      }

      // for each app component in appComponentsList, check if it exists
      foreach (string appComponent in appComponentsList)
      {
        EditorGUILayout.BeginHorizontal();

        string targetFile = "Assets/NovaVoidHowl/NavMeshFollowerSetup/Editor/" + appComponent;
        string sourceFile =
          "Packages/uk.novavoidhowl.dev.navmeshfollowersetup/Assets/Resources/appComponents/Editor/"
          + appComponent
          + ".source";
        bool notInstalled = false;

        string installedVersion = "?.?.?";
        string sourceVersion = "?.?.?";

        // check if target file exists
        if (AssetDatabase.LoadAssetAtPath(targetFile, typeof(Object)) != null)
        {
          // target file exists
          // get version of target file
          string firstLine = File.ReadLines(targetFile).FirstOrDefault();
          if (firstLine != null && firstLine.StartsWith("//") && firstLine.Contains("Version"))
          {
            string version = firstLine.Split(' ')[2];
            EditorGUILayout.LabelField("Installed: " + version, GUILayout.Width(110));
            installedVersion = version;
          }
          else
          {
            EditorGUILayout.LabelField("Installed: ?.?.?", GUILayout.Width(110));
          }
        }
        else
        {
          // target file does not exist
          EditorGUILayout.LabelField("Not installed", GUILayout.Width(110));
          notInstalled = true;
        }

        EditorGUILayout.LabelField(appComponent, EditorStyles.wordWrappedLabel);
        GUILayout.FlexibleSpace();

        // check if source file exists
        if (AssetDatabase.LoadAssetAtPath(sourceFile, typeof(Object)) != null)
        {
          // source file exists
          // get version of source file
          string firstLine = File.ReadLines(sourceFile).FirstOrDefault();
          if (firstLine != null && firstLine.StartsWith("//") && firstLine.Contains("Version"))
          {
            string version = firstLine.Split(' ')[2];
            EditorGUILayout.LabelField("Source: " + version, GUILayout.Width(110));
            sourceVersion = version;
          }
          else
          {
            EditorGUILayout.LabelField("Source: ?.?.?", GUILayout.Width(110));
          }

          string buttonText = "Update";
          // if source file version is higher than target file version
          if (sourceVersion.CompareTo(installedVersion) > 0)
          {
            // show update button
            buttonText = "Update";
          }
          // if source file version is lower than target file version
          if (sourceVersion.CompareTo(installedVersion) < 0)
          {
            // show install button
            buttonText = "Downgrade";
          }
          // if source file version is the same as target file version
          if (sourceVersion.CompareTo(installedVersion) == 0)
          {
            // show install button
            buttonText = "Reinstall";
          }
          // if target file does not exist
          if (notInstalled)
          {
            buttonText = "Install";
          }

          //button to update/install
          if (GUILayout.Button(buttonText, GUILayout.Width(80)))
          {
            // copy file from resource folder to assets folder

            if (!File.Exists(sourceFile))
            {
              Debug.LogError("Could not find file at path: " + sourceFile);
              return;
            }
            string directoryPath = Path.GetDirectoryName(targetFile);
            Directory.CreateDirectory(directoryPath);

            // remove existing file
            if (File.Exists(targetFile))
            {
              File.Delete(targetFile);
            }

            FileUtil.CopyFileOrDirectory(sourceFile, targetFile);
            AssetDatabase.Refresh();

            // Install button clicked
          }
        }
        else
        {
          // source file does not exist
          // this should not happen, show error
          EditorGUILayout.LabelField("ERROR: source file not found", GUILayout.Width(180));
        }
        EditorGUILayout.EndHorizontal();
      }
      //enable gui
      GUI.enabled = true;

      //end box
      EditorGUILayout.EndVertical();

      EditorGUILayout.Space(20);

      // label for 3rd party dependencies
      EditorGUILayout.LabelField("3rd Party Dependencies", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);

      // start box
      EditorGUILayout.BeginVertical("box");
      // label for CVR CCK section
      EditorGUILayout.LabelField("CVR CCK", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);
      // check if CVR CCK is installed, by checking the scripting define symbol 'CVR_CCK_EXISTS'
      if (scriptingDefines.Contains("CVR_CCK_EXISTS"))
      { // CVR CCK is installed
        EditorGUILayout.LabelField("CVR CCK state: installed.", EditorStyles.wordWrappedLabel);
      }
      else
      { // CVR CCK is not installed
        EditorGUILayout.LabelField("CVR CCK state: not installed.", EditorStyles.wordWrappedLabel);
      }
      EditorGUILayout.Space(2);
      // button to view CVR CCK website
      if (GUILayout.Button("View CVR CCK Website (free asset)"))
      {
        Application.OpenURL("https://docs.abinteractive.net/cck/setup/");
      }

      EditorGUILayout.Space(2);
      // end box
      EditorGUILayout.EndVertical();

      EditorGUILayout.Space(10);

      // start box
      EditorGUILayout.BeginVertical("box");
      // label for navmesh follower section
      EditorGUILayout.LabelField("NavMeshFollower CCK plugin", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);
      // check if navmesh follower is installed, by checking the scripting define symbol 'KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS'

      if (scriptingDefines.Contains("KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS"))
      { // navmesh follower is installed
        EditorGUILayout.LabelField("NavMeshFollower CCK plugin state: installed.", EditorStyles.wordWrappedLabel);
      }
      else
      { // navmesh follower is not installed
        EditorGUILayout.LabelField("NavMeshFollower CCK plugin state: not installed.", EditorStyles.wordWrappedLabel);
      }
      EditorGUILayout.Space(2);
      // button to view navmesh follower on github
      if (GUILayout.Button("View NavMeshFollower on Github (free asset)"))
      {
        Application.OpenURL("https://github.com/kafeijao/Kafe_CVR_CCKs/blob/master/NavMeshFollower/README.md");
      }

      EditorGUILayout.Space(2);
      // end box
      EditorGUILayout.EndVertical();

      EditorGUILayout.Space(10);

      // start box
      EditorGUILayout.BeginVertical("box");
      EditorGUILayout.Space(2);
      // label for final ik
      EditorGUILayout.LabelField("Final IK", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);
      //info box to stay that final ik (or the stub) is required for level 2 and 3
      EditorGUILayout.HelpBox(
        "Final IK (or the stub) is required for level 2 and 3. If you don't have it installed, you can still use level 1.",
        MessageType.Info
      );

      // check if final ik is installed, by checking the scripting define symbol 'NVH_FIK_EXISTS'
      if (scriptingDefines.Contains("NVH_FIK_EXISTS"))
      { // final ik is installed
        EditorGUILayout.LabelField("Final IK state: installed.", EditorStyles.wordWrappedLabel);
      }
      else
      { // final ik is not installed
        EditorGUILayout.LabelField("Final IK state: not installed.", EditorStyles.wordWrappedLabel);
      }
      // start horizontal
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.Space(2);
      // button to view final ik on the asset store
      if (GUILayout.Button("View Final IK on Asset Store (payed asset))"))
      {
        Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/final-ik-14290");
      }

      // button to view final ik stub on github
      if (GUILayout.Button("View Final IK Stub on Github (free asset))"))
      {
        Application.OpenURL("https://github.com/VRLabs/Final-IK-Stub");
      }
      //close horizontal
      EditorGUILayout.Space(2);
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space(2);
      // close box
      EditorGUILayout.EndVertical();

      EditorGUILayout.Space(20);
      EditorGUILayout.EndScrollView();
    }

    private void ApplyDependencies()
    {
      string manifestPath = "Packages/manifest.json";
      string manifestContent = File.ReadAllText(manifestPath);

      foreach (var dependency in predefinedDependencies)
      {
        manifestContent = AddOrUpdatePackage(manifestContent, dependency.Name, dependency.Version);
      }

      File.WriteAllText(manifestPath, manifestContent);
      AssetDatabase.Refresh();
    }

    private static string AddOrUpdatePackage(string manifestContent, string packageName, string packageVersion)
    {
      string pattern = $"\"{packageName}\": \"[^\"]+\"";
      string replacement = $"\"{packageName}\": \"{packageVersion}\"";

      if (Regex.IsMatch(manifestContent, pattern))
      {
        manifestContent = Regex.Replace(manifestContent, pattern, replacement);
      }
      else
      {
        manifestContent = manifestContent.Replace(
          "\"dependencies\": {",
          $"\"dependencies\": {{\n    \"{packageName}\": \"{packageVersion}\","
        );
      }

      return manifestContent;
    }

    private bool CheckVersionsMismatch()
    {
      string manifestPath = "Packages/manifest.json";
      string manifestContent = File.ReadAllText(manifestPath);

      foreach (var dependency in predefinedDependencies)
      {
        string pattern = $"\"{dependency.Name}\": \"([^\"]+)\"";
        Match match = Regex.Match(manifestContent, pattern);

        if (match.Success && match.Groups[1].Value != dependency.Version)
        {
          return true;
        }
      }

      return false;
    }

    private class PackageDependency
    {
      public string Name { get; }
      public string Version { get; }
      public string InstalledVersion { get; set; }

      public PackageDependency(string name, string version)
      {
        Name = name;
        Version = version;
        InstalledVersion = GetInstalledVersion(name);
      }

      private string GetInstalledVersion(string packageName)
      {
        string manifestPath = "Packages/manifest.json";
        string manifestContent = File.ReadAllText(manifestPath);
        string pattern = $"\"{packageName}\": \"([^\"]+)\"";

        Match match = Regex.Match(manifestContent, pattern);
        return match.Success ? match.Groups[1].Value : "Not installed";
      }
    }
  }
}
#endif
