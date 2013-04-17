using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiRecorder.Model
{
    using Midi;

    class NoteDetailed
    {
        protected float timeStart;
        public float duration { get; private set; }
        public Pitch pitch { get; private set; }
        protected int velocity;
        public int bar { get; private set; }
        public float beat { get; private set; }

        public NoteDetailed(float s, float d, Pitch p, int v, float beat = 0, int bar = 0)
        {
            this.timeStart = s;
            this.duration = d;
            this.pitch = p;
            this.velocity = v;
            this.beat = beat;
            this.bar = bar;
        }

        public void SetBeat(int bar, float beat)
        {
            this.bar = bar;
            this.beat = beat;
        }

        public void SetDuration(float duration)
        {
            this.duration = duration;
        }

        public float GetStart()
        {
            return timeStart;
        }
    }
}
