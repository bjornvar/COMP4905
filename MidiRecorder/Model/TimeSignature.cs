using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MidiRecorder.Model
{
    public class TimeSignature
    {
        public int BeatsPerBar { get; private set; }
        public Subdivision Subdivision  { get; private set; }
        public Rhythm Rhythm { get; private set; }

        public TimeSignature(int beatsPerBar, Subdivision timeSubdivision)
        {
            BeatsPerBar = beatsPerBar;
            Subdivision = timeSubdivision;
            Rhythm = new Rhythm(this);
        }
    }
}
