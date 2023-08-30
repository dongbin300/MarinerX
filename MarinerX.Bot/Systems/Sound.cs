using System;
using System.Windows.Media;

namespace MarinerX.Bot.Systems
{
    public class Sound
    {
        public static void Play(string fileName)
        {
            Play(fileName, 1);
        }

        public static void Play(string fileName, double volume)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(fileName));
            player.Volume = volume;
            player.Play();
        }
    }
}
