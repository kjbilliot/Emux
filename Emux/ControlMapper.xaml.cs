using Emux.GameBoy.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ControlMapper()
        {
            InitializeComponent();
            keyButtons = new List<Button>()
            {
                keyUp, keyDown, keyLeft, keyRight, keyA, keyB, keyStart, keySelect
            };
            keyButtons.ForEach(btn => originalValues.Add(btn, btn.Content.ToString()));
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (!CanContainDuplicateBinds && CheckForDuplicates())
            {
                MessageBox.Show("You have conflicting binds. Please fix this before saving.");
            }
            else
            {
                
                List<Key> keyBindings = new List<Key>();
                keyButtons.ForEach(btn =>
                {
                    if (Enum.TryParse(btn.Content.ToString(), out Key key))
                    {
                        keyBindings.Add(key);                        
                    }
                });
                int counter = 0;
                foreach (GameBoyPadButton gbpb in IoManager.InputMap.Keys.ToList())
                {
                    IoManager.InputMap[gbpb] = keyBindings[counter++];
                }
                Hide();
            }
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
    }
}
