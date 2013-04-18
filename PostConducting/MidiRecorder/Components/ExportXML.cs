using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Midi;

namespace MidiRecorder.Components
{
    using Model;

    /// <summary>
    ///     Support class for exporting a collection of ordered processed notes to MusicXML.
    /// </summary>
    class ExportXML
    {
        private XmlTextWriter writer;
        private static String[] sharpNotes  = { "C", "C", "D", "D", "E", "F", "F", "G", "G", "A", "A", "B" };
        private static String[] sharp       = { "0", "1", "0", "1", "0", "0", "1", "0", "1", "0", "1", "0" };
        private static String[] flatNotes   = { "C", "D", "D", "E", "E", "F", "G", "G", "A", "A", "B", "B" };
        private static String[] flat        = { "0", "-1", "0", "-1", "0", "0", "-1", "0", "-1", "0", "-1", "0" };

        int divisions = 0;
        int divisionsPerBeat = 0;

        int subDiv = 0;
        TimeSignature timeSignature;

        /// <summary>
        ///     Sets up XML exporter.
        /// </summary>
        /// <param name="subDivision">Highest note division allowed (1, 2, 4, 8, 16,...)</param>
        /// <param name="ts">The time signature used for the recording</param>
        /// <param name="file">The XML file path to export to</param>
        public ExportXML(int subDivision, TimeSignature ts, string file)
        {
            divisions = ts.BeatsPerBar * subDivision / (int)ts.Subdivision;
            divisionsPerBeat = divisions / ts.BeatsPerBar;

            writer = new XmlTextWriter(file, Encoding.UTF8);

            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;

            this.subDiv = subDivision;
            this.timeSignature = ts;
        }

        /// <summary>
        ///     Exports collection of note to MusicXML.
        /// </summary>
        /// <param name="notes">Collection of ordered notes</param>
        public void Export(IEnumerable<NoteDetailed> notes)
        {
            WriteHeader();

            writer.WriteStartElement("score-partwise");
            writer.WriteStartAttribute("version");
            writer.WriteValue("3.0");
            writer.WriteEndAttribute();

            WritePartList();

            writer.WriteStartElement("part");
            writer.WriteStartAttribute("id");
            writer.WriteValue("P1");
            writer.WriteEndAttribute();

            WriteFirstMeasure(timeSignature, subDiv);
            
            // Write all bars and notes
            int bar = 0;
            for (int i = 0; i < notes.Count(); i++)
            {
                NoteDetailed note = notes.ElementAt(i);
                if ((note.bar + 1) > bar)
                {
                    bar++;
                    writer.WriteEndElement();
                    writer.WriteStartElement("measure");
                    writer.WriteStartAttribute("number");
                    writer.WriteValue((bar + 2) + "");
                    writer.WriteEndAttribute();
                }

                // Note
                writer.WriteStartElement("note");

                // Pitch
                writer.WriteStartElement("pitch");
                writer.WriteElementString("step", sharpNotes[note.pitch.PositionInOctave()]);
                if (sharp[note.pitch.PositionInOctave()] != "0")
                {
                    writer.WriteElementString("alter", sharp[note.pitch.PositionInOctave()]);
                }
                writer.WriteElementString("octave", note.pitch.Octave() + "");
                writer.WriteEndElement();
                // End pitch

                // Type
                writer.WriteElementString("type", GetNoteType(note.duration, timeSignature.Subdivision));
                // End type

                // Duration
                writer.WriteElementString("duration", (divisionsPerBeat * note.duration) + "");
                // End duration

                writer.WriteEndElement();
                // End note

                // Rest?
                if ((i + 1) < notes.Count())
                {
                    NoteDetailed next = notes.ElementAt(i + 1);
                    float rest = 0;
                    if (next.bar == note.bar)
                    {
                        rest = next.beat - note.duration - note.beat;
                    }
                    else
                    {
                        rest = timeSignature.BeatsPerBar - note.duration - note.beat;
                    }
                    // Rest
                    if (rest != 0)
                    {
                        writer.WriteStartElement("note");
                        writer.WriteStartElement("rest");
                        writer.WriteEndElement();
                        writer.WriteElementString("duration", (divisionsPerBeat * rest) + "");
                        writer.WriteElementString("type", GetNoteType(rest, timeSignature.Subdivision));
                        writer.WriteEndElement();
                    }
                    // End rest
                }
            }
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.Close();
        }

        private void WriteHeader()
        {
            writer.WriteStartDocument(false);
            writer.WriteDocType("score-partwise", "-//Recordare//DTD MusicXML 1.0 Partwise//EN", "http://www.musicxml.org/dtds/partwise.dtd", null);
        }

        private void WritePartList()
        {
            writer.WriteRaw("<part-list><score-part id=\"P1\"><part-name>Music</part-name></score-part></part-list>");
        }

        private void WriteFirstMeasure(TimeSignature ts, int subDivision)
        {
            writer.WriteStartElement("measure");
            writer.WriteStartAttribute("number");
            writer.WriteValue(1 + "");
            writer.WriteEndAttribute();

            writer.WriteStartElement("attributes");
            writer.WriteElementString("divisions", divisions + "");

            writer.WriteStartElement("key");
            writer.WriteElementString("fifths", "0");
            writer.WriteElementString("mode", "major");
            writer.WriteEndElement();

            writer.WriteStartElement("time");
            writer.WriteElementString("beats", ts.BeatsPerBar + "");
            writer.WriteElementString("beat-type", (int)ts.Subdivision + "");
            writer.WriteEndElement();

            writer.WriteStartElement("clef");
            writer.WriteElementString("sign", "G");
            writer.WriteElementString("line", "2");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private string GetNoteType(float duration, Subdivision s)
        {
            int type = (int)((int)s / duration);

            switch (type)
            {
                case 1:
                    return "whole";
                case 2:
                    return "half";
                case 4:
                    return "quarter";
                case 8:
                    return "eighth";
                case 16:
                    return "16th";
                case 32:
                    return "32nd";
                case 64:
                    return "64th";
                case 128:
                    return "128th";
                case 256:
                default:
                    return "256th";
            }
        }
    }
}
