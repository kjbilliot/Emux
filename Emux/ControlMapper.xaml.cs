using Emux.GameBoy.Input;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Emux
{
    public partial class ControlMapper : Window
    {
        private const bool CanContainDuplicateBinds = false;
        private const string WaitingForKeyPress = "<press a key>";
        private Button activeSelection = null;
        private string activeSelectionOldData = "";
        private List<Button> keyButtons;
        private Dictionary<Button, string> originalValues = new Dictionary<Button, string>();
        private const string xinputTag = "(XInput) ";

        public ControlMapper()
        {
            InitializeComponent();
            keyButtons = new List<Button>() {
                keyUp, keyDown, keyLeft, keyRight, keyA, keyB, keyStart, keySelect
            };
            keyButtons.ForEach(btn => originalValues.Add(btn, btn.Content.ToString()));
            XInputUtil.StartThread();
            XInputUtil.UpdateEvent += XInputUpdateHandler;
        }

        public void XInputUpdateHandler(State controllerState)
        {
            if (activeSelection != null)
            {
                GamepadButtonFlags firstPressedButton = GetFirstPressedButton(controllerState);
                if (firstPressedButton == GamepadButtonFlags.None) return;
                Application.Current.Dispatcher.Invoke(delegate
                {
                    activeSelection.Content = xinputTag + firstPressedButton.ToString();
                    activeSelection = null;
                });
            }
        }
        
        private GamepadButtonFlags GetFirstPressedButton(State state)
        {
            GamepadButtonFlags keycode = GamepadButtonFlags.None;
            
            foreach (GamepadButtonFlags key in Enum.GetValues(typeof(GamepadButtonFlags)))
            {
                if (key == GamepadButtonFlags.None) continue;
                if (state.Gamepad.Buttons.HasFlag(key))
                {
                    keycode = key;
                    break;
                }
            }

            return keycode;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (!CanContainDuplicateBinds && CheckForDuplicates())
            {
                MessageBox.Show("You have conflicting binds. Please fix this before saving.");
            }
            else
            {
                IoManager.IoBlockEvent.Reset();
                List<GameBoyInputDefinition> keyBindings = new List<GameBoyInputDefinition>();
                keyButtons.ForEach(btn =>
                {
                    string btnContent = btn.Content.ToString();
                    if (btnContent.StartsWith(xinputTag))
                    {
                        btnContent = btnContent.Substring(xinputTag.Length);
                        if (Enum.TryParse(btnContent, out GamepadButtonFlags flag))
                            keyBindings.Add(new GameBoyInputDefinition(flag));
                    }
                    else if (Enum.TryParse(btnContent, out Key key))
                    { 
                        keyBindings.Add(new GameBoyInputDefinition(key));
                    }
                });
                int counter = 0;
                foreach (GameBoyPadButton button in IoManager.InputMap.Keys.ToList())
                {
                    IoManager.InputMap[button] = keyBindings[counter++];
                }
                Hide();
                Serialize();
                IoManager.IoBlockEvent.Set();
            }
        }
        public void Serialize()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("inputMap.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, IoManager.InputMap);
            stream.Close();
        }
        public void OpenWindow()
        {
            Show();
        }

        private bool CheckForDuplicates()
        {
            List<string> values = new List<string>();
            foreach (Button b in keyButtons)
            {
                string val = b.Content.ToString();
                if (values.Contains(val))
                    return true;
                values.Add(val);
            }
            return false;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            keyButtons.ForEach(btn => btn.Content = originalValues[btn]);
            Hide();
        }

        private void SetListen(object sender, RoutedEventArgs e)
        {
            if (activeSelection != null)
            {
                activeSelection.Content = activeSelectionOldData;
            }
            activeSelection = (Button)sender;
            activeSelectionOldData = activeSelection.Content.ToString();
            activeSelection.Content = WaitingForKeyPress;
        }

        private void ControlKeyUp(object sender, KeyEventArgs e)
        {
            if (activeSelection != null)
            {
                activeSelection.Content = e.Key.ToString();
                activeSelection = null;
            }
        }

        private void WndClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
