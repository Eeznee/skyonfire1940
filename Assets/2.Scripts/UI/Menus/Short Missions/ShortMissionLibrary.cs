using UnityEngine;

public class ShortMissionLibrary : MonoBehaviour
{
    public ShortMissionPreview shortMissionPreviewTemplate;
    public ShortMissionSO[] shortMissionsLibrary;


    void Start()
    {
        shortMissionPreviewTemplate.gameObject.SetActive(true);

        foreach (ShortMissionSO s in shortMissionsLibrary)
        {
            ShortMissionPreview preview = Instantiate(shortMissionPreviewTemplate, shortMissionPreviewTemplate.transform.parent);
            preview.LoadShortMission(s);
        }

        shortMissionPreviewTemplate.gameObject.SetActive(false);
    }
}
