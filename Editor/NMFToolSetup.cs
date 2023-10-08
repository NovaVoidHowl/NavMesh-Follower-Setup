using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  public class NMFToolSetup : EditorWindow
  {
    private List<PackageDependency> predefinedDependencies = new List<PackageDependency>
    {
      new PackageDependency(
        "uk.novavoidhowl.dev.common",
        "https://github.com/NovaVoidHowl/Common-Unity-Resources.git#1.0.0"
      ),
      // Add more dependencies as needed
    };

    [MenuItem("NVH/NavMeshFollower Setup/Tool Setup")]
    public static void ShowWindow()
    {
      NMFToolSetup window = GetWindow<NMFToolSetup>("NavMeshFollower Tool Setup");
      window.Show();
    }

    private void OnGUI()
    {
      GUIStyle windowHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 16,
        alignment = TextAnchor.MiddleCenter
      };
      EditorGUILayout.LabelField("NavMeshFollower Tool Setup", windowHeaderStyle);
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
