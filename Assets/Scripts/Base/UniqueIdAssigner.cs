#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class UniqueIdAssigner : MonoBehaviour
{
    [ContextMenu("Assign Unique ID")]
    private void AssignId()
    {
#if UNITY_EDITOR
        string newId = GUID.Generate().ToString();
        var collectable = GetComponent<CollectableItemController>();
        if (collectable != null)
        {
            var so = new SerializedObject(collectable);
            so.FindProperty("UniqueId").stringValue = newId;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(collectable);
            Debug.Log($"Assigned new UniqueID: {newId} to {collectable.name}");
        }
#endif
    }
}
