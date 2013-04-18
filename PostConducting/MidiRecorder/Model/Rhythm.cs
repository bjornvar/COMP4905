using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MidiRecorder.Model
{
    public class Rhythm
    {
        public bool[] Beats { get; private set; }

        public Rhythm(TimeSignature timeSignature)
        {
            Beats = new bool[timeSignature.BeatsPerBar];

            for (int i = 0; i < Beats.Length; i++)
            {
                SetEmphasized(i, false);
            }
        }

        public void SetEmphasized(int beat, bool emphasized)
        {
            Beats[beat] = emphasized;
        }

        public int ActiveBeats
        {
            get
            {
                int result = 0;
                foreach (bool b in Beats)
                {
                    if (b) result++;
                }
                return result;
            }
        }
    }
}
