### 管理者権限での実行が必要 ###

# Azure IoT Device Provisioning Service に関する設定
$resourceGroupName = ""
$dpsName = "iot-xxxxxxxxxxxxx"
$dpsEnrollmentId = "sample-group"
$dpsCertificateName = $dpsEnrollmentId

# ルート証明書に関する設定
$rootCertName = "Azure IoT DSP Root - $dpsName - $dpsCertificateName"
$rootCertSubject = "CN=$rootCertName"
$rootCerFileName = "$dpsEnrollmentId.cer"

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

# 証明書を作成
az iot dps certificate create `
    --dps-name $dpsName `
    --resource-group $resourceGroupName `
    --name $dpsCertificateName `
    --path $rootCerFileName

# 証明書の検証用コードを取得
$verificationCode = az iot dps certificate generate-verification-code `
    --dps-name $dpsName `
    --resource-group $resourceGroupName `
    --name $dpsCertificateName `
    -o json --query "properties.verificationCode" -o json `
    --etag (az iot dps certificate show `
        --dps-name $dpsName `
        --resource-group $resourceGroupName `
        --name $dpsCertificateName `
        --query "etag" `
        -o json) 

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
$verifyCertFileName = "verify.cer"
Export-Certificate -cert $verifyCert -filePath $verifyCertFileName -Type Cert

# 検証用証明書をアップロードしてルート証明書を検証
az iot dps certificate verify `
    --dps-name $dpsName `
    --resource-group $resourceGroupName `
    --name $dpsCertificateName `
    --path $verifyCertFileName `
    --etag (az iot dps certificate show `
        --dps-name $dpsName `
        --resource-group $resourceGroupName `
        --name $dpsCertificateName `
        --query "etag" `
        -o json) 

# 検証用証明書を削除
Remove-Item $verifyCertFileName

# 登録グループを作成
az iot dps enrollment-group create `
    -g $resourceGroupName `
    --dps-name $dpsName `
    --enrollment-id $dpsEnrollmentId `
    --ca-name $dpsCertificateName
