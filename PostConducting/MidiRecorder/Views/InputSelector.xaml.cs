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
using System.Windows.Shapes;

namespace MidiRecorder.Views
{
    /// <summary>
    /// Interaction logic for InputSelector.xaml
    /// </summary>
    public partial class InputSelector : Window
    {
        public Midi.InputDevice SelectedDevice { get; private set; }

        public InputSelector()
        {
            InitializeComponent();
            btn_ok.IsEnabled = false;
            lst_devices.ItemsSource = Midi.InputDevice.InstalledDevices;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedDevice = lst_devices.SelectedItem as Midi.InputDevice;
            Close();
        }

        private void lst_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lst_devices.SelectedItem != null)
            {
                btn_ok.IsEnabled = true;
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
