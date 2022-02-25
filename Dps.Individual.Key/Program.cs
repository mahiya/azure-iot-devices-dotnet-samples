// dotnet add package Microsoft.Azure.Devices.Client
// dotnet add package Microsoft.Azure.Devices.Provisioning.Client
// dotnet add package Microsoft.Azure.Devices.Provisioning.Transport.Mqtt
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Dps.Individual.Key
{
    /// <summary>
    /// Azure IoT Hub DPS の個別登録(対称キー)を使ってアクセスするデバイスプログラム
    /// </summary>
    class Program
    {
        // デバイスに与える情報
        const string DpsGlobalEndpoint = "global.azure-devices-provisioning.net";
        const string DpsIdScope = "";
        const string DpsPrimaryKey = "";
        const string RegistrationId = "";

        /// <summary>
        /// プログラム起動時の処理
        /// </summary>
        static async Task Main()
        {
            // デバイスクライアントを生成する
            var client = await GetDeviceClientByDpsAsync(RegistrationId, DpsPrimaryKey);

            // Azure IoT Hub にメッセージを送信する
            var message = new Message(Encoding.UTF8.GetBytes("Hello Azure IoT Hub DPS !!"));
            await client.SendEventAsync(message);
            Console.WriteLine("Success");
        }

        /// <summary>
        /// Azure IoT Hub Device Provisioning Service (DPS) を使用してデバイスクライアントを生成する
        /// </summary>
        static async Task<DeviceClient> GetDeviceClientByDpsAsync(string registrationId, string primaryKey)
        {
            // デバイスSASキーから認証情報を生成する
            var security = new SecurityProviderSymmetricKey(registrationId, primaryKey, null);

            // デバイスの通信方法を指定する
            var transport = new ProvisioningTransportHandlerMqtt();

            // IoT Hub DPS 経由でデバイス登録を行う
            var provClient = ProvisioningDeviceClient.Create(DpsGlobalEndpoint, DpsIdScope, security, transport);
            var result = await provClient.RegisterAsync();
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception($"Device is not registered: {result.Status}");
            }

            // デバイスクライアントを生成する
            var auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, primaryKey);
            var deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);

            return deviceClient;
        }
    }
}
