using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Midi;

namespace MidiRecorder.Components
{
    using Model;

    class MidiRecorder
    {
        protected LinkedList<NoteMessage> notes;
        protected InputDevice input;
        //protected MidiRecorder output;
        private bool recording;
        protected bool playing;

        public MidiRecorder(InputDevice inputDevice)
        {
            notes = new LinkedList<NoteMessage>();
            input = inputDevice;

            recording = false;
            playing = false;
        }

        #region Public methods

        /// <summary>
        ///     Sets the MidiRecorder in reco
        /// </summary>
        /// <returns></returns>
        public bool StartRecording()
        {
            if (!recording)
            {
                input.NoteOn += (msg) =>
                {
                    lock (this)
                    {
                        // Some devices do not send NoteOff signals correctly. This code translates those signals.
                        if (msg.Velocity == 0)
                        {
                            notes.AddLast(new NoteOffMessage(msg.Device, msg.Channel, msg.Pitch, 0, msg.Time));
                        }
                        else
                        {
                            notes.AddLast(msg);
                        }
                    }
                };
                // Some devices do not send NoteOff signals correctly
                input.NoteOff += (msg) =>
                {
                    lock (this)
                    {
                        notes.AddLast(msg);
                    }
                };
                input.Open();
                input.StartReceiving(null);
                recording = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool StopRecording()
        {
            if (recording)
            {
                input.StopReceiving();
                input.RemoveAllEventHandlers();
                input.Close();
                recording = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool StartPlayback()
        {
            if (!playing)
            {
                playing = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool StopPlayback()
        {
            if (playing)
            {
                playing = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
