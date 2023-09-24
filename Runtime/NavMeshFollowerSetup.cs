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


    public float agent_speed = 3f;
    public float agent_angularSpeed = 240f;
    public float agent_acceleration = 10f;
    public float agent_stoppingDistance = 1.5f;
    public float agent_radius = 0.15f;
    public float agent_height = 0.3f;

    void Update() { }
  }
}
#endif
