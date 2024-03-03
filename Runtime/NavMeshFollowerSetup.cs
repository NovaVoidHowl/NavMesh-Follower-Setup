#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  // Define data objects to hold the JSON config file data
  [System.Serializable]
  public class NmfConfig
  {
    public FollowerLevelData[] follower_level_data;
    public NmfConfigVariable[] variables;
    public NmfConfigVariable[] ik_variables;
  }

  [System.Serializable]
  public class FollowerLevelData
  {
    public string animator_file_path;
    public bool needs_FinalIK;
    public string display_name;
    public NmfSubSync[] sub_syncs;
    public string agent_path;
    public string look_at_path_raw;
    public string look_at_path_smooth;
    public string ik_script_path;
    public string ik_target_path;
    public string ik_left_arm_raw_path;
    public string ik_right_arm_raw_path;
    public string ik_left_arm_smooth_path;
    public string ik_right_arm_smooth_path;
    public string ik_root_path;
    public string left_hand_attach_point_path;
    public string right_hand_attach_point_path;
    public bool blank_avatar;
  }

  [System.Serializable]
  public class NmfSubSync
  {
    public string object_path;
    public string[] sync_flags;
  }

  [System.Serializable]
  public class NmfConfigVariable
  {
    public string name;
    public string default_value;
    public int[] mandatory_for_levels;
    public bool enabled = false;
  }

  [ExecuteInEditMode]
  public class NavMeshFollowerSetup : MonoBehaviour
  {
    public bool extendedInfoShow = false;

    public GameObject navMeshFollowerBody;
    public Transform headBone;

    public bool handIKsectionShow = false;
    public bool leftHandIKsectionShow = false;
    public bool leftHandIKenabled = false;
    public bool rightHandIKsectionShow = false;
    public bool rightHandIKenabled = false;
    public Transform leftHand;
    public Transform rightHand;

    public bool showAttachmentPointSection = false;
    public bool showLeftHandAttachmentPointGizmos = false;
    public bool showRightHandAttachmentPointGizmos = false;
    public bool leftHandAttachmentPointPositionAndRotationSectionShow = false;
    public bool rightHandAttachmentPointPositionAndRotationSectionShow = false;
    public float positionStep = 0.1f;

    public Avatar avatar;

    public int followerLevel = 0;

    // level 0 = un-configured
    // level 1 = simple follower
    // level 2 = follower with LookAt IK
    // level 3 = follower with LookAt IK and VRIK for arms

    public bool versionsShow = false;
    public bool animatorSectionShow = false;

    //need to store the values as which variables are supported by the mod may change
    public NmfConfigVariable[] nmfConfigVariables = new NmfConfigVariable[0];

    //need to store the values as which variables are supported by the mod may change
    public NmfConfigVariable[] nmfIKVariables = new NmfConfigVariable[0];

    //variable store for parameters that exist in the animator controller
    public NmfConfigVariable[] nmfAnimatorControllerVariables = new NmfConfigVariable[0];

    public bool ModSupportedVariableSectionShow = false;
    public bool CustomVariableSectionShow = false;
    public bool ModSupportedIKVariableSectionShow = false;

    public float agent_speed = 3f;
    public float agent_angularSpeed = 240f;
    public float agent_acceleration = 10f;
    public float agent_stoppingDistance = 1.5f;
    public float agent_radius = 0.15f;
    public float agent_height = 0.3f;
    public bool followerAgentSettingsSectionShow = false;

    public bool eyeIKenabled = false;
    public Transform leftEyeBone = null;
    public Transform rightEyeBone = null;
    public bool eyeIKsectionShow = false;
    public bool eyeExoticMode = false;
    public Transform[] exoticEyes = null;

    public bool spineIKsectionShow = false;
    public bool spineIKenabled = false;
    public Transform[] spineBones = null;

    void Update() { }
  }
}
#endif
