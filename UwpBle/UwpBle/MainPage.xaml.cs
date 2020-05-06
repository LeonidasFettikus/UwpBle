using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpBle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BluetoothLEAdvertisementWatcher _scanner;

        public MainPage()
        {
            this.InitializeComponent();

            //string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
            //var filter = new BluetoothLEAdvertisementFilter();
            //filter.Advertisement.ServiceUuids.Add(Guid.Parse("49535343-fe7d-4ae5-8fa9-9fafd205e455"));
            _scanner = new BluetoothLEAdvertisementWatcher();

            _scanner.Received += _scanner_Received;            
        }

        private void _scanner_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (args.Advertisement.LocalName.Contains("my-device"))
            {
                AddLog($"Found device with name {args.Advertisement.LocalName}.").Wait();
                var button = new IoTButton(args.BluetoothAddress);
                button.ValueReceived += Button_ValueReceived;
                button.Connected += Button_Connected;
                button.Disconnected += Button_Disconnected;
                Task.Run(button.Run);
            }
        }

        private void Button_Disconnected(object sender, ConnectionEventArgs e)
        {
            AddLog($"Button DISCONNECTED: {e.DeviceName}.").Wait();
        }

        private void Button_Connected(object sender, ConnectionEventArgs e)
        {
            AddLog($"Button CONNECTED: {e.DeviceName}.").Wait();
        }

        private void Button_ValueReceived(object sender, ValueReceivedEventArgs e)
        {
            AddLog($"Value RECEIVED: {e.ButtonId} - {e.ButtonPressCount}.").Wait();
        }

        private async Task AddLog(string value)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                LbLogs.Items.Add(value);
            });
        }       

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _scanner.Start();
            BtnStart.IsEnabled = false;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _scanner.Stop();
            BtnStart.IsEnabled = true;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            LbLogs.Items.Clear();
        }
    }
}
