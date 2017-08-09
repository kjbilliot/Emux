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
        public static Dictionary<GameBoyPadButton, Key> InputMap = new Dictionary<GameBoyPadButton, Key>()
        {
            {Up,     Key.Up },
            {Down,   Key.Down },
            {Left,   Key.Left },
            {Right,  Key.Right },
            {A,      Key.X },
            {B,      Key.Z },
            {Start,  Key.Enter },
            {Select, Key.LeftShift },
        };

        public static ManualResetEvent IoBlockEvent = new ManualResetEvent(true);

        private GameBoy vm;
        private Thread ioThread;
        
        
           
        
        public IoManager(GameBoy vm)
        {
            this.vm = vm;
            ioThread = new Thread(Update);
            ioThread.SetApartmentState(ApartmentState.STA);
            ioThread.Start();
        }

        public void StopThread()
        {
            ioThread.Abort();
        }

        long t = 0;
        public void Update()
        {
            while (true)
            {
                IoBlockEvent.WaitOne();
                foreach (GameBoyPadButton k in InputMap.Keys)
                {
                    Console.WriteLine(t++);
                    if (IsKeyDown(InputMap[k])) vm.KeyPad.PressedButtons |= k;
                    else vm.KeyPad.PressedButtons &= ~k;
                }
            }
        }
    }
}
