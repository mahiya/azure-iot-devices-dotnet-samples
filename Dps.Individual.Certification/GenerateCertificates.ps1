# Azure IoT Device Provisioning Service に関する設定
$resourceGroupName = ""
$dpsName = ""

# デバイス証明書に関する設定
$deviceName = "device-dps-indi-cert"
$certPassword = "1234"
$certificateValidityInYears = 10

# 証明書の出力先
$certDirPath = "certificates"
$rootCertPath = "$certDirPath\root.cer";
$deviceCertPath = "$certDirPath\device.pfx";

# 生成した証明書を格納するフォルダを作成
New-Item certificates -ItemType Directory -Force

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

# デバイス証明書を作成して出力
$secureCertPassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $deviceCertPath -Password $secureCertPassword

# Azure IoT Hub DPS に対して個々のデバイス登録を行う
az iot dps enrollment create `
    -g $resourceGroupName `
    --dps-name $dpsName `
    --enrollment-id $deviceName `
    --attestation-type x509 `
    --certificate-path $rootCertPath