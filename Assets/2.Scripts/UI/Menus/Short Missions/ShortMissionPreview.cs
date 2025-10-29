using UnityEngine;
using UnityEngine.UI;
public class ShortMissionPreview : MonoBehaviour
{
    public ShortMissionSO shortMission;
    public ShortMissionPanel shortMissionPanel;

    public Text missionName;
    public Button startMission;

    void Start()
    {
        if (shortMission != null) LoadShortMission(shortMission);
    }
    public void LoadShortMission(ShortMissionSO _shortMission)
    {
        shortMission = _shortMission;

        startMission.onClick.RemoveAllListeners();
        startMission.onClick.AddListener(delegate { shortMissionPanel.LoadShortMission(shortMission); });

        missionName.text = shortMission.missionName;
    }
}

