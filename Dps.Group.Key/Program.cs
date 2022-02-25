// dotnet add package Microsoft.Azure.Devices.Client
// dotnet add package Microsoft.Azure.Devices.Provisioning.Client
// dotnet add package Microsoft.Azure.Devices.Provisioning.Transport.Mqtt
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dps.Group.Key
{
    /// <summary>
    /// Azure IoT Hub DPS の登録グループ(対称キー)を使ってアクセスするデバイスプログラム
    /// </summary>
    class Program
    {
        // デバイスに与える情報
        const string DpsGlobalEndpoint = "global.azure-devices-provisioning.net";
        const string DpsIdScope = "";
        const string DpsPrimaryKey = "";
        const string DeviceId = "device-dps-group-key";
        static string DeviceSasKey = GenerateDeviceSasKey(DeviceId, DpsPrimaryKey);

        /// <summary>
        /// プログラム起動時の処理
        /// </summary>
        static async Task Main()
        {
            // デバイスクライアントを生成する
            var client = await GetDeviceClientByDpsAsync(DeviceId, DeviceSasKey);

            // Azure IoT Hub にメッセージを送信する
            var message = new Message(Encoding.UTF8.GetBytes("Hello Azure IoT Hub DPS !!"));
            await client.SendEventAsync(message);
            Console.WriteLine("Success");
        }

        /// <summary>
        /// Azure IoT Hub Device Provisioning Service (DPS) を使用してデバイスクライアントを生成する
        /// </summary>
        static async Task<DeviceClient> GetDeviceClientByDpsAsync(string deviceId, string deviceSasKey)
        {
            // デバイスSASキーから認証情報を生成する
            var security = new SecurityProviderSymmetricKey(deviceId, deviceSasKey, null);

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
            var auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, deviceSasKey);
            var deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);

            return deviceClient;
        }

        /// <summary>
        /// 対称キーのハッシュ計算をすることでデバイスキーを生成する
        /// (この処理は通常、デバイスの外で行う)
        /// </summary>
        static string GenerateDeviceSasKey(string deviceId, string primaryKey)
        {
            using var hmac = new HMACSHA256(Convert.FromBase64String(primaryKey));
            var key = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(deviceId)));
            return key;
        }
    }
}
