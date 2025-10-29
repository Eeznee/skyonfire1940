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

    [InputControl(name = "primaryFire", layout = "Button", bit = 1, displayName = "Primary Fire")]
    [InputControl(name = "secondaryFire", layout = "Button", bit = 2, displayName = "Secondary Fire")]
    [InputControl(name = "dropBomb", layout = "Button", bit = 3, displayName = "Drop Bomb")]
    [InputControl(name = "fireRocket", layout = "Button", bit = 4, displayName = "Fire Rocket")]
    [InputControl(name = "reload", layout = "Button", bit = 5, displayName = "Reload")]
    [InputControl(name = "bailOut", layout = "Button", bit = 6, displayName = "Bail Out")]

    [InputControl(name = "brakes", layout = "Button", bit = 11, displayName = "Brakes")]
    [InputControl(name = "toggleEngines", layout = "Button", bit = 12, displayName = "Toggle Engines")]
    [InputControl(name = "toggleGear", layout = "Button", bit = 13, displayName = "Toggle Gear")]
    [InputControl(name = "toggleAirbrakes", layout = "Button", bit = 14, displayName = "Toggle Airbrakes")]
    [InputControl(name = "toggleCanopy", layout = "Button", bit = 15, displayName = "Toggle Canopy")]
    [InputControl(name = "toggleBombBay", layout = "Button", bit = 16, displayName = "Toggle Bomb Bay")]
    [InputControl(name = "flapsUp", layout = "Button", bit = 17, displayName = "Flaps Up")]
    [InputControl(name = "flapsDown", layout = "Button", bit = 18, displayName = "Flaps Down")]

    [InputControl(name = "resetCamera", layout = "Button", bit = 20, displayName = "Reset Camera")]
    [InputControl(name = "zoomIn", layout = "Button", bit = 21, displayName = "Zoom In")]
    [InputControl(name = "cancelPause", layout = "Button", bit = 22, displayName = "Cancel Pause")]
    [InputControl(name = "screenshot", layout = "Button", bit = 23, displayName = "Screenshot")]
    [InputControl(name = "hideUI", layout = "Button", bit = 24, displayName = "Hide UI")]


    [InputControl(name = "nextSquadron", layout = "Button", bit = 25, displayName = "Next Squadron")]
    [InputControl(name = "previousSquadron", layout = "Button", bit = 26, displayName = "Previous Squadron")]
    [InputControl(name = "nextWing", layout = "Button", bit = 27, displayName = "Next Wing")]
    [InputControl(name = "previousWing", layout = "Button", bit = 28, displayName = "Previous Wing")]
    [InputControl(name = "bombSight", layout = "Button", bit = 29, displayName = "Bomb Sight")]
    [InputControl(name = "switchSeat", layout = "Button", bit = 30, displayName = "Switch Seat")]

    public uint buttons;


    [InputControl(layout = "Axis", displayName = "Rudder")] public short rudder;
    [InputControl(layout = "Axis", displayName = "Second Rudder")] public short secondRudder;
    [InputControl(layout = "Axis", displayName = "Throttle")] public short throttle;

    [InputControl(layout = "Stick", displayName = "Main Stick")] public Vector2 mainStick;
    [InputControl(layout = "Stick", displayName = "Camera Horizontal Movement")] public Vector2 moveCamera;
    [InputControl(layout = "Axis", displayName = "Camera Vertical Movement")] public short cameraVertical;

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

    public ButtonControl primaryFire { get; private set; }
    public ButtonControl secondaryFire { get; private set; }
    public ButtonControl dropBomb { get; private set; }
    public ButtonControl fireRocket { get; private set; }
    public ButtonControl reload { get; private set; }
    public ButtonControl bailOut { get; private set; }

    public ButtonControl brakes { get; private set; }
    public ButtonControl toggleEngines { get; private set; }
    public ButtonControl toggleGear { get; private set; }
    public ButtonControl toggleAirbrakes { get; private set; }
    public ButtonControl toggleCanopy { get; private set; }
    public ButtonControl toggleBombBay { get; private set; }
    public ButtonControl flapsUp { get; private set; }
    public ButtonControl flapsDown { get; private set; }


    public ButtonControl resetCamera { get; private set; }
    public ButtonControl zoomIn { get; private set; }


    public ButtonControl cancelPause { get; private set; }
    public ButtonControl screenshot { get; private set; }
    public ButtonControl hideUI { get; private set; }


    public ButtonControl nextSquadron { get; private set; }
    public ButtonControl previousSquadron { get; private set; }
    public ButtonControl nextWing { get; private set; }
    public ButtonControl previousWing { get; private set; }
    public ButtonControl bombSight { get; private set; }
    public ButtonControl switchSeat { get; private set; }


    public AxisControl rudder { get; private set; }
    public AxisControl secondRudder { get; private set; }
    public AxisControl throttle { get; private set; }

    public StickControl mainStick { get; private set; }
    public StickControl moveCamera { get; private set; }
    public AxisControl cameraVertical { get; private set; }

    public Vector2Control cameraRotate { get; private set; }

    protected override void FinishSetup()
    {
        base.FinishSetup();

        primaryFire = GetChildControl<ButtonControl>("primaryFire");
        secondaryFire = GetChildControl<ButtonControl>("secondaryFire");
        dropBomb = GetChildControl<ButtonControl>("dropBomb");
        fireRocket = GetChildControl<ButtonControl>("fireRocket");
        reload = GetChildControl<ButtonControl>("reload");
        bailOut = GetChildControl<ButtonControl>("bailOut");

        brakes = GetChildControl<ButtonControl>("brakes");
        toggleEngines = GetChildControl<ButtonControl>("toggleEngines");
        toggleGear = GetChildControl<ButtonControl>("toggleGear");
        toggleAirbrakes = GetChildControl<ButtonControl>("toggleAirbrakes");
        toggleCanopy = GetChildControl<ButtonControl>("toggleCanopy");
        toggleBombBay = GetChildControl<ButtonControl>("toggleBombBay");
        flapsUp = GetChildControl<ButtonControl>("flapsUp");
        flapsDown = GetChildControl<ButtonControl>("flapsDown");

        resetCamera = GetChildControl<ButtonControl>("resetCamera");
        zoomIn = GetChildControl<ButtonControl>("zoomIn");

        cancelPause = GetChildControl<ButtonControl>("cancelPause");
        screenshot = GetChildControl<ButtonControl>("screenshot");
        hideUI = GetChildControl<ButtonControl>("hideUI");

        nextSquadron = GetChildControl<ButtonControl>("nextSquadron");
        previousSquadron = GetChildControl<ButtonControl>("previousSquadron");
        nextWing = GetChildControl<ButtonControl>("nextWing");
        previousWing = GetChildControl<ButtonControl>("previousWing");
        bombSight = GetChildControl<ButtonControl>("bombSight");
        switchSeat = GetChildControl<ButtonControl>("switchSeat");


        rudder = GetChildControl<AxisControl>("rudder");
        secondRudder = GetChildControl<AxisControl>("secondRudder");
        throttle = GetChildControl<AxisControl>("throttle");

        mainStick = GetChildControl<StickControl>("mainStick");
        moveCamera = GetChildControl<StickControl>("moveCamera");
        cameraVertical = GetChildControl<AxisControl>("cameraVertical");

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
    //[MenuItem("Tools/Custom Device Sample/Create Device")]
    private static void CreateDevice()
    {
        InputSystem.AddDevice(new InputDeviceDescription
        {
            interfaceName = "Custom",
            product = "Sample Product"
        });
    }

    //[MenuItem("Tools/Custom Device Sample/Remove Device")]
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
