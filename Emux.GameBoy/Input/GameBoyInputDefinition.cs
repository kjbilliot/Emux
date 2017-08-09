using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Emux.GameBoy.Input
{
    [Serializable]
    public class GameBoyInputDefinition
    {
        public bool IsPressed
        {
            get
            {
                return _key != Key.None ? Keyboard.IsKeyDown(_key) : XInputUtil.IsPressed(_flag);
            }
        }

        private Key _key;
        private GamepadButtonFlags _flag;

        public GameBoyInputDefinition(Key key) => _key = key;
        public GameBoyInputDefinition(GamepadButtonFlags flag) => _flag = flag;

        public override string ToString()
        {
            return _key != Key.None ? _key.ToString() : "(XInput) " + _flag.ToString();
        }
    }
}
