using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Provisioning.Security;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Dsp.Individual.Tpm
{
    /// <summary>
    /// Azure IoT Hub DPS の個別登録(TPM)を使ってアクセスするデバイスプログラム
    /// ※ プログラムを管理者権限で実行する必要があります
    /// </summary>
    class Program
    {
        // デバイスに与える情報
        const string DpsGlobalEndpoint = "global.azure-devices-provisioning.net";
        const string DpsIdScope = "";
        const string RegistrationId = "device-dps-indi-tpm";

        /// <summary>
        /// プログラム起動時の処理
        /// </summary>
        static async Task Main()
        {
            // EK(保証キー)を表示する (個別登録作成に必要)
            ShowEndorsementKey();

            // デバイスクライアントを生成する
            var client = await GetDeviceClientByDpsAsync(RegistrationId);

            // Azure IoT Hub にメッセージを送信する
            var message = new Message(Encoding.UTF8.GetBytes("Hello Azure IoT Hub DPS !!"));
            await client.SendEventAsync(message);
            Console.WriteLine("Success");
        }

        /// <summary>
        /// EK(保証キー)を表示する
        /// </summary>
        static void ShowEndorsementKey()
        {
            var security = new SecurityProviderTpmHsm(null);
            var endorsementKey = Convert.ToBase64String(security.GetEndorsementKey());
            Console.WriteLine($"Your EK is {endorsementKey}");
            Console.WriteLine("*** Please press the [Enter] key after registering the enrollment on your IoT DPS ***");
            Console.ReadLine();
        }

        /// <summary>
        /// Azure IoT Hub Device Provisioning Service (DPS) を使用してデバイスクライアントを生成する
        /// </summary>
        static async Task<DeviceClient> GetDeviceClientByDpsAsync(string registrationId)
        {
            // デバイスSASキーから認証情報を生成する
            var security = new SecurityProviderTpmHsm(registrationId);

            // デバイスの通信方法を指定する
            var transport = new ProvisioningTransportHandlerAmqp(); // MQTT通信はSDKが非対応

            // IoT Hub DPS 経由でデバイス登録を行う
            var provClient = ProvisioningDeviceClient.Create(DpsGlobalEndpoint, DpsIdScope, security, transport);
            var result = await provClient.RegisterAsync();
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                throw new Exception($"Device is not registered: {result.Status}");
            }

            // デバイスクライアントを生成する
            var auth = new DeviceAuthenticationWithTpm(result.DeviceId, security);
            var deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);

            return deviceClient;
        }
    }
}
