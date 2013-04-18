using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Midi;

namespace MidiRecorder.Components
{
    using Model;

    class MidiRecorder
    {
        private LinkedList<NoteMessage> notes;
        public LinkedList<NoteDetailed> processedNotes { get; private set; }
        private InputDevice input;
        //protected MidiRecorder output;
        private bool recording;
        private bool playing;

        public MidiRecorder(InputDevice inputDevice)
        {
            input = inputDevice;

            recording = false;
            playing = false;
        }

        #region Recording

        /// <summary>
        ///     Sets the MidiRecorder in reco
        /// </summary>
        /// <returns></returns>
        public bool StartRecording()
        {
            if (!recording)
            {
                notes = new LinkedList<NoteMessage>();
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
                recording = false;
                ProcessNotes();
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Processing
        /// <summary>
        ///     Turn NoteMessages into detailed and summarized format.
        /// </summary>
        private void ProcessNotes()
        {
            processedNotes = new LinkedList<NoteDetailed>();
            List<NoteMessage> noteList = notes.ToList<NoteMessage>();

            // Do not process empty recording
            if (0 == notes.Count)
            {
                return;
            }

            int index = 0;
            foreach (NoteMessage m in notes)
            {
                NoteOnMessage on;
                NoteOffMessage off;

                // Only use ON messages
                on = m as NoteOnMessage;
                if (null == on)
                { 
                    index++;
                    continue;
                }

                // Find the OFF mesage
                IEnumerator<NoteMessage> iter = noteList.GetRange(index + 1, noteList.Count - index - 1).GetEnumerator();
                while (iter.MoveNext())
                {
                    // Must be the next message with the same pitch
                    if (iter.Current.Pitch == on.Pitch)
                    {
                        off = iter.Current as NoteOffMessage;
                        processedNotes.AddLast(new NoteDetailed(on.Time, off.Time - on.Time, on.Pitch, on.Velocity));
                        break;
                    }
                }

                index++;
            }
        }

        /// <summary>
        ///     Allign recording with conducted beats.
        /// </summary>
        /// <param name="b">Conducted beats</param>
        /// <param name="ts">Time signature</param>
        public void Conduct(LinkedList<DateTime> b, TimeSignature ts)
        {
            // Convert to delta times
            List<float> beats = new List<float>();
            DateTime start = b.ElementAt(0);
            foreach (DateTime beat in b)
            {
                beats.Add((float)beat.Subtract(start).TotalSeconds);
            }

            foreach (NoteDetailed n in processedNotes)
            {
                int i = 0;
                float t = beats[i];

                // Find the beat for the note
                while ((n.GetStart() > t) && ((i + 1) < beats.Count))
                {
                    t = beats[++i];
                }
                float after = beats[i];
                float before = beats[--i];

                // Determine note placement
                float beat = ((i - 1) % ts.BeatsPerBar) + ((n.GetStart() - before) / (after - before));
                n.SetBeat((int)
                    (
                        (
                            (i - 0.9) // Makes sure that the beat ends up in the right bar (not subtract 1)
                            +
                            ((n.GetStart() - before) / (after - before))
                        ) 
                        / 
                        ts.BeatsPerBar
                    ), beat);
            }
        }

        /// <summary>
        ///     Move notes to the closest subdivision
        /// </summary>
        /// <param name="subdivision"></param>
        public void Quantize(int subdivision, TimeSignature ts)
        {
            int quantity = subdivision / (int)ts.Subdivision;
            float limit = 1.0f / quantity;

            foreach (NoteDetailed n in processedNotes)
            {
                float qBeat = (float)(Math.Floor(n.beat) + Math.Round(((float)(n.beat % 1) / limit)) * limit);
                float qDuration = (float)(Math.Floor(n.duration) + Math.Round(((float)(n.duration % 1) / limit)) * limit);

                // Quantized to invalid beat
                if (qBeat < 0) { qBeat = 0.0f; }
                if (qBeat >= ts.BeatsPerBar) { qBeat = 0.0f; }

                if (0.0 == qDuration) { qDuration = limit; }

                n.SetBeat(n.bar, qBeat);
                n.SetDuration(qDuration);
            }
        }
        #endregion
    }
}
