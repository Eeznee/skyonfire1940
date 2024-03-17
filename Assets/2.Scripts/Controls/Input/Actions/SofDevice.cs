using System.Linq;
using System.Runtime.InteropServices;
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
    public FourCC format => new FourCC('S', 'O', 'F', 'D');

    [InputControl(name = "throttleBoost", layout = "Button", bit = 0, displayName = "Throttle Boost")]
    [InputControl(name = "primaryFire", layout = "Button", bit = 1, displayName = "Primary Fire")]
    [InputControl(name = "secondaryFire", layout = "Button", bit = 2, displayName = "Secondary Fire")]
    [InputControl(name = "resetCamera", layout = "Button", bit = 3, displayName = "Reset Camera")]
    [InputControl(name = "zoomIn", layout = "Button", bit = 4, displayName = "Zoom In")]
    [InputControl(name = "cancelPause", layout = "Button", bit = 5, displayName = "Cancel Pause")]
    public uint buttons;


    [InputControl(layout = "Axis", displayName = "Rudder")] public short rudder;
    [InputControl(layout = "Axis", displayName = "Second Rudder")] public short secondRudder;
    [InputControl(layout = "Axis", displayName = "Throttle")] public short throttle;

    [InputControl(layout = "Axis", displayName = "Zoom")] public short zoom;
    [InputControl(layout = "Axis", displayName = "Time Scale")] public short timeScale;
    [InputControl(layout = "Axis", displayName = "Camera Speed")] public short cameraSpeed;
    [InputControl(layout = "Axis", displayName = "Camera Vertical Movement")] public short cameraVertical;


    [InputControl(layout = "Stick", displayName = "Main Stick")] public Vector2 mainStick;
    [InputControl(layout = "Stick", displayName = "Camera Horizontal Movement")] public Vector2 moveCamera;

    [InputControl(layout = "Vector2", displayName = "Rotate Camera")] public Vector2 cameraRotate;

}


#if UNITY_EDITOR
[InitializeOnLoad] // Call static class constructor in editor.
#endif
[InputControlLayout(stateType = typeof(SofDeviceState))]
public class SofDevice : InputDevice, IInputUpdateCallbackReceiver
{
#if UNITY_EDITOR
    static SofDevice()
    {
        Initialize();
    }

#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        InputSystem.RegisterLayout<SofDevice>(
            matches: new InputDeviceMatcher()
                .WithInterface("Custom"));
    }

    public ButtonControl throttleBoost { get; private set; }
    public ButtonControl primaryFire { get; private set; }
    public ButtonControl secondaryFire { get; private set; }
    public ButtonControl resetCamera { get; private set; }
    public ButtonControl zoomIn { get; private set; }
    public ButtonControl cancelPause { get; private set; }

    public AxisControl rudder { get; private set; }
    public AxisControl secondRudder { get; private set; }
    public AxisControl throttle { get; private set; }
    public AxisControl zoom { get; private set; }
    public AxisControl timeScale { get; private set; }
    public AxisControl cameraSpeed { get; private set; }
    public AxisControl cameraVertical { get; private set; }

    public StickControl mainStick { get; private set; }
    public StickControl moveCamera { get; private set; }

    public Vector2Control cameraRotate { get; private set; }

    protected override void FinishSetup()
    {
        base.FinishSetup();

        throttleBoost = GetChildControl<ButtonControl>("throttleBoost");
        primaryFire = GetChildControl<ButtonControl>("primaryFire");
        secondaryFire = GetChildControl<ButtonControl>("secondaryFire");
        resetCamera = GetChildControl<ButtonControl>("resetCamera");
        zoomIn = GetChildControl<ButtonControl>("zoomIn");
        cancelPause = GetChildControl<ButtonControl>("cancelPause");

        rudder = GetChildControl<AxisControl>("rudder");
        secondRudder = GetChildControl<AxisControl>("secondRudder");
        throttle = GetChildControl<AxisControl>("throttle");
        zoom = GetChildControl<AxisControl>("zoom");
        timeScale = GetChildControl<AxisControl>("timeScale");
        cameraSpeed = GetChildControl<AxisControl>("cameraSpeed");
        cameraVertical = GetChildControl<AxisControl>("cameraVertical");

        mainStick = GetChildControl<StickControl>("mainStick");
        moveCamera = GetChildControl<StickControl>("moveCamera");

        cameraRotate = GetChildControl<Vector2Control>("cameraRotate");
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
    [MenuItem("Tools/Custom Device Sample/Create Device")]
    private static void CreateDevice()
    {
        InputSystem.AddDevice(new InputDeviceDescription
        {
            interfaceName = "Custom",
            product = "Sample Product"
        });
    }

    [MenuItem("Tools/Custom Device Sample/Remove Device")]
    private static void RemoveDevice()
    {
        var customDevice = InputSystem.devices.FirstOrDefault(x => x is SofDevice);
        if (customDevice != null)
            InputSystem.RemoveDevice(customDevice);
    }

#endif

    public void OnUpdate()
    {
        //var state = new SofDeviceState();
        //InputSystem.QueueStateEvent(this, state);
    }
}
