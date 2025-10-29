using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MultiImageTargetGraphics))]
public class MultiImageButton : Button
{
    private Graphic[] graphics;

    private MultiImageTargetGraphics targetGraphics;

    protected override void Start()
    {
        base.Start();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        //get the graphics, if it could not get the graphics, return here
        if (!GetGraphics())
            return;

        var targetColor =
            state == SelectionState.Disabled ? colors.disabledColor :
            state == SelectionState.Highlighted ? colors.highlightedColor :
            state == SelectionState.Normal ? colors.normalColor :
            state == SelectionState.Pressed ? colors.pressedColor :
            state == SelectionState.Selected ? colors.selectedColor : Color.white;

        foreach (var graphic in graphics)
            graphic.CrossFadeColor(targetColor, instant ? 0 : colors.fadeDuration, true, true);
    }

    private bool GetGraphics()
    {
        if (!targetGraphics) targetGraphics = GetComponent<MultiImageTargetGraphics>();
        graphics = targetGraphics?.GetTargetGraphics;
        return graphics != null && graphics.Length > 0;
    }
}