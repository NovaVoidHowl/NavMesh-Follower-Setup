#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  [ExecuteInEditMode]
  public class NavMeshFollowerSetup : MonoBehaviour
  {
    public GameObject navMeshFollowerBody;
    public Transform headBone;
    public int followerLevel = 0;

    // level 0 = un-configured
    // level 1 = simple follower
    // level 2 = follower with LookAt IK
    // level 3 = follower with LookAt IK and VRIK for arms

    public bool versionsShow = false;
    public bool animatorSectionShow = false;

    // Mod Supported variable enable/disable
    //
    // MovementY - Value between -1.0 and 1.0 depending on the velocity forward. It will be 0.0 when not moving.
    // MovementX - Value between -1.0 and 1.0 depending on the velocity sideways. It will be 0.0 when not moving.
    // Grounded - This will be 0.0 if the Follower is jumping across a nav mesh link, 1.0 otherwise.
    // Idle - This will be 0.0 if the Follower is busy doing something, 1.0 otherwise.
    // HasNavMeshFollowerMod - 0.0 when then Follower Spawner doesn't have the mod installed (or has an old version), 1.0 otherwise.
    // IsBakingNavMesh - This value will be 1.0 when the nav mesh is baking for this follower, 0.0 otherwise.
    // VRIK/LeftArm/Weight - This will be 1.0 when the follower is controlling the left arm with the VRIK script, 0.0 otherwise.
    // VRIK/RightArm/Weight - This will be 1.0 when the follower is controlling the right arm with the VRIK script, 0.0 otherwise.

    public bool MovementX_enabled = false;
    public bool MovementY_enabled = false;
    public bool Grounded_enabled = false;
    public bool Idle_enabled = false;
    public bool HasNavMeshFollowerMod_enabled = false;
    public bool IsBakingNavMesh_enabled = false;
    public bool VRIK_LeftArm_Weight_enabled = false;
    public bool VRIK_RightArm_Weight_enabled = false;
    public bool ModSupportedVariableSectionShow = false;

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
