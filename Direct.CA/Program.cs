// dotnet add package Microsoft.Azure.Devices.Client
using Microsoft.Azure.Devices.Client;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Direct.CA
{
    /// <summary>
    /// Azure IoT Hub X.509 (CA 署名済み) を使って直接アクセスするデバイスプログラム
    /// </summary>
    class Program
    {
        // デバイスに与える情報
        const string IoTHubHostName = "iot-xxxxxxxxxxxxx";
        const string DeviceId = "device-direct-CA";
        const string CertificationPath = @"..\..\..\certificates\device.pfx";
        const string CertificationPassword = "1234";

        /// <summary>
        /// プログラム起動時の処理
        /// </summary>
        static async Task Main()
        {
            // デバイスクライアントを生成する
            var client = GetDeviceClient(CertificationPath, CertificationPassword);

            // Azure IoT Hub にメッセージを送信する
            var message = new Message(Encoding.UTF8.GetBytes("Hello Azure IoT Hub DPS !!"));
            await client.SendEventAsync(message);
            Console.WriteLine("Success");
        }

        /// <summary>
        /// デバイス証明書を使ってデバイスクライアントを生成する
        /// </summary>
        static DeviceClient GetDeviceClient(string certificatePath, string certificatePassword)
        {
            // デバイス証明書を参照する
            var certificate = LoadProvisioningCertificate(certificatePath, certificatePassword);

            // デバイスクライアントを生成する
            var auth = new DeviceAuthenticationWithX509Certificate(DeviceId, certificate);
            var deviceClient = DeviceClient.Create(IoTHubHostName, auth, TransportType.Mqtt);

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
