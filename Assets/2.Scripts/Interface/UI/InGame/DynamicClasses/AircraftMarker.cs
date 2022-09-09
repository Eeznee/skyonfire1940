using System;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
public class AircraftMarker : DynamicUI
{
    private SofAircraft target;

    public RectTransform arrow;

    RectTransform rect;
    private Image direction;
    private Image marker;
    private Text infos;
    
    public override bool IsActive()
    {
        if (GameManager.player.aircraft == null || GameManager.gameUI == GameUI.CamerasEditor) return true;
        bool playerTag = target.tag == GameManager.player.sofObj.tag;
        return target != GameManager.player.aircraft && PlayerPrefs.GetInt(playerTag ? "AlliedMarkers" : "EnnemiesMarkers", 1)  == 1;
    }

    public void Init(SofAircraft _target)
    {
        target = _target;
        rect = GetComponent<RectTransform>();
        marker = GetComponent<Image>();
        direction = arrow.GetComponent<Image>();
        infos = GetComponentInChildren<Text>();
    }


    private void LateUpdate()
    {
        SofObject player = GameManager.ogPlayer.sofObj;
        bool playerTag = target.tag == player.tag;
        Color color = playerTag ? Color.blue : Color.red;
        if (player.data.aircraft && target.squadronId == player.data.aircraft.squadronId) color = Color.green;
        if (player == target.data.sofObject) color = Color.cyan;

        float distance = (PlayerCamera.instance.transform.position - target.transform.position).magnitude;

        infos.text = target.card.completeName + "\n";
        infos.text += (target.difficulty * 100f).ToString("0") + "  " +(UnitsConverter.distance.Multiplier * distance).ToString("0.00") + " " + UnitsConverter.distance.Symbol +  "\n";
        if (GameManager.gameUI == GameUI.CamerasEditor)
        {
            infos.text += "Sqdr " + (target.squadronId + 1);
            infos.text += target.placeInSquad == 0 ? " Leader" : " Wing " + target.placeInSquad;
        } else
        {
            infos.text += target.crew[0].Action();
        }

        marker.color = direction.color = infos.color = color;

        //Rect
        float offset = rect.sizeDelta.x;
        rect.position = PlayerCamera.instance.cam.WorldToScreenPoint(target.transform.position);
        rect.position = new Vector3(rect.position.x, rect.position.y, 1f);
        Vector3 position = Mathv.ClampRectangle(rect.position, Screen.width, Screen.height);
        position.x = Mathf.Clamp(position.x,offset, Screen.width - offset);
        position.y = Mathf.Clamp(position.y, offset, Screen.height - offset);

        bool forward = PlayerCamera.instance.cam.transform.InverseTransformPoint(target.transform.position).z > 0f;
        if (!forward) position = new Vector3(Screen.width-position.x, Screen.height-position.y, 1f);
        marker.enabled = infos.enabled = position == rect.position && forward;
        direction.enabled = position != rect.position && !target.destroyed;

        arrow.position = rect.position = position;
        arrow.rotation = Quaternion.identity;
        Vector2 pos = new Vector2(rect.position.x - Screen.width / 2f, rect.position.y - Screen.height / 2f);
        arrow.Rotate(Vector3.forward, -Mathf.Atan(pos.x / pos.y) * Mathf.Rad2Deg + ((pos.y < 0) ? 180f : 0f));

        if (target.destroyed) marker.color = infos.color = Color.black;
    }
}