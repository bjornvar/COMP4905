using Microsoft.Win32;
using System;   
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MidiRecorder
{
    using Components;
    using Views;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BeatCounter beatCounter;
        private WavRecorder wavRecorder;
        private MidiRecorder midiRecorder;

        private bool isRecording;
        private bool isConducting;

        public MainWindow()
        {
            InitializeComponent();
            wavRecorder = new WavRecorder();

            isRecording = false;
            isConducting = false;

            updateView();
        }

        #region View functions
        private void updateView()
        {
            txt_beats.Text = "4";
            txt_beatType.Text = "4";
            txt_subdivision.Text = "8";

            updateButtons();
        }

        private void updateTempo()
        {
            if (beatCounter != null)
            {
                txb_tempo.Text = beatCounter.Tempo.ToString();
            }
            else
            {
                txb_tempo.Text = "0";
            }
        }

        private void updateTimeSignature()
        {
            txt_beats.Text = beatCounter.TimeSignature.BeatsPerBar.ToString();
            txt_beatType.Text = (int)beatCounter.TimeSignature.Subdivision + ""; 
        }

        private void updateRhythm()
        {
            stc_rythm.Children.Clear();

            int i = 0;
            foreach (bool emphasized in beatCounter.TimeSignature.Rhythm.Beats)
            {
                Rectangle r = new Rectangle();
                if (emphasized)
                {
                    r.Style = (Style)FindResource("RythmBoxActive");
                }
                else
                {
                    r.Style = (Style)FindResource("RythmBoxInactive");
                }
                r.Tag = i++;
                r.PreviewMouseLeftButtonUp += rct_rhythm_Click;
                stc_rythm.Children.Add(r);
            }
        }

        private void updateButtons()
        {
            if (isRecording) { btn_record.Content = "Stop recording"; }
            else { btn_record.Content = "1. Record"; }

            if (isConducting) { btn_conduct.Content = "Stop conducting"; }
            else { btn_conduct.Content = "2. Conduct"; }
        }
        #endregion

        #region Event handlers
        private void rct_rhythm_Click(object sender, EventArgs e)
        {
            beatCounter.TimeSignature.Rhythm.Beats[Int32.Parse((sender as Rectangle).Tag.ToString())] ^= true;
            updateRhythm();
        }

        private void btn_conduct_Click(object sender, RoutedEventArgs e)
        {
            if (!isConducting)
            {
                StartConducting();
            }
            else
            {
                StopConducting();
            }
            updateButtons();
        }

        private void btn_record_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        private void btn_tap_Click(object sender, RoutedEventArgs e)
        {
            beatCounter.AddBeat();
            updateTempo();
        }

        private void btn_xml_Click(object sender, RoutedEventArgs e)
        {
            ExportXML();
        }
        #endregion

        #region Fan-outs
        private void StartRecording()
        {
            InputSelector i = new InputSelector();
                
            // Event handler for selecting input device
            i.Closed += (sender, e) =>
            {
                if (i.SelectedDevice != null)
                {
                    midiRecorder = new MidiRecorder(i.SelectedDevice);
                    midiRecorder.StartRecording();

                    wavRecorder.Start();
                    isRecording = true;
                    updateButtons();
                }
            };

            i.Visibility = System.Windows.Visibility.Visible;
        }

        private void StopRecording()
        {
            midiRecorder.StopRecording();
            wavRecorder.Stop();
            isRecording = false;
            updateButtons();
        }

        private void StartConducting()
        {
            beatCounter = new BeatCounter(Int32.Parse(txt_beats.Text), Int32.Parse(txt_beatType.Text));
            try { wavRecorder.Sound.Play(); }
            catch (NullReferenceException) { }
            beatCounter.AddBeat();
            btn_tap.IsEnabled = true;
            btn_tap.Focus();
            isConducting = true;
        }

        private void StopConducting()
        {
            wavRecorder.Sound.Stop();
            btn_tap.IsEnabled = false;
            isConducting = false;
            if (MessageBox.Show(this, "Process recording?", "Process", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                midiRecorder.Conduct(beatCounter.beats, beatCounter.TimeSignature);
                midiRecorder.Quantize(Int32.Parse(txt_subdivision.Text), beatCounter.TimeSignature);
            }
        }

        private void ExportXML()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileOk +=
                (s, f) =>
                {
                    ExportXML xml = new ExportXML(Int32.Parse(txt_subdivision.Text), beatCounter.TimeSignature, dlg.FileName);
                    xml.Export(midiRecorder.processedNotes);
                };
            dlg.ShowDialog(this);
        }
        #endregion
    }
}
