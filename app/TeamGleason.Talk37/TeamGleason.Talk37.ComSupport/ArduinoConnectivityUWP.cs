using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace TeamGleason.Talk37.ComSupport
{
    public class ArduinoConnectivityUWP : INotifyPropertyChanged
    {
        enum ArduinoRPCCommands : byte { UPLOAD_SEQUENCE8 = 1, CLEAR_DISPLAY = 2, DISPLAY_BRIGHTNESS = 3, SYSLOGGING = 4, EEPROM_CLEAR = 5, CONNECT_HEADERRQ = 6 };

        private SerialDevice _serialDevice;
        private DataReader _dataReaderObject;
        private DataWriter _dataWriterObject;



        #region NotifyProperty
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                NotifyPropertyChanged(propertyName);
        }



        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private bool _connected = false;
        public bool Connected
        {
            get
            {
                return _connected;
            }
            set
            {
                _connected = value;
                NotifyPropertyChanged("Connected");
            }
        }



        public async Task<bool> Connect(string port)
        {
            var filter = SerialDevice.GetDeviceSelector(port);
            var devices = await DeviceInformation.FindAllAsync(filter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                _serialDevice = await SerialDevice.FromIdAsync(deviceId);

                if (_serialDevice != null)
                {
                    _serialDevice.BaudRate = 57600;
                    _serialDevice.StopBits = SerialStopBitCount.One;
                    _serialDevice.DataBits = 8;
                    _serialDevice.Parity = SerialParity.None;
                    _serialDevice.Handshake = SerialHandshake.None;

                    // Create a single device writer for this port connection
                    _dataWriterObject = new DataWriter(_serialDevice.OutputStream);

                    // Create a single device reader for this port connection
                    _dataReaderObject = new DataReader(_serialDevice.InputStream);

                    // Allow partial reads of the input stream
                    _dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

                    Connected = true;
                }
            }
            return Connected;
        }


        public void Close()
        {
            // If serial device defined...
            if (_serialDevice != null)
            {
                // Dispose and clear device
                _serialDevice.Dispose();
                _serialDevice = null;
            }

            // If data reader defined...
            if (_dataReaderObject != null)
            {
                // Detatch reader stream
                _dataReaderObject.DetachStream();

                // Dispose and clear data reader
                _dataReaderObject.Dispose();
                _dataReaderObject = null;
            }

            // If data writer defined...
            if (_dataWriterObject != null)
            {
                // Detatch writer stream
                _dataWriterObject.DetachStream();

                // Dispose and clear data writer
                _dataWriterObject.Dispose();
                _dataWriterObject = null;
            }

            // Port now closed
            Connected = false;
        }



        private async Task WriteAsync(byte[] data)
        {
            // Write block of data to serial port
            _dataWriterObject.WriteBytes(data);

            // Transfer data to the serial device now
            await _dataWriterObject.StoreAsync();

            // Flush the data out to the serial device now
            await _dataWriterObject.FlushAsync();
        }



        private async Task WriteAsync(string str)
        {
            byte[] toBytes = Encoding.ASCII.GetBytes(str);
            await WriteAsync(toBytes);
        }




        public string DeviceSequenceString(string compressedSeq)
        {
            byte rpcCommand = (byte)ArduinoRPCCommands.UPLOAD_SEQUENCE8;
            StringBuilder sb = new StringBuilder();

            string str = string.Format("{0:x4}", compressedSeq.Length);
            sb.Append(string.Format("{0:x2}", rpcCommand));
            sb.Append(string.Format("{0:x4}", compressedSeq.Length));
            sb.Append(compressedSeq);
            return sb.ToString();
        }




        public async Task<bool> UploadSequence(string seq)
        {
            try
            {
                if (Connected)
                {
                    await WriteAsync(DeviceSequenceString(seq));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**EXCEPTION ArduinoConnectivity::UploadSequence " + ex.ToString());
            }
            return false;
        }



        public async Task<bool> UploadCompressedSequence(string compressedSeq)
        {
            try
            {
                if (Connected)
                {
                    await WriteAsync(compressedSeq);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**EXCEPTION ArduinoConnectivity::UploadSequence " + ex.ToString());
            }
            return false;
        }



        public async Task<bool> ClearSequence()
        {
            byte rpcCommand = (byte)ArduinoRPCCommands.CLEAR_DISPLAY;
            StringBuilder sb = new StringBuilder();

            try
            {
                if (Connected)
                {
                    sb.Append(string.Format("{0:x2}", rpcCommand));
                    sb.Append(string.Format("{0:x4}", 0));
                    await WriteAsync(sb.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**EXCEPTION ArduinoConnectivity::ClearSequence " + ex.ToString());
            }
            return false;
        }


    }
}
