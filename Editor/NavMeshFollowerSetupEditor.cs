#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using uk.novavoidhowl.dev.navmeshfollowersetup;
#if KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS
using Kafe.NavMeshFollower.CCK;
#endif

//////////////////////////////////////// BUG: can't get at the ABI components, think its a load order issue
// #if CVR_CCK_EXISTS
// using ABIComponents = ABI.CCK.Components;
// #endif

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  [CustomEditor(typeof(NavMeshFollowerSetup))]
  public class NavMeshFollowerSetupEditor : Editor
  {
    private string GetPackageVersion()
    {
      string packagePath = "Packages/uk.novavoidhowl.dev.navmeshfollowersetup/package.json";
      string packageJson = File.ReadAllText(packagePath);
      Match match = Regex.Match(packageJson, @"""version""\s*:\s*""([^""]+)""");
      if (match.Success)
      {
        return match.Groups[1].Value;
      }
      return "Unknown";
    }

    private void OnEnable() { }

    public override void OnInspectorGUI()
    {
      // get object this script is attached to

      NavMeshFollowerSetup setupScript = (NavMeshFollowerSetup)target;
      GameObject scriptRootObject = setupScript.gameObject;

      // Show package version
      GUILayout.Label("Navmesh Follower Setup v" + GetPackageVersion(), EditorStyles.boldLabel);
      renderHorizontalSeparator();

      if (!isKafeCvrCckNavMeshFollowerExists())
      {
        EditorGUILayout.HelpBox(
          "KafeCVR CCK NavMeshFollower is not installed. Please install KafeCVR CCK NavMeshFollower to use this package.",
          MessageType.Warning
        );
        // add button to visit KafeCVR CCK NavMeshFollower page on GitHub
        if (GUILayout.Button("Visit KafeCVR CCK NavMeshFollower on GitHub"))
        {
          Application.OpenURL("https://github.com/kafeijao/Kafe_CVR_CCKs/tree/master/NavMeshFollower");
        }
        return;
      }

      if (!isCvrCckExists())
      {
        EditorGUILayout.HelpBox(
          "CVR CCK is not installed. Please install CVR CCK to use this package.",
          MessageType.Warning
        );
        // add button to visit CVR CCK page
        if (GUILayout.Button("Visit CVR CCK documentation page"))
        {
          Application.OpenURL("https://docs.abinteractive.net/cck/setup/");
        }
        return;
      }

      GUILayout.Space(10);

      // Show field for navMeshFollowerBody
      NavMeshFollowerSetup navMeshFollowerSetup = (NavMeshFollowerSetup)target;
      navMeshFollowerSetup.navMeshFollowerBody = (GameObject)
        EditorGUILayout.ObjectField(
          "Follower Body",
          navMeshFollowerSetup.navMeshFollowerBody,
          typeof(GameObject),
          true
        );

      GUILayout.Space(20);

      if (navMeshFollowerSetup.navMeshFollowerBody == null)
      {
        EditorGUILayout.HelpBox("Please select a Navmesh Follower Body.", MessageType.Info);
        // show button for each game object under the scriptRootObject that has an animator component,
        // to set navMeshFollowerBody, excluding items under the [NavMeshFollower] object
        Animator[] animatorComponents = scriptRootObject.GetComponentsInChildren<Animator>();
        foreach (Animator animatorComponent in animatorComponents)
        {
          if (animatorComponent.transform.parent.name != "[NavMeshFollower]")
          {
            if (GUILayout.Button("Set Follower Body to " + animatorComponent.gameObject.name))
            {
              navMeshFollowerSetup.navMeshFollowerBody = animatorComponent.gameObject;
              Debug.Log("Set Follower Body to " + animatorComponent.gameObject.name);

              // disable the other game objects from the list
              foreach (Animator animatorComponent2 in animatorComponents)
              {
                if (animatorComponent2.gameObject != animatorComponent.gameObject)
                {
                  animatorComponent2.gameObject.SetActive(false);
                }
              }
            }
          }
        }

        return;
      }

      // dropdown for followerLevel, levels 2 and 3 require FinalIK
      navMeshFollowerSetup.followerLevel = EditorGUILayout.Popup(
        "Follower Level",
        navMeshFollowerSetup.followerLevel,
        new string[] { "Un-Configured", "Simple Follower", "LookAt IK", "LookAt IK + VRIK" }
      );

      GUILayout.Space(20);

      // when followerLevel is 0, show info message
      if (navMeshFollowerSetup.followerLevel == 0)
      {
        EditorGUILayout.HelpBox("Please select a follower level.", MessageType.Info);
        return;
      }

      if (navMeshFollowerSetup.followerLevel > 0)
      {
        // show field for agent.speed
        navMeshFollowerSetup.agent_speed = EditorGUILayout.FloatField("Speed", navMeshFollowerSetup.agent_speed);
        // show field for agent.angularSpeed
        navMeshFollowerSetup.agent_angularSpeed = EditorGUILayout.FloatField(
          "Angular Speed",
          navMeshFollowerSetup.agent_angularSpeed
        );
        // show field for agent_acceleration
        navMeshFollowerSetup.agent_acceleration = EditorGUILayout.FloatField(
          "Acceleration",
          navMeshFollowerSetup.agent_acceleration
        );
        // show field for agent_stoppingDistance
        navMeshFollowerSetup.agent_stoppingDistance = EditorGUILayout.FloatField(
          "Stopping Distance",
          navMeshFollowerSetup.agent_stoppingDistance
        );
        // show field for agent_radius
        navMeshFollowerSetup.agent_radius = EditorGUILayout.FloatField(
          "Collision Radius",
          navMeshFollowerSetup.agent_radius
        );
        // show field for agent_height
        navMeshFollowerSetup.agent_height = EditorGUILayout.FloatField(
          "Collision Height",
          navMeshFollowerSetup.agent_height
        );
      }

      GUILayout.Space(20);

      // when followerLevel is 1
      if (navMeshFollowerSetup.followerLevel == 1)
      {
        if (levelOneFollowerCheckup(scriptRootObject))
        {
          // show button to reset follower agent settings
          if (GUILayout.Button("Reset Follower Agent Settings"))
          {
            navMeshFollowerSetup.agent_speed = 3f;
            navMeshFollowerSetup.agent_angularSpeed = 240f;
            navMeshFollowerSetup.agent_acceleration = 10f;
            navMeshFollowerSetup.agent_stoppingDistance = 1.5f;
            navMeshFollowerSetup.agent_radius = 0.15f;
            navMeshFollowerSetup.agent_height = 0.3f;
            Debug.Log("Reset Follower Agent Settings");
          }
          // show button to setup follower
          if (GUILayout.Button("Setup Follower"))
          {
            setupLevelOneFollower(navMeshFollowerSetup, navMeshFollowerSetup.navMeshFollowerBody, scriptRootObject);
          }
        }
        else
        {
          // checkup failed
          // show button to add NavMeshAgent object under the scriptRootObject
          if (GUILayout.Button("Add NavMeshAgent"))
          {
            // if [NavMeshAgent] object already exists (from level 2/3), remove it
            GameObject navMeshAgentLevelTwoOrThree = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;
            if (navMeshAgentLevelTwoOrThree != null)
            {
              DestroyImmediate(navMeshAgentLevelTwoOrThree);
            }

            // setup for level 1
            GameObject navMeshAgent = new GameObject("NavMeshAgent");
            navMeshAgent.transform.parent = scriptRootObject.transform;
            // zero out position and rotation
            navMeshAgent.transform.localPosition = Vector3.zero;
            navMeshAgent.transform.localRotation = Quaternion.identity;
            UnityEngine.AI.NavMeshAgent agent = navMeshAgent.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = navMeshFollowerSetup.agent_speed;
            agent.angularSpeed = navMeshFollowerSetup.agent_angularSpeed;
            agent.acceleration = navMeshFollowerSetup.agent_acceleration;
            agent.stoppingDistance = navMeshFollowerSetup.agent_stoppingDistance;
            agent.autoBraking = true; // locked to true
            agent.radius = navMeshFollowerSetup.agent_radius;
            agent.height = navMeshFollowerSetup.agent_height;
            agent.avoidancePriority = 50; // locked
            agent.autoTraverseOffMeshLink = true; // locked
            agent.autoRepath = true; // locked
            agent.areaMask = -1; // locked
            // disable navmeshagent component
            agent.enabled = false; // locked

            Debug.Log("Added NavMeshAgent");
          }

          return;
        }
      }

      // when followerLevel is 2 or 3, check if FinalIK is installed
      if (navMeshFollowerSetup.followerLevel > 1)
      {
        if (isFinalIKInstalled())
        {
          // FinalIK is installed, so can continue
        }
        else
        {
          // Show error message
          EditorGUILayout.HelpBox(
            "FinalIK is not installed. Please install FinalIK to use LookAt IK.",
            MessageType.Error
          );
          // Stop here
          return;
        }
      }

      if (navMeshFollowerSetup.followerLevel == 2)
      {
        if (levelTwoFollowerCheckup(scriptRootObject))
        {
          // passed test, show setup options
          setHeadBoneFromnavMeshFollowerBody(navMeshFollowerSetup);
          if (!isHeadBoneSet(navMeshFollowerSetup))
          {
            // if head bone is not set, stop here
            return;
          }

          GUILayout.Space(20);
          if (GUILayout.Button("Setup Follower"))
          {
            ////////////////////////////////////////////////////////////// TODO
            // setupLevelTwoFollower(navMeshFollowerSetup.navMeshFollowerBody);
          }
        }
        else
        {
          // show button to add the [NavMeshFollower] object under the scriptRootObject
          if (GUILayout.Button("Add NavMeshFollower Config Objects"))
          {
            // if NavMeshAgent object already exists (from level 1), remove it
            GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("NavMeshAgent")?.gameObject;
            if (navMeshFollowerConfigObject != null)
            {
              DestroyImmediate(navMeshFollowerConfigObject);
            }

            // setup for level 2
            GameObject prefabNMLevelTwo = Resources.Load<GameObject>("[NavMeshFollower]-level2");
            GameObject navMeshFollowerConfigObjectLevelTwo = Instantiate(prefabNMLevelTwo);
            navMeshFollowerConfigObjectLevelTwo.name = "[NavMeshFollower]";
            navMeshFollowerConfigObjectLevelTwo.transform.parent = scriptRootObject.transform;
            // zero out position and rotation
            navMeshFollowerConfigObjectLevelTwo.transform.localPosition = Vector3.zero;
            navMeshFollowerConfigObjectLevelTwo.transform.localRotation = Quaternion.identity;
            Debug.Log("Added NavMeshFollower Config Objects");
          }

          // if checkup failed stop here
          return;
        }
      }

      if (navMeshFollowerSetup.followerLevel == 3)
      {
        if (levelThreeFollowerCheckup(scriptRootObject))
        {
          // passed test, show setup options
          setHeadBoneFromnavMeshFollowerBody(navMeshFollowerSetup);

          if (!isHeadBoneSet(navMeshFollowerSetup))
          {
            // if head bone is not set, stop here
            return;
          }

          GUILayout.Space(20);
          if (GUILayout.Button("Setup Follower"))
          {
            ////////////////////////////////////////////////////////////// TODO
            // setupLevelThreeFollower(navMeshFollowerSetup.navMeshFollowerBody);
          }
        }
        else
        {
          // if checkup failed stop here
          return;
        }
      }
    }

    //////////////////////////////////////////
    //// Functions to checkup followers
    ///


    private bool levelOneFollowerCheckup(GameObject scriptRootObject)
    {
      //show error message if hasNavMeshAgentCore is false
      if (!hasNavMeshAgentCore(scriptRootObject))
      {
        EditorGUILayout.HelpBox("Navmesh Follower Config Object not found.", MessageType.Warning);
        return false;
      }
      else
      {
        return true;
      }
    }

    private bool levelTwoFollowerCheckup(GameObject scriptRootObject)
    {
      //show error message if hasNavMeshFollowerConfigObjectsCore is false
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        EditorGUILayout.HelpBox("Navmesh Follower Config Object not found.", MessageType.Warning);
        return false;
      }
      else
      {
        bool navMeshFollowerConfigObjectsCoreError = false;
        //check each of the required objects exist, if not show error message
        if (hasLookAtTargetSmooth(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LookAtTarget [Smooth] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasLookAtTargetRaw(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LookAtTarget [Raw] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasLookAtTargetRawOffset(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LookAtTarget [Raw] -> Offset sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasNavMeshAgentRawLevelTwo(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("NavMeshAgent [Raw] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }

        if (navMeshFollowerConfigObjectsCoreError)
        {
          // button to remove the [NavMeshFollower] object
          if (GUILayout.Button("Remove Corrupt NavMeshFollower Config Objects"))
          {
            // Get the [NavMeshFollower] object
            GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

            if (navMeshFollowerConfigObject != null)
            {
              // Destroy the [NavMeshFollower] object
              DestroyImmediate(navMeshFollowerConfigObject);
              Debug.Log("Removed NavMeshFollower Config Objects");
            }
          }
          return false;
        }
        else
        {
          return true;
        }
      }
    }

    private bool levelThreeFollowerCheckup(GameObject scriptRootObject)
    {
      //show error message if hasNavMeshFollowerConfigObjectsCore is false
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        EditorGUILayout.HelpBox("Navmesh Follower Config Object pack not found.", MessageType.Warning);
        return false;
      }
      else
      {
        bool navMeshFollowerConfigObjectsCoreError = false;
        //check each of the required objects exist, if not show error message
        if (hasLookAtTargetSmooth(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LookAtTarget [Smooth] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasLookAtTargetRaw(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LookAtTarget [Raw] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasLookAtTargetRawOffset(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LookAtTarget [Raw] -> Offset sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasNavMeshAgentRawLevelThree(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("NavMeshAgent [Raw] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIK(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKTargets(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Targets sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKScripts(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Scripts sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKTargetsLeftArmRaw(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Targets -> LeftArm [Raw] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKTargetsRightArmRaw(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Targets -> RightArm [Raw] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKTargetsLeftArmSmooth(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Targets -> LeftArm [Smooth] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKTargetsRightArmSmooth(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Targets -> RightArm [Smooth] sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKScriptsBothHands(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Scripts -> BothHands sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKScriptsLeftHand(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Scripts -> LeftHand sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasVRIKScriptsRightHand(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("VRIK -> Scripts -> RightHand sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasLeftHandAttachmentPoint(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("LeftHandAttachmentPoint sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }
        if (hasRightHandAttachmentPoint(scriptRootObject) == false)
        {
          EditorGUILayout.HelpBox("RightHandAttachmentPoint sub component not found.", MessageType.Error);
          navMeshFollowerConfigObjectsCoreError = true;
        }

        if (navMeshFollowerConfigObjectsCoreError)
        {
          // button to remove the [NavMeshFollower] object
          if (GUILayout.Button("Remove Corrupt NavMeshFollower Config Objects"))
          {
            // Get the [NavMeshFollower] object
            GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

            if (navMeshFollowerConfigObject != null)
            {
              // Destroy the [NavMeshFollower] object
              DestroyImmediate(navMeshFollowerConfigObject);
              Debug.Log("Removed NavMeshFollower Config Objects");
            }
          }
          return false;
        }
        else
        {
          return true;
        }
      }
    }

    //////////////////////////////////////////
    //// Functions to lookup components
    ////

    // Function to check if headbone is set
    private bool isHeadBoneSet(NavMeshFollowerSetup navMeshFollowerSetup)
    {
      if (navMeshFollowerSetup.headBone == null)
      {
        EditorGUILayout.HelpBox("Please select a Head Bone.", MessageType.Info);
        return false;
      }
      else
      {
        return true;
      }
    }

    // Function to return the head bone given the navMeshFollowerBody
    private Transform getHeadBone(GameObject navMeshFollowerBody)
    {
      // Output error if navMeshFollowerBody is null
      if (navMeshFollowerBody == null)
      {
        Debug.LogError("Navmesh Follower Body is null");
        return null;
      }
      else
      {
        Debug.Log("Navmesh Follower Body detected");
        // Check if navMeshFollowerBody has an animator component
        Animator animator = navMeshFollowerBody.GetComponent<Animator>();
        if (animator == null)
        {
          Debug.LogError("Navmesh Follower Body does not have Animator component");
          return null;
        }
        else
        {
          Debug.Log("Navmesh Follower Body has Animator component");
          // Get the head bone transform directly from the animator
          Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
          if (headBone != null)
          {
            Debug.Log("Navmesh Follower Body has Head Bone");
            return headBone;
          }
          else
          {
            Debug.LogError("Head Bone transform is null");
            return null;
          }
        }
      }
    }

    // Function to return the head bone given the navMeshFollowerBody
    private void setHeadBoneFromnavMeshFollowerBody(NavMeshFollowerSetup navMeshFollowerSetup)
    {
      // Show button to get headBone from navMeshFollowerBody
      if (GUILayout.Button("Get Head Bone"))
      {
        navMeshFollowerSetup.headBone = getHeadBone(navMeshFollowerSetup.navMeshFollowerBody);
      }

      // Show field for headBone readonly
      navMeshFollowerSetup.headBone = (Transform)
        EditorGUILayout.ObjectField("Head Bone", navMeshFollowerSetup.headBone, typeof(Transform), true);
    }

    //////////////////////////////////////////
    //// Functions to validate state of items
    ////


    // Function to check if the KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS scripting define symbol is set
    private bool isKafeCvrCckNavMeshFollowerExists()
    {
      return (
        PlayerSettings
          .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
          .Contains("KAFE_CVR_CCK_NAV_MESH_FOLLOWER_EXISTS")
      );
    }

    // Function to check if the CVR_CCK_EXISTS scripting define symbol is set
    private bool isCvrCckExists()
    {
      return (
        PlayerSettings
          .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
          .Contains("CVR_CCK_EXISTS")
      );
    }

    //Function to check if finalik is installed
    private bool isFinalIKInstalled()
    {
      return (Directory.Exists("Assets/Plugins/RootMotion"));
    }

    // Function to check if there is a child object with name "NavMeshAgent" for the game object on which this script is attached
    private bool hasNavMeshAgentCore(GameObject scriptRootObject)
    {
      // Check if scriptRootObject is null
      if (scriptRootObject == null)
      {
        Debug.LogError("[CRITICAL CORE ERROR] scriptRootObject is null");
        return false;
      }
      else
      {
        // Check if scriptRootObject has a child object with name "NavMeshAgent"
        Transform navMeshAgent = scriptRootObject.transform.Find("NavMeshAgent");
        if (navMeshAgent != null)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    // Function to check if there is a child object with name "[NavMeshFollower]" for the game object on which this script is attached
    private bool hasNavMeshFollowerConfigObjectsCore(GameObject scriptRootObject)
    {
      // Check if scriptRootObject is null
      if (scriptRootObject == null)
      {
        Debug.LogError("[CRITICAL CORE ERROR] scriptRootObject is null");
        return false;
      }
      else
      {
        // Check if scriptRootObject has a child object with name "[NavMeshFollower]"
        Transform navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]");
        if (navMeshFollowerConfigObject != null)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    // Function to check if there is a child object of the "[NavMeshFollower]" object with name "LookAtTarget [Smooth]"
    private bool hasLookAtTargetSmooth(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "LookAtTarget [Smooth]"
          GameObject lookAtTargetSmooth = navMeshFollowerConfigObject.transform
            .Find("LookAtTarget [Smooth]")
            ?.gameObject;
          if (lookAtTargetSmooth != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LookAtTarget [Smooth]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "[NavMeshFollower]" object with name "LookAtTarget [Raw]"
    private bool hasLookAtTargetRaw(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "LookAtTarget [Raw]"
          GameObject lookAtTargetRaw = navMeshFollowerConfigObject.transform.Find("LookAtTarget [Raw]")?.gameObject;
          if (lookAtTargetRaw != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LookAtTarget [Raw]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "[NavMeshFollower]" object with name "LookAtTarget [Raw] -> Offset"
    private bool hasLookAtTargetRawOffset(GameObject scriptRootObject)
    {
      if (!hasLookAtTargetRaw(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "LookAtTarget [Raw]" object
        GameObject lookAtTargetRaw = scriptRootObject.transform
          .Find("[NavMeshFollower]/LookAtTarget [Raw]")
          ?.gameObject;

        if (lookAtTargetRaw != null)
        {
          // Check if lookAtTargetRaw has a child object with name "LookAtTarget [Raw] -> Offset"
          GameObject lookAtTargetRawOffset = lookAtTargetRaw.transform.Find("LookAtTarget [Raw] -> Offset")?.gameObject;
          if (lookAtTargetRawOffset != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LookAtTarget [Raw] -> Offset" is not found, return false
        return false;
      }
    }

    // (Level 2) Function to check if there is a child object of the "[NavMeshFollower]" object with name "NavMeshAgent [Raw]"
    private bool hasNavMeshAgentRawLevelTwo(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "NavMeshAgent"
          GameObject navMeshAgentRaw = navMeshFollowerConfigObject.transform.Find("NavMeshAgent")?.gameObject;
          if (navMeshAgentRaw != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "NavMeshAgent [Raw]" is not found, return false
        return false;
      }
    }

    // (Level 3) Function to check if there is a child object of the "[NavMeshFollower]" object with name "NavMeshAgent [Raw]"
    private bool hasNavMeshAgentRawLevelThree(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "NavMeshAgent [Raw]"
          GameObject navMeshAgentRaw = navMeshFollowerConfigObject.transform.Find("NavMeshAgent [Raw]")?.gameObject;
          if (navMeshAgentRaw != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "NavMeshAgent [Raw]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "[NavMeshFollower]" object with name "VRIK"
    private bool hasVRIK(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "VRIK"
          GameObject vrik = navMeshFollowerConfigObject.transform.Find("VRIK")?.gameObject;
          if (vrik != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "VRIK" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "VRIK" object with name "Targets"
    private bool hasVRIKTargets(GameObject scriptRootObject)
    {
      if (!hasVRIK(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "VRIK" object
        GameObject vrik = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK")?.gameObject;

        if (vrik != null)
        {
          // Check if vrik has a child object with name "Targets"
          GameObject vrikTargets = vrik.transform.Find("Targets")?.gameObject;
          if (vrikTargets != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "Targets" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "VRIK" object with name "Scripts"
    private bool hasVRIKScripts(GameObject scriptRootObject)
    {
      if (!hasVRIK(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "VRIK" object
        GameObject vrik = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK")?.gameObject;

        if (vrik != null)
        {
          // Check if vrik has a child object with name "Scripts"
          GameObject vrikScripts = vrik.transform.Find("Scripts")?.gameObject;
          if (vrikScripts != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "Scripts" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Targets" object with name "LeftArm [Raw]"
    private bool hasVRIKTargetsLeftArmRaw(GameObject scriptRootObject)
    {
      if (!hasVRIKTargets(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Targets" object
        GameObject vrikTargets = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Targets")?.gameObject;

        if (vrikTargets != null)
        {
          // Check if vrikTargets has a child object with name "LeftArm [Raw]"
          GameObject vrikTargetsLeftArmRaw = vrikTargets.transform.Find("LeftArm [Raw]")?.gameObject;
          if (vrikTargetsLeftArmRaw != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LeftArm [Raw]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Targets" object with name "RightArm [Raw]"
    private bool hasVRIKTargetsRightArmRaw(GameObject scriptRootObject)
    {
      if (!hasVRIKTargets(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Targets" object
        GameObject vrikTargets = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Targets")?.gameObject;

        if (vrikTargets != null)
        {
          // Check if vrikTargets has a child object with name "RightArm [Raw]"
          GameObject vrikTargetsRightArmRaw = vrikTargets.transform.Find("RightArm [Raw]")?.gameObject;
          if (vrikTargetsRightArmRaw != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "RightArm [Raw]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Targets" object with name "LeftArm [Smooth]"
    private bool hasVRIKTargetsLeftArmSmooth(GameObject scriptRootObject)
    {
      if (!hasVRIKTargets(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Targets" object
        GameObject vrikTargets = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Targets")?.gameObject;

        if (vrikTargets != null)
        {
          // Check if vrikTargets has a child object with name "LeftArm [Smooth]"
          GameObject vrikTargetsLeftArmSmooth = vrikTargets.transform.Find("LeftArm [Smooth]")?.gameObject;
          if (vrikTargetsLeftArmSmooth != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LeftArm [Smooth]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Targets" object with name "RightArm [Smooth]"
    private bool hasVRIKTargetsRightArmSmooth(GameObject scriptRootObject)
    {
      if (!hasVRIKTargets(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Targets" object
        GameObject vrikTargets = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Targets")?.gameObject;

        if (vrikTargets != null)
        {
          // Check if vrikTargets has a child object with name "RightArm [Smooth]"
          GameObject vrikTargetsRightArmSmooth = vrikTargets.transform.Find("RightArm [Smooth]")?.gameObject;
          if (vrikTargetsRightArmSmooth != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "RightArm [Smooth]" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Scripts" object with name "BothHands"
    private bool hasVRIKScriptsBothHands(GameObject scriptRootObject)
    {
      if (!hasVRIKScripts(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Scripts" object
        GameObject vrikScripts = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Scripts")?.gameObject;

        if (vrikScripts != null)
        {
          // Check if vrikScripts has a child object with name "BothHands"
          GameObject vrikScriptsBothHands = vrikScripts.transform.Find("BothHands")?.gameObject;
          if (vrikScriptsBothHands != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "BothHands" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Scripts" object with name "LeftHand"
    private bool hasVRIKScriptsLeftHand(GameObject scriptRootObject)
    {
      if (!hasVRIKScripts(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Scripts" object
        GameObject vrikScripts = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Scripts")?.gameObject;

        if (vrikScripts != null)
        {
          // Check if vrikScripts has a child object with name "LeftHand"
          GameObject vrikScriptsLeftHand = vrikScripts.transform.Find("LeftHand")?.gameObject;
          if (vrikScriptsLeftHand != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LeftHand" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "Scripts" object with name "RightHand"
    private bool hasVRIKScriptsRightHand(GameObject scriptRootObject)
    {
      if (!hasVRIKScripts(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the "Scripts" object
        GameObject vrikScripts = scriptRootObject.transform.Find("[NavMeshFollower]/VRIK/Scripts")?.gameObject;

        if (vrikScripts != null)
        {
          // Check if vrikScripts has a child object with name "RightHand"
          GameObject vrikScriptsRightHand = vrikScripts.transform.Find("RightHand")?.gameObject;
          if (vrikScriptsRightHand != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "RightHand" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "[NavMeshFollower]" object with name "LeftHandAttachmentPoint"
    private bool hasLeftHandAttachmentPoint(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "LeftHandAttachmentPoint"
          GameObject leftHandAttachmentPoint = navMeshFollowerConfigObject.transform
            .Find("LeftHandAttachmentPoint")
            ?.gameObject;
          if (leftHandAttachmentPoint != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "LeftHandAttachmentPoint" is not found, return false
        return false;
      }
    }

    // Function to check if there is a child object of the "[NavMeshFollower]" object with name "RightHandAttachmentPoint"
    private bool hasRightHandAttachmentPoint(GameObject scriptRootObject)
    {
      if (!hasNavMeshFollowerConfigObjectsCore(scriptRootObject))
      {
        return false;
      }
      else
      {
        // Get the [NavMeshFollower] object
        GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("[NavMeshFollower]")?.gameObject;

        if (navMeshFollowerConfigObject != null)
        {
          // Check if navMeshFollowerConfigObjectCore has a child object with name "RightHandAttachmentPoint"
          GameObject rightHandAttachmentPoint = navMeshFollowerConfigObject.transform
            .Find("RightHandAttachmentPoint")
            ?.gameObject;
          if (rightHandAttachmentPoint != null)
          {
            return true;
          }
        }

        // If any of the objects are null or "RightHandAttachmentPoint" is not found, return false
        return false;
      }
    }

    //////////////////////////////////////////
    //// Functions to setup components
    ///

    // Function to setup level 1 follower
    private void setupLevelOneFollower(
      NavMeshFollowerSetup navMeshFollowerSetup,
      GameObject navMeshFollowerBody,
      GameObject scriptRootObject
    )
    {
      alignFollowerBody(navMeshFollowerBody);
      addFollowerBodyParentConstraint(navMeshFollowerBody, scriptRootObject);
      updateNavMeshAgentLevelOne(navMeshFollowerSetup, scriptRootObject);
      updateSpawnableLevelOne(scriptRootObject, navMeshFollowerSetup);
      updateFollowerInfoLevelOne(scriptRootObject, navMeshFollowerSetup);
    }

    // Function to setup spawnable (level 1 follower)
    private void updateSpawnableLevelOne(GameObject scriptRootObject, NavMeshFollowerSetup navMeshFollowerSetup) { }

    // Function to update follower info (level 1 follower)
    private void updateFollowerInfoLevelOne(GameObject scriptRootObject, NavMeshFollowerSetup navMeshFollowerSetup)
    {
      /////////////////////////////////////////////////////////////////// TODO:
      // Get the NavMeshAgent object under the scriptRootObject
      GameObject navMeshFollowerConfigObject = scriptRootObject.transform.Find("NavMeshAgent")?.gameObject;

      // Get the NavMeshFollowerInfo component

      FollowerInfo navMeshFollowerInfoComponent = scriptRootObject.GetComponent<FollowerInfo>();

      // Update the NavMeshFollowerInfo component
      navMeshFollowerInfoComponent.navMeshAgent =
        navMeshFollowerConfigObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
      navMeshFollowerInfoComponent.hasLookAt = false;
      navMeshFollowerInfoComponent.lookAtTargetTransform = null;
      navMeshFollowerInfoComponent.headTransform = null;
      navMeshFollowerInfoComponent.hasVRIK = false;
      navMeshFollowerInfoComponent.hasLeftArmIK = false;
      navMeshFollowerInfoComponent.vrikLeftArmTargetTransform = null;
      navMeshFollowerInfoComponent.leftHandAttachmentPoint = null;
      navMeshFollowerInfoComponent.hasRightArmIK = false;
      navMeshFollowerInfoComponent.vrikRightArmTargetTransform = null;
      navMeshFollowerInfoComponent.rightHandAttachmentPoint = null;
    }

    // Function to update navmesh agent (level 1 follower)
    private void updateNavMeshAgentLevelOne(NavMeshFollowerSetup navMeshFollowerSetup, GameObject scriptRootObject)
    {
      // Get the navmesh agent object
      GameObject navMeshAgent = scriptRootObject.transform.Find("NavMeshAgent")?.gameObject;
      navMeshAgent.transform.parent = scriptRootObject.transform;
      // zero out position and rotation
      navMeshAgent.transform.localPosition = Vector3.zero;
      navMeshAgent.transform.localRotation = Quaternion.identity;
      // get navmesh agent component
      UnityEngine.AI.NavMeshAgent agent = navMeshAgent.GetComponent<UnityEngine.AI.NavMeshAgent>();
      agent.speed = navMeshFollowerSetup.agent_speed;
      agent.angularSpeed = navMeshFollowerSetup.agent_angularSpeed;
      agent.acceleration = navMeshFollowerSetup.agent_acceleration;
      agent.stoppingDistance = navMeshFollowerSetup.agent_stoppingDistance;
      agent.autoBraking = true; // locked to true
      agent.radius = navMeshFollowerSetup.agent_radius;
      agent.height = navMeshFollowerSetup.agent_height;
      agent.avoidancePriority = 50; // locked
      agent.autoTraverseOffMeshLink = true; // locked
      agent.autoRepath = true; // locked
      agent.areaMask = -1; // locked
      // disable navmeshagent component
      agent.enabled = false; // locked

      Debug.Log("Updated NavMeshAgent");
    }

    // Function to align navMeshFollowerBody to 0,0,0 position relative to parent
    private void alignFollowerBody(GameObject navMeshFollowerBody)
    {
      // set navMeshFollowerBody to 0,0,0 position relative to parent
      navMeshFollowerBody.transform.localPosition = Vector3.zero;
    }

    // Function to add parent constraint to navMeshFollowerBody
    private void addFollowerBodyParentConstraint(GameObject navMeshFollowerBody, GameObject scriptRootObject)
    {
      // Check if navMeshFollowerBody has a parent constraint
      ParentConstraint parentConstraint = navMeshFollowerBody.GetComponent<ParentConstraint>();
      if (parentConstraint == null)
      {
        // Add parent constraint to navMeshFollowerBody
        parentConstraint = navMeshFollowerBody.AddComponent<ParentConstraint>();
        Debug.Log("added parent constraint to navMeshFollowerBody");
      }
      else
      {
        // Remove existing sources from parent constraint
        parentConstraint.RemoveSource(0);
      }

      // remove existing sources from parent constraint

      // Get the list of sources.
      SerializedObject serializedConstraint = new SerializedObject(parentConstraint);
      SerializedProperty sourcesProperty = serializedConstraint.FindProperty("m_Sources");

      // Clear all sources by removing them one by one.
      sourcesProperty.ClearArray();

      // Apply the changes to the constraint.
      serializedConstraint.ApplyModifiedProperties();

      // deactivate parent constraint
      parentConstraint.constraintActive = false;

      // Set parent constraint to follow navMeshFollowerBody
      ConstraintSource constraintSourceBody = new ConstraintSource();
      constraintSourceBody.sourceTransform = navMeshFollowerBody.transform;
      constraintSourceBody.weight = 1;

      // Set parent constraint to follow NavMeshAgent object under scriptRootObject (for level 1 follower)
      GameObject navMeshAgent = scriptRootObject.transform.Find("NavMeshAgent")?.gameObject;
      // Set parent constraint to follow "NavMeshAgent [Raw]" object under "[NavMeshFollower]" game object under scriptRootObject (for level 2 & 3 followers)
      if (navMeshAgent == null)
      {
        navMeshAgent = scriptRootObject.transform.Find("[NavMeshFollower]/NavMeshAgent [Raw]")?.gameObject;
      }
      ConstraintSource constraintSourceAgent = new ConstraintSource();
      constraintSourceAgent.sourceTransform = navMeshAgent.transform;
      constraintSourceAgent.weight = 0.25F;

      parentConstraint.AddSource(constraintSourceBody);
      parentConstraint.AddSource(constraintSourceAgent);

      parentConstraint.SetTranslationOffset(0, Vector3.zero);
      parentConstraint.SetRotationOffset(0, Vector3.zero);
      parentConstraint.constraintActive = true;
      Debug.Log("updating parent constraint on navMeshFollowerBody");
    }

    //////////////////////////////////////////
    //// Functions to render UI elements
    ////
    // Function to render a horizontal separator
    private void renderHorizontalSeparator()
    {
      GUILayout.Space(4);
      GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
      GUILayout.Space(4);
    }
  }
}
#endif
