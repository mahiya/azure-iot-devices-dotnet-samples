// dotnet add package Microsoft.Azure.Devices.Client
// dotnet add package Microsoft.Azure.Devices.Provisioning.Client
// dotnet add package Microsoft.Azure.Devices.Provisioning.Transport.Mqtt
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Dps.Individual.Certification
{
    /// <summary>
    /// Azure IoT Hub DPS の個別登録(証明書)を使ってアクセスするデバイスプログラム
    /// </summary>
    class Program
    {
        // デバイスに与える情報
        const string DpsGlobalEndpoint = "global.azure-devices-provisioning.net";
        const string DpsIdScope = "";
        const string CertificationPath = @"..\..\..\certificates\device.pfx";
        const string CertificationPassword = "1234";

        /// <summary>
        /// プログラム起動時の処理
        /// </summary>
        static async Task Main()
        {
            // デバイスクライアントを生成する
            var client = await GetDeviceClientByDpsAsync(CertificationPath, CertificationPassword);

            // Azure IoT Hub にメッセージを送信する
            var message = new Message(Encoding.UTF8.GetBytes("Hello Azure IoT Hub DPS !!"));
            await client.SendEventAsync(message);
            Console.WriteLine("Success");
        }

        /// <summary>
        /// Azure IoT Hub Device Provisioning Service (DPS) を使用してデバイスクライアントを生成する
        /// </summary>
        static async Task<DeviceClient> GetDeviceClientByDpsAsync(string certificatePath, string certificatePassword)
        {
            // デバイス証明書を参照する
            var certificate = LoadProvisioningCertificate(certificatePath, certificatePassword);
            var security = new SecurityProviderX509Certificate(certificate);

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
            var auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, certificate);
            var deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);

            return deviceClient;
        }

        /// <summary>
        /// デバイス証明書を取得する
        /// </summary>
        static X509Certificate2 LoadProvisioningCertificate(string certificatePath, string certificatePassword)
        {
            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(certificatePath, certificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;
            foreach (X509Certificate2 element in certificateCollection)
            {
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                throw new FileNotFoundException($"{certificatePath} did not contain any certificate with a private key.");
            }

            return certificate;
        }
    }
}
