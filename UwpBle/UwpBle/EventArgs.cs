using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpBle
{
    public class ConnectionEventArgs : EventArgs
    {
        public string DeviceName { get; set; }
        public ulong BluetoothAddress { get; set; }

        public ConnectionEventArgs(string deviceName, ulong bluetoothAddress)
        {
            DeviceName = deviceName;
            BluetoothAddress = bluetoothAddress;
        }
    }

    public class ValueReceivedEventArgs : EventArgs
    {
        public string ButtonId { get; set; }
        public ushort ButtonPressCount { get; set; }

        public ValueReceivedEventArgs(string buttonId, ushort buttonPressCount)
        {
            ButtonId = buttonId;
            ButtonPressCount = buttonPressCount;
        }
    }
}
