
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Media;
using Android.Hardware.Input;

using SmartGlass.Common;
using SmartGlass.Nano.Consumer;

namespace SmartGlass.Nano.Droid
{
    [Activity(Label = "StreamActivity",
              ScreenOrientation = ScreenOrientation.Landscape)]
    public class StreamActivity
        : Activity, TextureView.ISurfaceTextureListener, InputManager.IInputDeviceListener
    {
        private const int INVALID_GAMEPAD_INDEX = -1;

        private bool setupRan = false;
        private TextureView _videoSurface;

        private int _current_device_id;
        private List<int> _connected_devices;
        private InputManager _inputManager;

        private string _hostName;
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;
        private MediaCoreConsumer _mcConsumer;
        private InputHandler _inputHandler;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Remove title bar
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.StreamLayout);

            _hostName = Intent.Extras.GetString("hostName");
            Toast.MakeText(this,
                           String.Format("Connecting to {0}...", _hostName),
                           ToastLength.Short)
                 .Show();

            // Create your application here
            _videoSurface = FindViewById<TextureView>(Resource.Id.tvVideoStream);
            _videoSurface.SurfaceTextureListener = this;

            _current_device_id = INVALID_GAMEPAD_INDEX;
            _connected_devices = new List<int>();
            _inputHandler = new InputHandler();

            _inputManager = (InputManager)GetSystemService(Context.InputService);
            _inputManager.RegisterInputDeviceListener(this, null);

            EnumerateGamepads();
        }

        protected override void OnStop()
        {
            base.OnStop();

            _videoSurface.Dispose();
            _smartGlassClient.Dispose();
            _nanoClient.Dispose();
            _mcConsumer.Dispose();
        }

        /*
         * Video / Texture surface
         */

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            if (setupRan)
                return;

            Task.Run(() => StartStream(surface));
            setupRan = true;
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return false;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        /*
         * Gamepad input
         */

        public int EnumerateGamepads()
        {
            int[] deviceIds = _inputManager.GetInputDeviceIds();
            foreach (int deviceId in deviceIds)
            {
                InputDevice dev = InputDevice.GetDevice(deviceId);
                if (IsGamepad(dev))
                {
                    if (!_connected_devices.Contains(deviceId))
                    {
                        _connected_devices.Add(deviceId);
                        if (_current_device_id == INVALID_GAMEPAD_INDEX)
                        {
                            _current_device_id = deviceId;
                        }
                    }
                }
            }
            return _connected_devices.Count;
        }

        private bool IsGamepad(InputDevice device)
        {
            if ((device.Sources & InputSourceType.Gamepad) == InputSourceType.Gamepad ||
               (device.Sources & InputSourceType.ClassJoystick) == InputSourceType.Joystick)
            {
                return true;
            }
            return false;
        }

        //Get the centered position for the joystick axis
        private float GetCenteredAxis(MotionEvent e, InputDevice device, Axis axis)
        {
            InputDevice.MotionRange range = device.GetMotionRange(axis, e.Source);
            if (range != null)
            {
                float flat = range.Flat;
                float value = e.GetAxisValue(axis);
                if (System.Math.Abs(value) > flat)
                    return value;
            }

            return 0;
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            InputDevice device = e.Device;
            if (device != null && device.Id == _current_device_id)
            {
                if (IsGamepad(device))
                {
                    /*
                    for (int i = 0; i < AxesMapping.size; i++)
                    {
                        axes[i] = GetCenteredAxis(e, device, AxesMapping.OrdinalValueAxis(i));
                    }
                    return true;
                    */
                    return true;
                }
            }
            return base.OnGenericMotionEvent(e);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            InputDevice device = e.Device;
            if (device != null && device.Id == _current_device_id)
            {
                if (IsGamepad(device))
                {
                    /*
                    int index = ButtonMapping.OrdinalValue(keyCode);
                    if (index >= 0)
                    {
                        buttons[index] = 1;
                    }
                    return true;
                    */
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            InputDevice device = e.Device;
            if (device != null && device.Id == _current_device_id)
            {
                if (IsGamepad(device))
                {
                    /*
                    int index = ButtonMapping.OrdinalValue(keyCode);
                    if (index >= 0)
                    {
                        buttons[index] = 0;
                    }
                    return true;
                    */
                    return true;
                }
            }
            return base.OnKeyUp(keyCode, e);
        }

        public void OnInputDeviceAdded(int deviceId)
        {
            InputDevice dev = _inputManager.GetInputDevice(deviceId);
            if (!IsGamepad(dev))
                return;

            else if (_current_device_id == INVALID_GAMEPAD_INDEX)
            {
                _current_device_id = deviceId;
                if (!_connected_devices.Contains(deviceId))
                    _connected_devices.Add(deviceId);
            }
        }

        public void OnInputDeviceChanged(int deviceId)
        {
            throw new NotImplementedException("OnInputDeviceChanged");
        }

        public void OnInputDeviceRemoved(int deviceId)
        {
            if (_connected_devices.Contains(deviceId))
            {
                _connected_devices.Remove(deviceId);
            }
            if (_current_device_id == deviceId)
            {
                _current_device_id = INVALID_GAMEPAD_INDEX;
            }
        }

        /*
         * Protocol
         */

        public async Task StartStream(SurfaceTexture surface)
        {
            System.Diagnostics.Debug.WriteLine($"Connecting to console...");

            _smartGlassClient = await SmartGlassClient.ConnectAsync(_hostName);

            // Get general gamestream configuration
            var config = GamestreamConfiguration.GetStandardConfig();

            /* Modify standard config, if desired */

            var broadcastChannel = _smartGlassClient.BroadcastChannel;
            var session = await broadcastChannel.StartGamestreamAsync(config);

            System.Diagnostics.Debug.WriteLine(
                $"Connecting to Nano, TCP: {session.TcpPort}, UDP: {session.UdpPort}");

            _nanoClient = new NanoClient(_hostName, session);

            // General Handshaking & Opening channels
            await _nanoClient.InitializeProtocolAsync();

            // Audio & Video client handshaking
            // Sets desired AV formats
            Packets.AudioFormat audioFormat = _nanoClient.AudioFormats[0];
            Packets.VideoFormat videoFormat = _nanoClient.VideoFormats[0];
            await _nanoClient.InitializeStreamAsync(audioFormat, videoFormat);

            // Start ChatAudio channel
            Packets.AudioFormat chatAudioFormat = new Packets.AudioFormat(1, 24000, AudioCodec.Opus);
            await _nanoClient.OpenChatAudioChannelAsync(chatAudioFormat);

            _mcConsumer = new MediaCoreConsumer(surface, audioFormat, videoFormat);
            _nanoClient.AddConsumer(_mcConsumer);

            // Tell console to start sending AV frames
            await _nanoClient.StartStreamAsync();

            // Start Controller input channel
            await _nanoClient.OpenInputChannelAsync(1280, 720);

            System.Diagnostics.Debug.WriteLine($"Nano connected and running.");
        }
    }
}
