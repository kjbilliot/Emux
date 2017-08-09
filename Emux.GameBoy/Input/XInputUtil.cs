using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emux.GameBoy.Input
{
    public class XInputUtil
    {
        public delegate void ControllerUpdate(State newState);
        public static ControllerUpdate UpdateEvent;
        private static volatile bool alreadyInitialized = false;
        public static volatile bool KeepPollingInput = true;
        private static Thread xInputPollThread = new Thread(PollInput);

        public static void StartThread()
        {
            if (alreadyInitialized) return;
            xInputPollThread.Start();
            alreadyInitialized = true;
        }

        private static State lastState;
        private static void PollInput()
        {
            while (KeepPollingInput)
            {
                Controller ctrl = FindFirstAvailableController();
                if (ctrl != null)
                {
                    State newState = ctrl.GetState();
                    if (lastState.PacketNumber != newState.PacketNumber)
                    {
                        UpdateEvent?.Invoke(newState);
                        lastState = newState;
                    }
                }
                Thread.Sleep(16);
            }
        }

        public static bool IsPressed(GamepadButtonFlags flag)
        {
            return lastState.Gamepad.Buttons.HasFlag(flag);
        }

        private static UserIndex[] indexes = new UserIndex[] {
            UserIndex.One, UserIndex.Two, UserIndex.Three, UserIndex.Four
        };
        private static Controller FindFirstAvailableController()
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                Controller testController = new Controller(indexes[i]);
                if (testController.IsConnected) return testController;
            }
            return null;
        }
    }
}
