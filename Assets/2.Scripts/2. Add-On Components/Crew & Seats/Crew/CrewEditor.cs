using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CustomEditor(typeof(CrewMember)), CanEditMultipleObjects]
public class CrewEditor : ModuleEditor
{
    SerializedProperty seats;

    protected override void OnEnable()
    {
        base.OnEnable();
        seats = serializedObject.FindProperty("seats");
    }
    static bool showSeats = true;
    static bool showModelViewer = true;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        CrewMember crew = (CrewMember)target;
        if (!crew.sofModular) return;

        if(crew.seats == null) crew.seats = new List<CrewSeat>();
        if (crew.seats.Count == 0) EditorGUILayout.HelpBox("You must assign a seat to this crewmember", MessageType.Warning);

        GUILayout.Space(15f);
        showSeats = EditorGUILayout.Foldout(showSeats, "Seats", true, EditorStyles.foldoutHeader);
        if (showSeats)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(seats);

            EditorGUI.indentLevel--;
        }
        showModelViewer = EditorGUILayout.Foldout(showModelViewer, "Model Viewer", true, EditorStyles.foldoutHeader);
        if (showModelViewer)
        {
            EditorGUI.indentLevel++;

            if (crew.GetComponentInChildren<CrewAnimator>() != null) GUI.enabled = false;
            if(GUILayout.Button("Show crewmember model"))
            {
                
                GameObject crewVisualPrefab = crew.aircraft ? crew.aircraft.card.faction.crewMemberVisualModel : StaticReferences.Instance.defaultAlliesCrewmember;
                GameObject crewVisual = Instantiate(crewVisualPrefab, crew.transform);
                if (crewVisual)
                {
                    crewVisual.GetComponent<CrewAnimator>().SetReferences();
                }
            }
            GUI.enabled = true;

            if (crew.seats.Count > 1) SeatIdTestPopup(crew);
            else crew.seatIdTest = 0;

            EditorGUI.indentLevel--;
        }


        serializedObject.ApplyModifiedProperties();
    }

    private void SeatIdTestPopup(CrewMember crew)
    {
        string[] names = new string[crew.seats.Count];
        int[] values = new int[crew.seats.Count];

        for (int i = 0; i < names.Length; i++)
        {
            CrewSeat seat = crew.seats[i];
            if (seat == null) names[i] = "NULL";
            else names[i] = seat.gameObject.activeInHierarchy ? seat.name : seat.name + " (UNACTIVE)";
            values[i] = i;
        }
        crew.seatIdTest = EditorGUILayout.IntPopup("Test Seat Animation", crew.seatIdTest, names, values);
    }


    [MenuItem("Tools/Prefabs/Add Temporary Helper Cube %t")] // Example: Hotkey Ctrl+T (or Cmd+T on Mac)
    private static void AddTemporaryHelper()
    {
        // Check if we are in Prefab Mode
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            Debug.LogWarning("Not in Prefab Mode. Cannot add temporary helper.");
            return;
        }

        // Create the temporary object (e.g., a simple cube)
        GameObject tempObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tempObj.name = "Temporary Helper (Not Saved)";

        // --- This is the crucial part ---
        // Mark it so it doesn't get saved with the scene or prefab
        tempObj.hideFlags = HideFlags.DontSaveInEditor;

        // Optional: Add a component to easily find and delete these later if needed
        tempObj.AddComponent<TemporaryObjectMarker>();

        // Optional: Position it conveniently (e.g., at the origin of the prefab stage)
        tempObj.transform.position = Vector3.zero; // Position relative to the prefab stage root

        // Select the new object in the hierarchy
        Selection.activeGameObject = tempObj;

        Debug.Log("Added temporary helper object. It will NOT be saved with the Prefab.", tempObj);
    }

    // Optional helper component to mark objects
    public class TemporaryObjectMarker : MonoBehaviour { }

    // Optional: Add a menu item to clean up helpers if needed (though DontSaveInEditor should handle it)
    [MenuItem("Tools/Prefabs/Remove Temporary Helpers")]
    private static void RemoveTemporaryHelpers()
    {
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null) return; // Only run in prefab mode

        TemporaryObjectMarker[] markers = prefabStage.scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<TemporaryObjectMarker>(true)).ToArray();

        if (markers.Length == 0)
        {
            Debug.Log("No temporary helpers found to remove.");
            return;
        }

        foreach (var marker in markers)
        {
            Undo.DestroyObjectImmediate(marker.gameObject); // Use Undo for safety in editor
        }
        Debug.Log($"Removed {markers.Length} temporary helper object(s).");
    }
}
#endif

