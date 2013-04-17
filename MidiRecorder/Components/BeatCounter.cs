using System;
using System.Collections.Generic;

namespace MidiRecorder.Components
{
    using Model;

    /// <summary>
    ///     Keeps track of points in time
    /// </summary>
    public class BeatCounter
    {
        private LinkedList<DateTime> beats = new LinkedList<DateTime>();
        public TimeSignature TimeSignature { get; private set; }

        public BeatCounter()
            : this(new TimeSignature(4, Subdivision.QuarterNote))
        { }

        public BeatCounter(int beatsPerBar, int timeSubdivision)
            : this(new TimeSignature(beatsPerBar, (Subdivision)timeSubdivision))
        { }

        public BeatCounter(TimeSignature timeSignature)
        {
            TimeSignature = timeSignature;
        }

        /// <summary>
        ///     Gets the current BPM based on the last TEMPO_BEATS beats.
        /// </summary>
        public int Tempo
        {
            get
            {
                int tempo = 0;
                int activeBeats = TimeSignature.Rhythm.ActiveBeats;

                if (beats.Count >= activeBeats && activeBeats != 0)
                {
                    DateTime[] sample = new DateTime[activeBeats];
                    LinkedListNode<DateTime> last = beats.Last;
                    
                    // Quick extraction of last TEMPO_BEATS beats
                    for (int i = 0; i < activeBeats; i++)
                    {
                        sample[i] = last.Value;
                        last = last.Previous;
                    }

                    // Calculate average tempo
                    double total = 0;
                    for (int i = 1; i < activeBeats; i++)
                    {
                        // NOTE: Elements in sample are in reverse order
                        total += sample[i - 1].Subtract(sample[i]).TotalMilliseconds;
                    }
                    tempo = (int)((60 / ((total / 1000) / (activeBeats - 1))));
                }

                return tempo;
            }
        }

        /// <summary>
        ///     Adds the current time as a beat.
        /// </summary>
        public void AddBeat()
        {
            beats.AddLast(DateTime.Now);
        }
    }
}
