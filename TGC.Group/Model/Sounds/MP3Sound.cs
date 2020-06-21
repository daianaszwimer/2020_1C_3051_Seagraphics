using Microsoft.DirectX.DirectInput;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGC.Core.Sound;
using TGC.Core.Text;

namespace TGC.Group.Model.Sounds
{
    class MP3Sound
    {
        private string route;
        private TgcMp3Player mp3Player;

        public MP3Sound(string _route)
        {
            route = _route;
            mp3Player = new TgcMp3Player();
            mp3Player.FileName = route;
        }

        public void play()
        {
            var currentState = mp3Player.getStatus();
            if (currentState == TgcMp3Player.States.Open)
            {
                //Reproducir MP3
                mp3Player.play(true);
            }
            if (currentState == TgcMp3Player.States.Stopped)
            {
                //Parar y reproducir MP3
                mp3Player.closeFile();
                mp3Player.play(true);
            }
            if (currentState == TgcMp3Player.States.Paused)
            {
                //Resumir la ejecución del MP3
                mp3Player.resume();
            }
        }

        public void pause()
        {
            var currentState = mp3Player.getStatus();
            if (currentState == TgcMp3Player.States.Playing)
            {
                //Pausar el MP3
                mp3Player.pause();
            }
        }

        public void stop()
        {
            var currentState = mp3Player.getStatus();
            if (currentState == TgcMp3Player.States.Playing)
            {
                //Parar el MP3
                mp3Player.stop();
            }
        }

        public void Dispose()
        {
            mp3Player.closeFile();
        }
    }
}
