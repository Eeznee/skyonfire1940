using UnityEngine;
using System.Collections;

public class SetSunLocation : MonoBehaviour
{
    [SerializeField]
    Sun sun;

    public void Start()
    {
        StartCoroutine(SetLocation());
    }

    public IEnumerator SetLocation()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("location disabled by user");
            yield break;
        }
        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Running)
        {
            var locInfo = Input.location.lastData;
            Debug.LogFormat("long={0} lat={1}", locInfo.longitude, locInfo.latitude);
            sun.SetLocation(locInfo.longitude, locInfo.latitude);
        }

        Input.location.Stop();
    }
}
