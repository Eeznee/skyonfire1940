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

    const int textUpdate = 5;
    private int counter;
    
    public override bool IsActive()
    {
        if (Player.aircraft == null || UIManager.gameUI == GameUI.CamEditor) return true;
        bool playerTag = target.tag == Player.sofObj.tag;
        return target != Player.aircraft && PlayerPrefs.GetInt(playerTag ? "AlliedMarkers" : "EnnemiesMarkers", 1)  == 1;
    }

    public void Init(SofAircraft _target)
    {
        target = _target;
        rect = GetComponent<RectTransform>();
        marker = GetComponent<Image>();
        direction = arrow.GetComponent<Image>();
        infos = GetComponentInChildren<Text>();
        ResetProperties();
    }


    private void LateUpdate()
    {
        SofObject player = Player.sofObj;
        if (!player) return;
        bool playerTag = target.tag == player.tag;
        Color color = playerTag ? Color.blue : Color.red;
        if (player.aircraft && target.squadronId == player.aircraft.squadronId) color = Color.green;
        if (player == target.data.sofObject) color = Color.yellow;

        marker.color = direction.color = infos.color = color;

        //Rect
        Vector3 screenPoint = SofCamera.cam.WorldToScreenPoint(target.transform.position);
        float offset = rect.sizeDelta.x;
        rect.position = screenPoint;
        rect.position = new Vector3(rect.position.x, rect.position.y, 1f);
        Vector3 position = Mathv.ClampRectangle(rect.position, Screen.width, Screen.height);
        position.x = Mathf.Clamp(position.x,offset, Screen.width - offset);
        position.y = Mathf.Clamp(position.y, offset, Screen.height - offset);

        bool forward = screenPoint.z > 0f;
        if (!forward) position = new Vector3(Screen.width-position.x, Screen.height-position.y, 1f);
        marker.enabled = infos.enabled = position == rect.position && forward;
        direction.enabled = position != rect.position && !target.destroyed;

        arrow.position = rect.position = position;
        arrow.rotation = Quaternion.identity;
        Vector2 pos = new Vector2(rect.position.x - Screen.width / 2f, rect.position.y - Screen.height / 2f);
        arrow.Rotate(Vector3.forward, -Mathf.Atan(pos.x / pos.y) * Mathf.Rad2Deg + ((pos.y < 0) ? 180f : 0f));
        
        //Text
        if (counter == 0 && infos.enabled)
        {
            float distance = Mathf.Abs(screenPoint.z);
            string txt = target.card.completeName + "\n";
            txt += (target.difficulty * 100f).ToString("0") + "  " + (UnitsConverter.distance.Multiplier * distance).ToString("0.00") + " " + UnitsConverter.distance.Symbol + "\n";
            if (UIManager.gameUI == GameUI.CamEditor)
            {
                txt += "Sqdr " + (target.squadronId + 1);
                txt += target.placeInSquad == 0 ? " Leader" : " Wing " + target.placeInSquad;
            }
            else
            {
                txt += target.crew[0].Seat.Action;
            }
            infos.text = txt;
        }
        counter = (counter + 1) % textUpdate;


        if (target.destroyed) marker.color = infos.color = Color.black;
    }
}