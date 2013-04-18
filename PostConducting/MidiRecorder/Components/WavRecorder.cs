using System.IO;
using System.Media;
using System.Runtime.InteropServices;

namespace MidiRecorder.Components
{
    class WavRecorder
    {
        public string Recording { get; private set; }
        public SoundPlayer Sound { get; private set; }
        private bool isRecording = false;

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        public bool Start()
        {
            if (!isRecording)
            {
                mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
                mciSendString("record recsound", "", 0, 0);
                isRecording = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Stop()
        {
            if (isRecording)
            {
                Recording = Path.GetTempPath() + Path.GetRandomFileName() + ".wav";
                mciSendString("save recsound " + Recording, "", 0, 0);
                mciSendString("close recsound ", "", 0, 0);
                isRecording = false;
                Sound = new SoundPlayer(Recording);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
