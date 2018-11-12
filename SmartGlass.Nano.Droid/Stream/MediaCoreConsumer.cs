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

        public MediaCoreConsumer(Android.Graphics.SurfaceTexture surface)
        {
            _video = new VideoHandler(surface);
            _audio = new AudioHandler();
            //TODO: Setup dynamically
            _video.SetupVideo(1280, 720, null, null);
            _audio.SetupAudio(48000, 2, null);
        }

        public void ConsumeAudioData(AudioData data)
        {
            _audio.ConsumeAudioData(data);
        }

        public void ConsumeAudioFormat(Packets.AudioFormat format)
        {
            _audio.ConsumeAudioFormat(format);
        }

        public void ConsumeVideoData(VideoData data)
        {
            _video.ConsumeVideoData(data);
        }

        public void ConsumeVideoFormat(VideoFormat format)
        {
            _video.ConsumeVideoFormat(format);
        }

        public void Dispose()
        {
            _video.Dispose();
            _audio.Dispose();
        }
    }
}
