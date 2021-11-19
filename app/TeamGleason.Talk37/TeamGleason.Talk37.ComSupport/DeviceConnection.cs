using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;

/*
<Capabilities>
    <DeviceCapability Name="serialcommunication">
        <Device Id="any">
            <Function Type="name:serialPort" />
        </Device>
    </DeviceCapability>
</Capabilities>
*/

namespace TeamGleason.Talk37.ComSupport
{
    public class DeviceConnection
    {
        DeviceConnection()
        {
            _arduinoConnectivity = new ArduinoConnectivityUWP();
        }

        public static async Task<DeviceConnection> CreateAsync(string comPort)
        {
            var connection = new DeviceConnection();
            var connected = await connection._arduinoConnectivity.Connect(comPort);
            return connected ? connection : null;
        }

        public async Task PlayAnimationSequenceAsync(IReadOnlyList<IMediaMarker> markers)
        {
            var startTime = DateTime.UtcNow;

            foreach (var marker in markers)
            {
                var cueTime = startTime + marker.Time;
                var delay = cueTime - DateTime.UtcNow;
                if (TimeSpan.Zero < delay)
                {
                    await Task.Delay(delay);
                }

                await PlayAnimationAsync(marker.Text);
            }
        }

        public async Task PlayAnimationAsync(string visualString)
        {
            switch (visualString)
            {
                default:
                    await _arduinoConnectivity.UploadCompressedSequence(visualString);
                    break;

                case "-":
                    await ClearDisplayAsync();
                    break;
            }
        }

        public async Task ClearDisplayAsync()
        {
            await _arduinoConnectivity.ClearSequence();
        }

        private ArduinoConnectivityUWP _arduinoConnectivity;

        public void Close()
        {
            _arduinoConnectivity.Close();
        }
    }
}
