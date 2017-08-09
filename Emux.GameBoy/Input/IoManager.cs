﻿using System;
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

        public static Dictionary<GameBoyPadButton, GameBoyInputDefinition> InputMap = new Dictionary<GameBoyPadButton, GameBoyInputDefinition>()
        {
            {Up,     new GameBoyInputDefinition(Key.Up)        },
            {Down,   new GameBoyInputDefinition(Key.Down)      },
            {Left,   new GameBoyInputDefinition(Key.Left)      },
            {Right,  new GameBoyInputDefinition(Key.Right)     },
            {A,      new GameBoyInputDefinition(Key.X)         },
            {B,      new GameBoyInputDefinition(Key.Z)         },
            {Start,  new GameBoyInputDefinition(Key.Enter)     },
            {Select, new GameBoyInputDefinition(Key.LeftShift) },
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
            XInputUtil.StartThread();
        }

        public void StopThread()
        {
            ioThread.Abort();
        }

        public void Update()
        {
            while (true)
            {
                IoBlockEvent.WaitOne();
                foreach (GameBoyPadButton k in InputMap.Keys)
                {
                    if (InputMap[k].IsPressed) vm.KeyPad.PressedButtons |= k;
                    else vm.KeyPad.PressedButtons &= ~k;
                }
            }
        }
    }
}
