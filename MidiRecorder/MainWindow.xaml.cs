using System;   
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BeatCounter beatCounter;
        private WavRecorder wavRecorder;
        private MidiRecorder midiRecorder;

        private bool isRecording;

        public MainWindow()
        {
            InitializeComponent();
            beatCounter = new BeatCounter(12,8);
            wavRecorder = new WavRecorder();

            isRecording = false;

            updateView();
        }

        private void updateView()
        {
            updateTempo();
            updateTimeSignature();
            updateRhythm();
            updateButtons();
        }

        private void updateTempo()
        {
            txb_tempo.Text = beatCounter.Tempo.ToString();
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
            if (isRecording)
            {
                btn_record.Content = "Stop recording";
            }
            else
            {
                btn_record.Content = "Record";
            }
        }

        private void rct_rhythm_Click(object sender, EventArgs e)
        {
            beatCounter.TimeSignature.Rhythm.Beats[Int32.Parse((sender as Rectangle).Tag.ToString())] ^= true;
            updateRhythm();
        }

        private void btn_conduct_Click(object sender, RoutedEventArgs e)
        {
            //med_playback.Source = new Uri("C:\\Users\\Bjorn\\Music\\Michael Buble\\Crazy Love\\03.Georgia On My Mind.mp3");
            //med_playback.Play();
            wavRecorder.Start();
            btn_tap.Focus();
        }

        private void med_playback_Initialized(object sender, EventArgs e)
        {
        }

        private void btn_tap_Click(object sender, RoutedEventArgs e)
        {
            beatCounter.AddBeat();
            updateTempo();
        }

        private void btn_record_Click(object sender, RoutedEventArgs e)
        {
            record();
        }

        private void record()
        {
            if (!isRecording)
            {
                midiRecorder = new MidiRecorder(Midi.InputDevice.InstalledDevices[3]);
                midiRecorder.StartRecording();
                wavRecorder.Start();
                isRecording = true;
            }
            else
            {
                midiRecorder.StopRecording();
                wavRecorder.Stop();
                isRecording = false;
            }
        }
    }
}
