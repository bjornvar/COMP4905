using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Midi;

namespace MidiRecorder.Components
{
    class SplitPointGenerator
    {
        private float left;
        private float right;
        private int keys;
        private Pitch low;
        private Pitch high;

        // Mapping from Time to Position
        private Dictionary<double, Pitch> splitPointLog;
        private DateTime start;

        public SplitPointGenerator(float leftEnd, float rightEnd, int numKeys, Pitch lowest, Pitch highest)
        {
            this.left = leftEnd;
            this.right = rightEnd;
            this.keys = numKeys;
            this.low = lowest;
            this.high = highest;
        }

        public Pitch GetSplitPoint(double time)
        {
            if (null == splitPointLog)
            {
                return Pitch.A0;
            }
            else
            {
                double key = 0;

                IEnumerable<double> keys = splitPointLog.Keys.AsEnumerable<double>();
                for (int i = 0; i < keys.Count(); i++)
                {
                    if (keys.ElementAt(i) > time)
                    {
                        key = keys.ElementAt(i - 1);
                        break;
                    }
                }

                Pitch result;
                splitPointLog.TryGetValue(key, out result);

                return result;
            }
        }

        //
        private Pitch Translate(float xPosition)
        {
            return (Pitch)
                (
                    (
                        (
                            (xPosition - left)
                            /
                            (right - left)
                        )
                        *
                        ((int)high - (int)low)
                    )
                    +
                    (int)low
                );
        }

        public void StartRecording()
        {
            splitPointLog = new Dictionary<double, Pitch>();
            start = DateTime.Now;
            SplitPointChanged += SplitPoint_Changed;
        }

        public void StopRecording()
        {
            SplitPointChanged -= SplitPoint_Changed;
        }

        #region Event handler
        public delegate void SplitPointChangedHandler(float xPosition);
        public event SplitPointChangedHandler SplitPointChanged;
        
        private void SplitPoint_Changed(float xPosition)
        {
            splitPointLog.Add(DateTime.Now.Subtract(start).TotalSeconds, Translate(xPosition));
        }
        #endregion

    }
}
