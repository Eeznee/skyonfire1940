using UnityEngine;
using UnityEngine.UI;

public class MultiImageTargetGraphics : MonoBehaviour
{
    [SerializeField] private Graphic[] targetGraphics;
    public Graphic[] GetTargetGraphics => targetGraphics;
}