using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Windows.Input.Keyboard;
using static Emux.GameBoy.Input.GameBoyPadButton;
using System.Threading;

namespace Emux.GameBoy.Input
{
    public class IoManager
    {
        public static Dictionary<Key, GameBoyPadButton> InputMap = new Dictionary<Key, GameBoyPadButton>()
        {
            {Key.Up,        Up },
            {Key.Down,      Down },
            {Key.Left,      Left },
            {Key.Right,     Right },
            {Key.X,         A },
            {Key.Z,         B },
            {Key.Enter,     Start },
            {Key.LeftShift, Select }
        };

        private GameBoy vm;

        public IoManager(GameBoy vm)
        {
            this.vm = vm;
            Thread ioThread = new Thread(Update);
            ioThread.SetApartmentState(ApartmentState.STA);
            ioThread.Start();
        }

        public void Update()
        {
            while (true)
            {
                foreach (Key k in InputMap.Keys)
                {
                    if (IsKeyDown(k)) vm.KeyPad.PressedButtons |= InputMap[k];
                    else vm.KeyPad.PressedButtons &= ~InputMap[k];
                }
            }
        }
    }
}
