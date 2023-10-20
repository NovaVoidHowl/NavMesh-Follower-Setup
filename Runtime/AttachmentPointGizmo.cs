using UnityEngine;
using UnityEditor;

namespace uk.novavoidhowl.dev.navmeshfollowersetup
{
  [ExecuteInEditMode]
  public class AttachmentPointGizmo : MonoBehaviour
  {
    [SerializeField]
    public GameObject parent_navmesh_follower_setup;
    public bool showAdvancedOptions;

#if UNITY_EDITOR
    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
      DrawGizmo();
    }

    [ExecuteInEditMode]
    public void DrawGizmo()
    {
      NavMeshFollowerSetup navMeshFollowerSetup = parent_navmesh_follower_setup?.GetComponent<NavMeshFollowerSetup>();

      if (navMeshFollowerSetup == null)
      {
        return;
      }

      if (navMeshFollowerSetup.showLeftHandAttachmentPointGizmos && this is LeftHandAttachmentPointGizmo)
      {
        DrawHandle(transform.position, transform.rotation, gameObject.name, transform);
      }

      if (navMeshFollowerSetup.showRightHandAttachmentPointGizmos && this is RightHandAttachmentPointGizmo)
      {
        DrawHandle(transform.position, transform.rotation, gameObject.name, transform);
      }
    }

    // Function to draw the handle at the specified position and rotation with a title
    [ExecuteInEditMode]
    public void DrawHandle(Vector3 position, Quaternion rotation, string title, Transform transform)
    {
      Handles.color = Color.yellow;
      Matrix4x4 handleMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);
      using (var handleScope = new Handles.DrawingScope(handleMatrix))
      {
        Vector3 labelPosition = Vector3.up * 0.2f;
        Handles.Label(labelPosition, title);
        Vector3 newPosition = Handles.PositionHandle(Vector3.zero, Quaternion.identity);
        if (newPosition != Vector3.zero)
        {
          SerializedObject serializedObject = new SerializedObject(transform);
          SerializedProperty positionProperty = serializedObject.FindProperty("m_LocalPosition");

          Undo.RecordObject(transform, "Move Object");
          positionProperty.vector3Value += newPosition;
          serializedObject.ApplyModifiedProperties();
        }
      }
    }

#endif
  }
}
