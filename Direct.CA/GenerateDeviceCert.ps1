### 管理者権限での実行が必要 ###

# スクリプトの引数にするべき情報
$deviceName = "device-direct-CA"
$certPassword = "1234"

# Azure IoT Hub に関する設定
$iotHubName          = "iot-xxxxxxxxxxxxx"
$certificateName     = "sample-cert"

# ルート証明書に関する設定
$rootCertName        = "Azure IoT Hub Root - $iotHubName - $certificateName"
$rootCertSubject     = "CN=$rootCertName"

# デバイス証明書に関する設定
$secureCertPassword  = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
$certDirPath         = "certificates"
$deviceCertPath      = "$certDirPath\device.pfx";

# デバイスを Azure IoT Hub に登録
az iot hub device-identity create `
    -n $iotHubName `
    -d $deviceName `
    --auth-method x509_ca

# 証明書ストアからルート証明書を読み込む
$signingCert = gci -Recurse Cert:\LocalMachine\ `
| ? { $_.gettype().name -eq "X509Certificate2" } `
| ? { $_.subject -eq $rootCertSubject -and $_.PSParentPath -eq "Microsoft.PowerShell.Security\Certificate::LocalMachine\My" }
if ($signingCert -eq $null) {
    throw ("Unable to find certificate with subjectName $rootCertSubject")
}

# デバイス証明書を作成
$selfSignedArgs = @{
    "-DnsName"           = $deviceName;
    "-CertStoreLocation" = "cert:\LocalMachine\My";
    "-KeyUsage"          = "CertSign";
    "-TextExtension"     = @("2.5.29.37={text}1.3.6.1.5.5.7.3.2,1.3.6.1.5.5.7.3.1", "2.5.29.19={text}ca=FALSE&pathlength=0");
    "-Signer"            = $signingCert;
}
$deviceCert = New-SelfSignedCertificate @selfSignedArgs

# デバイス証明書をファイルとして出力
Export-PFXCertificate -cert $deviceCert -filePath $deviceCertPath -password $secureCertPassword
