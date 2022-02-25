### 管理者権限での実行が必要 ###

# Azure IoT Hub に関する設定
$iotHubName = "iot-xxxxxxxxxxxxx"
$certificateName = "sample-cert"

# ルート証明書に関する設定
$rootCertName = "Azure IoT Hub Root - $iotHubName - $certificateName"
$rootCertSubject = "CN=$rootCertName"

# 証明書の出力先
$certDirPath = "certificates"
$rootCerFileName = "$certDirPath\root.cer"
$verifyCertPath = "$certDirPath\verify.cer";

# 生成した証明書を格納するフォルダを作成
New-Item $certDirPath -ItemType Directory -Force

# ルート証明書を作成
$selfSignedArgs = @{
    "-DnsName"           = $rootCertName;
    "-CertStoreLocation" = "cert:\LocalMachine\My";
    "-KeyUsage"          = "CertSign";
    "-TextExtension"     = @(("2.5.29.19={text}ca=TRUE&pathlength=12"));
}
$rootCert = New-SelfSignedCertificate @selfSignedArgs

# ルート証明書をファイルとして出力
Export-Certificate -Cert $rootCert -FilePath $rootCerFileName -Type CERT

# Azure IoT Hub に証明書を登録
az iot hub certificate create `
    --hub-name $iotHubName `
    --name $certificateName `
    --path $rootCerFileName

# 証明書の検証用コードを取得
$verificationCode = az iot hub certificate generate-verification-code `
    --hub-name $iotHubName `
    --name $certificateName `
    --output json --query "properties.verificationCode" -o json `
    --etag (az iot hub certificate show `
        --hub-name $iotHubName `
        --name $certificateName `
        --query "etag" `
        --output json) 

# 証明書ストアからルート証明書を取得する
$certificates = gci -Recurse Cert:\LocalMachine\ | ? { $_.gettype().name -eq "X509Certificate2" }
$rootCert = $certificates | ? { $_.subject -eq $rootCertSubject -and $_.PSParentPath -eq "Microsoft.PowerShell.Security\Certificate::LocalMachine\My" }

# 検証用証明書を作成
$selfSignedArgs = @{
    "-DnsName"           = $verificationCode;
    "-CertStoreLocation" = "cert:\LocalMachine\My";
    "-TextExtension"     = @("2.5.29.37={text}1.3.6.1.5.5.7.3.2,1.3.6.1.5.5.7.3.1", "2.5.29.19={text}ca=FALSE&pathlength=0");
    "-Signer"            = $rootCert;
}
$verifyCert = New-SelfSignedCertificate @selfSignedArgs

# 検証用証明書をファイルとして出力
Export-Certificate -cert $verifyCert -filePath $verifyCertPath -Type Cert

# 検証用証明書をアップロードしてルート証明書を検証
az iot hub certificate verify `
    --hub-name $iotHubName `
    --name $certificateName `
    --path $verifyCertPath `
    --etag (az iot hub certificate show `
        --hub-name $iotHubName `
        --name $certificateName `
        --query "etag" `
        --output json) 
