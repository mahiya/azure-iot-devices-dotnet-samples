# Azure IoT Device Provisioning Service に関する設定
$iotHubName = "iot-xxxxxxxxxxxxx"

# デバイス証明書に関する設定
$deviceName = "device-direct-cert"
$certPassword = "1234"
$certificateValidityInYears = 10

# 証明書の出力先
$certDirPath = "certificates"
$rootCertPath = "$certDirPath\root.cer";
$deviceCertPath = "$certDirPath\device.pfx";

# 生成した証明書を格納するフォルダを作成
New-Item $certDirPath -ItemType Directory -Force

# ルート証明書を作成して出力
$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=$deviceName" `
    -KeySpec Signature `
    -KeyExportPolicy Exportable `
    -HashAlgorithm sha256 `
    -KeyLength 2048 `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2") `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddYears($certificateValidityInYears)
Set-Content -Path $rootCertPath -Value ([Convert]::ToBase64String($cert.RawData)) -Encoding Ascii

# デバイス証明書を作成して出力
$secureCertPassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
$cert | Export-PfxCertificate -FilePath $deviceCertPath -Password $secureCertPassword

# デバイスを Azure IoT Hub に登録
az iot hub device-identity create `
    -n $iotHubName `
    -d $deviceName `
    --auth-method x509_thumbprint `
    --ptp $cert.Thumbprint `
    --stp $cert.Thumbprint
