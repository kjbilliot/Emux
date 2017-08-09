﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Emux.GameBoy.Cartridge;
using Emux.GameBoy.Cpu;
using Microsoft.Win32;

namespace Emux
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand StepCommand = new RoutedUICommand(
            "Execute next instruction.",
            "Step",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F10)
            }));

        public static readonly RoutedUICommand RunCommand = new RoutedUICommand(
            "Continue execution.",
            "Run",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F5)
            }));

        public static readonly RoutedUICommand BreakCommand = new RoutedUICommand(
            "Break execution.",
            "Break",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F5, ModifierKeys.Control)
            }));

        public static readonly RoutedUICommand SetBreakpointCommand = new RoutedUICommand(
            "Set an execution breakpoint to a memory address.",
            "Set Breakpoint",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F2)
            }));


        public static readonly RoutedUICommand ClearBreakpointsCommand = new RoutedUICommand(
            "Clear all breakpoints",
            "Clear all breakpoints",
            typeof(MainWindow));

        public static readonly RoutedUICommand ResetCommand = new RoutedUICommand(
            "Reset the GameBoy device.",
            "Reset",
            typeof(MainWindow));

        public static readonly RoutedUICommand VideoOutputCommand = new RoutedUICommand(
            "Open the video output window",
            "Video Output",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F11)
            }));

        public static readonly RoutedUICommand KeyPadCommand = new RoutedUICommand(
            "Open the virtual keypad window",
            "Keypad",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F12)
            }));

        public static readonly RoutedUICommand SourceCodeCommand = new RoutedUICommand(
            "View the source code of the program.",
            "Source Code",
            typeof(MainWindow));

        public static readonly RoutedUICommand AboutCommand = new RoutedUICommand(
            "View about details.",
            "About",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F1)
            }));

        public static readonly RoutedUICommand ControlMapperCommand = new RoutedUICommand(
            "Remap the controls.",
            "Control Mapper",
            typeof(MainWindow),
            new InputGestureCollection(new[]
            {
                new KeyGesture(Key.F8)
            }));

        private GameBoy.GameBoy _gameBoy;
        private readonly VideoWindow _videoWindow;
        private readonly KeypadWindow _keypadWindow;
        private readonly ControlMapper _controlMapperWindow;

        public MainWindow()
        {
            InitializeComponent();
            _videoWindow = new VideoWindow();
            _keypadWindow = new KeypadWindow();
            _controlMapperWindow = new ControlMapper();
        }

        public void RefreshView()
        {
            RegistersTextBox.Text = _gameBoy.Cpu.Registers + "\r\nTick: " + _gameBoy.Cpu.TickCount + "\r\n\r\n" +
                                    "LCDC: " + ((byte) _gameBoy.Gpu.Lcdc).ToString("X2") + "\r\n" +
                                    "STAT: " + ((byte) _gameBoy.Gpu.Stat).ToString("X2") + "\r\n" +
                                    "LY: " + _gameBoy.Gpu.LY.ToString("X2") + "\r\n" +
                                    "ScY: " + _gameBoy.Gpu.ScY.ToString("X2") + "\r\n" +
                                    "ScX: " + _gameBoy.Gpu.ScX.ToString("X2") + "\r\n" +
                                    "\r\n" +
                                    "TIMA: " + _gameBoy.Timer.Tima.ToString("X2") + "\r\n" +
                                    "TMA: " + _gameBoy.Timer.Tma.ToString("X2") + "\r\n" +
                                    "TAC: " + ((byte) _gameBoy.Timer.Tac).ToString("X2") + "\r\n";
                ;
            DisassemblyView.Items.Clear();
            var disassembler = new Z80Disassembler(_gameBoy.Memory);
            disassembler.Position = _gameBoy.Cpu.Registers.PC;
            for (int i = 0; i < 30 && disassembler.Position < 0xFFFF; i ++)
            {
                var instruction = disassembler.ReadNextInstruction();
                DisassemblyView.Items.Add(instruction.ToString());
            }
            
        }

        private void OpenCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                _gameBoy?.Terminate();
                _gameBoy = new GameBoy.GameBoy(new EmulatedCartridge(File.ReadAllBytes(dialog.FileName)));
                _gameBoy.Cpu.Paused += GameBoyOnPaused;
                _gameBoy.Gpu.VideoOutput = _videoWindow;

                _videoWindow.Device = _gameBoy;
                _videoWindow.Show();
                _keypadWindow.Device = _gameBoy;

                RefreshView();
            }
        }

        private void GameBoyOnPaused(object sender, EventArgs eventArgs)
        {
            Dispatcher.Invoke(RefreshView);
        }

        private void StepCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _gameBoy.Cpu.Step();
            RefreshView();
        }

        private void RunningOnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _gameBoy != null && _gameBoy.Cpu.Running;
        }

        private void PausingOnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _gameBoy != null && !_gameBoy.Cpu.Running;
        }

        private void GameBoyExistsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _gameBoy != null;
        }

        private void RunCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _gameBoy.Cpu.Run();
        }

        private void BreakCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _gameBoy.Cpu.Break();
        }

        private void SetBreakpointCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string text = "0000";
            bool repeat = true;
            while (repeat)
            {
                var dialog = new InputDialog
                {
                    Title = "Enter breakpoint address",
                    Text = text
                };
                var result = dialog.ShowDialog();
                repeat = result.HasValue && result.Value;
                if (repeat)
                {
                    ushort address;
                    text = dialog.Text;
                    repeat = !ushort.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address);

                    if (repeat)
                    {
                        MessageBox.Show("Please enter a valid hexadecimal number between 0000 and FFFF", "Emux",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _gameBoy.Cpu.Breakpoints.Add(address);
                    }
                }
            }
        }

        private void ResetCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _gameBoy.Reset();
            RefreshView();
        }

        private void ClearBreakpointsCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _gameBoy.Cpu.Breakpoints.Clear();
        }

        private void KeyPadCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _keypadWindow.Show();
        }

        private void SourceCodeCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(Properties.Settings.Default.Repository);
        }

        private void AboutCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void VideoOutputCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _videoWindow.Show();
        }

        private void ControlMapperCommandOnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _controlMapperWindow.OpenWindow();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _gameBoy?.Terminate();
            _videoWindow.Device = null;
            _keypadWindow.Device = null;
            _videoWindow.Close();
            _keypadWindow.Close();
        }

    }
}
