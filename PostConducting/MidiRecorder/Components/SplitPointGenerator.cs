using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Midi;

namespace MidiRecorder.Components
{
    /// <summary>
    ///     Keeps track of split point changes throughout a recording
    /// </summary>
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

        /// <summary>
        ///     Creates a new split point generator given information from the Detector.
        /// </summary>
        /// <param name="leftEnd">The position of the left (lower) end of the piano</param>
        /// <param name="rightEnd">The position of the right (higher) end of the piano</param>
        /// <param name="lowest">The lowest pitch on the piano</param>
        /// <param name="highest">The highest pitch on the piano</param>
        public SplitPointGenerator(float leftEnd, float rightEnd, Pitch lowest, Pitch highest)
        {
            this.left = leftEnd;
            this.right = rightEnd;
            this.keys = (int)highest - (int)lowest;
            this.low = lowest;
            this.high = highest;
        }

        /// <summary>
        ///     Gets the split point at a given time
        /// </summary>
        /// <param name="time">The time for which to get the split point</param>
        /// <returns>The split point at the given time</returns>
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

                Pitch result = Pitch.A0;
                splitPointLog.TryGetValue(key, out result);

                return result;
            }
        }

        /// <summary>
        ///     Translate a split point as horizontal position into a pitch.
        /// </summary>
        /// <param name="xPosition">The split point as a horizontal position</param>
        /// <returns>The pitch of the given horizontal position</returns>
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

        /// <summary>
        ///     Start recording changes in split point
        /// </summary>
        public void StartRecording()
        {
            splitPointLog = new Dictionary<double, Pitch>();
            start = DateTime.Now;
            SplitPointChanged += SplitPoint_Changed;
        }

        /// <summary>
        ///     Stop recording changes in split point.
        /// </summary>
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
