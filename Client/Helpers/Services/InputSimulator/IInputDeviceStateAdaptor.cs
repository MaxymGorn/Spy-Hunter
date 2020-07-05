using Client.Helpers.Services.InputSimulator.Native;

namespace Client.Helpers.Services.InputSimulator
{
    public interface IInputDeviceStateAdaptor
    {
        bool IsKeyDown(VirtualKeyCode keyCode);

        bool IsKeyUp(VirtualKeyCode keyCode);

        bool IsHardwareKeyDown(VirtualKeyCode keyCode);

        bool IsHardwareKeyUp(VirtualKeyCode keyCode);

        bool IsTogglingKeyInEffect(VirtualKeyCode keyCode);
    }
}