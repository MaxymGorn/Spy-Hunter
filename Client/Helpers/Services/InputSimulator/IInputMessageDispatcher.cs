using Client.Helpers.Services.InputSimulator.Native;

namespace Client.Helpers.Services.InputSimulator
{
    internal interface IInputMessageDispatcher
    {
        void DispatchInput(INPUT[] inputs);
    }
}