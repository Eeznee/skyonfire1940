using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif


public struct SofDeviceState : IInputStateTypeInfo
{
    public FourCC format => new FourCC('S', 'O', 'F');

    [InputControl(name = "timeScale", layout = "Axis", format = "SHRT", offset = 0)]
    public short timeScale;
    [InputControl(name = "zoom", layout = "Axis", format = "SHRT", offset = 0)]
    public short zoom;
}

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[InputControlLayout(displayName = "Sof Device", stateType = typeof(SofDeviceState))]
public class SofDevice : InputDevice, IInputUpdateCallbackReceiver
{
    public AxisControl timeScale { get; private set; }
    public AxisControl zoom { get; private set; }

    protected override void FinishSetup()
    {
        base.FinishSetup();

        timeScale = GetChildControl<AxisControl>("timeScale");
        zoom = GetChildControl<AxisControl>("zoom");
    }
    public void OnUpdate()
    {
        var state = new SofDeviceState();
        InputSystem.QueueStateEvent(this, state);
    }
#if UNITY_EDITOR
    static SofDevice() {Initialize();}
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        InputSystem.RegisterLayout<SofDevice>(
            matches: new InputDeviceMatcher()
                .WithInterface("Sof"));
    }

    public static SofDevice current { get; private set; }
    public override void MakeCurrent()
    {
        base.MakeCurrent();
        current = this;
    }
    protected override void OnRemoved()
    {
        base.OnRemoved();
        if (current == this)
            current = null;
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Sof Device/Create Device")]
    private static void CreateDevice()
    {
        InputSystem.AddDevice(new InputDeviceDescription
        {
            interfaceName = "Sof",
            product = "Sample Product"
        });
    }
    [MenuItem("Tools/Sof Device/Remove Device")]
    private static void RemoveDevice()
    {
        var customDevice = InputSystem.devices.FirstOrDefault(x => x is SofDevice);
        if (customDevice != null)
            InputSystem.RemoveDevice(customDevice);
    }

#endif
}

