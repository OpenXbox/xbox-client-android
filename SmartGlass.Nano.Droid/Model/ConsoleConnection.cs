using System;
using System.Threading.Tasks;

namespace SmartGlass.Nano.Droid.Model
{
    public static class ConsoleConnection
    {
        public static SmartGlass.SmartGlassClient SmartGlassClient { get; private set; }
        public static SmartGlass.Common.GamestreamSession GamestreamSession { get; private set; }
        public static SmartGlass.Nano.NanoClient NanoClient { get; private set; }
        public static SmartGlass.Common.GamestreamConfiguration GamestreamConfig { get; private set; }
        public static SmartGlass.Nano.Packets.AudioFormat AudioFormat { get; private set; }
        public static SmartGlass.Nano.Packets.VideoFormat VideoFormat { get; private set; }
        public static SmartGlass.Nano.Consumer.IConsumer Consumer { get; private set; }

        public static async Task Initialize(string hostName)
        {
            SmartGlassClient = await SmartGlassClient.ConnectAsync(hostName);

            // Get general gamestream configuration
            GamestreamConfig = Common.GamestreamConfiguration.GetStandardConfig();

            /* Modify standard config, if desired */

            var broadcastChannel = SmartGlassClient.BroadcastChannel;
            GamestreamSession = await broadcastChannel.StartGamestreamAsync(GamestreamConfig);

            NanoClient = new NanoClient(hostName, GamestreamSession);

            // General Handshaking & Opening channels
            await NanoClient.InitializeProtocolAsync();

            // Audio & Video client handshaking
            // Sets desired AV formats
            AudioFormat = NanoClient.AudioFormats[0];
            VideoFormat = NanoClient.VideoFormats[0];
            await NanoClient.InitializeStreamAsync(AudioFormat, VideoFormat);

            // Start ChatAudio channel
            Packets.AudioFormat chatAudioFormat = new Packets.AudioFormat(1, 24000, AudioCodec.Opus);
            await NanoClient.OpenChatAudioChannelAsync(chatAudioFormat);

            // Start Controller input channel
            await NanoClient.OpenInputChannelAsync(1280, 720);
        }

        public static async Task StartStreaming(SmartGlass.Nano.Consumer.IConsumer consumer)
        {
            Consumer = consumer;
            NanoClient.AddConsumer(Consumer);

            // Tell console to start sending AV frames
            await NanoClient.StartStreamAsync();
        }
    }
}