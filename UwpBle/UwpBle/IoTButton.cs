using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace UwpBle
{
    public class IoTButton
    {
        private readonly ulong _address;
        private readonly AutoResetEvent _signal;

        private BluetoothLEDevice _device;
        private bool _valueReceived;

        public event EventHandler<ValueReceivedEventArgs> ValueReceived;
        public event EventHandler<ConnectionEventArgs> Connected;
        public event EventHandler<ConnectionEventArgs> Disconnected;

        public IoTButton(ulong address)
        {
            _address = address;
            _signal = new AutoResetEvent(false);
        }

        public async Task Run()
        {
            _device = await BluetoothLEDevice.FromBluetoothAddressAsync(_address);
            if (_device != null)
            {
                _device.ConnectionStatusChanged += _device_ConnectionStatusChanged;

                var services = await GetServices(_device);
                var service = GetService(services);
                var characteristics = await GetCharacteristics(service);
                var characteristic = GetCharacteristic(characteristics);
                await WriteClientConfiguration(characteristic);
                _signal.WaitOne();
                await SendShutdownSignal(characteristic);
                _signal.WaitOne();
            }
        }

        private void _device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
                Connected?.Invoke(this, new ConnectionEventArgs(_device.Name, _device.BluetoothAddress));
            else
            {
                Disconnected?.Invoke(this, new ConnectionEventArgs(_device.Name, _device.BluetoothAddress));
                _signal.Set();
            }

        }

        private async Task WriteClientConfiguration(GattCharacteristic characteristic)
        {
            characteristic.ValueChanged += Characteristic_ValueChanged;
            var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
            
            if (status == GattCommunicationStatus.Success)
            {
            }
        }

        private async Task SendShutdownSignal(GattCharacteristic characteristic)
        {
            var writer = new DataWriter();
            writer.WriteString("1");

            GattCommunicationStatus result = await characteristic.WriteValueAsync(writer.DetachBuffer());
            if (result == GattCommunicationStatus.Success)
            {
            }
        }

        private async Task<IReadOnlyList<GattDeviceService>> GetServices(BluetoothLEDevice device)
        {
            if (device != null)
            {
                var servicesResult = await device.GetGattServicesAsync();

                if (servicesResult.Status == GattCommunicationStatus.Success)
                {
                    return servicesResult.Services;
                }
            }

            return null;
        }

        private GattDeviceService GetService(IReadOnlyList<GattDeviceService> services)
        {
            if (services != null)
            {
                foreach (var service in services)
                {
                    if (service.Uuid == Guid.Parse("49535343-fe7d-4ae5-8fa9-9fafd205e455"))
                    {
                        return service;
                    }
                }
            }

            return null;
        }

        private async Task<IReadOnlyList<GattCharacteristic>> GetCharacteristics(GattDeviceService service)
        {
            if (service != null)
            {
                var characteristicsResult = await service.GetCharacteristicsAsync();

                if (characteristicsResult.Status == GattCommunicationStatus.Success)
                {
                    return characteristicsResult.Characteristics;
                }
            }

            return null;
        }

        private GattCharacteristic GetCharacteristic(IReadOnlyList<GattCharacteristic> characteristics)
        {
            if (characteristics != null)
            {
                foreach (var characteristic in characteristics)
                {
                    if (characteristic.Uuid == Guid.Parse("49535343-1e4d-4bd9-ba61-23c647249616"))
                    {
                        return characteristic;
                    }
                }
            }

            return null;
        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (!_valueReceived)
            {
                _valueReceived = true;
                var tet = args.CharacteristicValue.ToString();
                var byteArray = args.CharacteristicValue.ToArray();
                var strValue = Encoding.UTF8.GetString(byteArray);
                var buttonId = strValue.Split(";")[0];
                var buttonPressCount = strValue.Split(";")[1];
                ValueReceived?.Invoke(this, new ValueReceivedEventArgs(buttonId, ushort.Parse(buttonPressCount)));
                _signal.Set();
            }
        }
    }
}
