using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AircraftsAmountSlider : MonoBehaviour
{
    public AircraftsDropdown aircraftsDropdown;
    private Slider slider;
    void Start()
    {
        slider = GetComponent<Slider>();
        aircraftsDropdown.dropdown.onValueChanged.AddListener(delegate { UpdateMaxValue(); });
        UpdateMaxValue();
    }
    void UpdateMaxValue()
    {
        slider.maxValue = aircraftsDropdown.SelectedCard.formation.aircraftPositions.Length;
        
    }
}
