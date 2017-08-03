using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emux.GameBoy.Graphics;
using Emux.GameBoy.Input;

namespace Emux
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : IVideoOutput
    {
        private readonly WriteableBitmap _bitmap = new WriteableBitmap(GameBoyGpu.FrameWidth, GameBoyGpu.FrameHeight, 96, 96, PixelFormats.Bgr24, null);

        private readonly Timer _frameRateTimer = new Timer(1000);

        private GameBoy.GameBoy _device;
        
        public VideoWindow()
        {
            InitializeComponent();
            _frameRateTimer.Start();
            _frameRateTimer.Elapsed += FrameRateTimerOnElapsed;
        }

        private void FrameRateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (Device != null)
            {
                lock (this)
                {
                    Dispatcher.Invoke(() => Title = string.Format("GameBoy Video Output ({0:0.00} FPS)",
                        _device.Cpu.FramesPerSecond));
                }
            }
        }

        public GameBoy.GameBoy Device
        {
            get { return _device; }
            set
            {
                if (_device != null)
                    _device.Gpu.VideoOutput = new EmptyVideoOutput();
                _device = value;
                if (value != null)
                    Device.Gpu.VideoOutput = this;
            }
        }

        public void RenderFrame(byte[] pixelData)
        {
            Dispatcher.Invoke(() =>
            {
                _bitmap.WritePixels(new Int32Rect(0, 0, 160, 144), pixelData, _bitmap.BackBufferStride, 0);
                VideoImage.Source = _bitmap;
            });
        }

        private void VideoWindowOnClosing(object sender, CancelEventArgs e)
        {
            lock (this)
            {
                _frameRateTimer.Stop();
                e.Cancel = Device != null;
                Hide();
            }
        }
    }
}
