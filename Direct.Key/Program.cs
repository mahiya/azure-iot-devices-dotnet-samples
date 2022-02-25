// dotnet add package Microsoft.Azure.Devices.Client
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Direct.Key
{
    /// <summary>
    /// Azure IoT Hub に対称キーを使って直接アクセスするデバイスプログラム
    /// </summary>
    class Program
    {
        // デバイスに与える情報
        const string IoTHubHostName = "iot-xxxxxxxxxxxxx";
        const string DeviceId = "device-direct-key";
        const string SharedAccessKey = "";

        /// <summary>
        /// プログラム起動時の処理
        /// </summary>
        static async Task Main()
        {
            // デバイスクライアントを作成する
            var ConnectionString = $"HostName={IoTHubHostName};DeviceId={DeviceId};SharedAccessKey={SharedAccessKey}";
            var client = DeviceClient.CreateFromConnectionString(ConnectionString);

            // Azure IoT Hub にメッセージを送信する
            var message = new Message(Encoding.UTF8.GetBytes("Hello Azure IoT Hub DPS !!"));
            await client.SendEventAsync(message);
            Console.WriteLine("Success");
        }
    }
}
