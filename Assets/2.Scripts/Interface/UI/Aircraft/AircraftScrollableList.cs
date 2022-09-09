using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AircraftScrollableList : MonoBehaviour
{
    RectTransform listTransform;
    ScrollRect scroller;
    AircraftSelection[] aircraftSelections;

    public AircraftSelection baseAircraftUI;
    public RectTransform scrollBar;
    public AircraftsList aircraftsList;
    public float extendedSize = 120f;

    public int selected = 0;
    bool extended = false;

    public AircraftCard SelectedCard { get { return aircraftsList.list[selected]; } }

    public void Select(int id)
    {
        if (aircraftsList.list[id].Available())
        {
            selected = id;
            Retract();
        }
    }

    public void Extend()
    {
        extended = scroller.enabled = true;
        listTransform.sizeDelta = new Vector2(listTransform.sizeDelta.x, extendedSize);

        aircraftSelections = new AircraftSelection[aircraftsList.list.Length];
        for (int i = 0; i < aircraftsList.list.Length; i++)
        {
            if (i != selected)
            {
                aircraftSelections[i] = Instantiate(baseAircraftUI, baseAircraftUI.transform.parent).GetComponent<AircraftSelection>();
                aircraftSelections[i].SendCard(aircraftsList.list[i]);
            }
        }
        aircraftSelections[selected] = baseAircraftUI;
    }
    public void Retract()
    {
        extended = scroller.enabled = false;
        Vector3 pos = listTransform.GetChild(0).transform.localPosition;
        pos.y = 0f;
        listTransform.GetChild(0).transform.localPosition = pos;
        listTransform.sizeDelta = new Vector2(listTransform.sizeDelta.x, 20f);

        for (int i = 0; i < aircraftSelections.Length; i++)
        {
            if (i != selected)
                Destroy(aircraftSelections[i].gameObject);
        }
        baseAircraftUI = aircraftSelections[selected];
    }

    public void Toggle()
    {
        if (extended) Retract();
        else Extend();
    }


    private void Start()
    {
        scroller = GetComponentInChildren<ScrollRect>();
        listTransform = scroller.GetComponent<RectTransform>();

        scrollBar.sizeDelta = new Vector2(scrollBar.sizeDelta.x, extendedSize-20f);
        aircraftsList.UpdateCards();
    }
}
