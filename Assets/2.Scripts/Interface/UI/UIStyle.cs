using UnityEngine;

[CreateAssetMenu(menuName = "Data/UI Style", fileName = "New UI Style")]
public class UIStyle : ScriptableObject
{
    public GameObject Standard;
    public GameObject Mobile;

    public UIManager SpawnUI()
    {
#if MOBILE_INPUT
        return Instantiate(Mobile).GetComponent<UIManager>();
#else
           return   Instantiate(Standard).GetComponent<UIManager>();
#endif
    }
}
