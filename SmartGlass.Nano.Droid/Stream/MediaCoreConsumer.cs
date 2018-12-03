using System;
using System.Collections.Generic;
using Android.Media;
using Android.Views;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace SmartGlass.Nano.Droid
{
    public class MediaCoreConsumer : IConsumer
    {
        VideoHandler _video;
        AudioHandler _audio;

        public MediaCoreConsumer(Android.Graphics.SurfaceTexture surface,
            Packets.AudioFormat audioFormat, Packets.VideoFormat videoFormat)
        {
            _video = new VideoHandler(surface, videoFormat);
            _audio = new AudioHandler(audioFormat);
            //TODO: Setup dynamically
            _video.SetupVideo((int)videoFormat.Width, (int)videoFormat.Height, null, null);
            _audio.SetupAudio((int)audioFormat.SampleRate, (int)audioFormat.Channels, null);
        }

        public void ConsumeAudioData(object sender, AudioDataEventArgs args)
        {
            _audio.ConsumeAudioData(args.AudioData);
        }

        public void ConsumeVideoData(object sender, VideoDataEventArgs args)
        {
            _video.ConsumeVideoData(args.VideoData);
        }

        public void ConsumeInputFeedbackFrame(object sender, InputFrameEventArgs args)
        {
        }

        public void Dispose()
        {
            _video.Dispose();
            _audio.Dispose();
        }
    }
}
