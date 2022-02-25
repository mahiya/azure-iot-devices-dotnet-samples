# Azure IoT Device SDK for .NET を使ったデバイスプログラムの例

Azure IoT Hub Device Provisioning Service (DPS) を使うか使わない(直接)か、またデバイスの認証の種類ごとでの、Azure IoT Hub Device SDK for .NET を使ったデバイスプログラムの実装の例を格納しています。

|フォルダ名|Azure IoT Hub へのアクセス方法|
|--|--|
|Direct.CA|Azure IoT Hub X.509 (CA 署名済み) を使って直接アクセス|
|Direct.Certification|Azure IoT Hub X.509 (自己署名済み) を使って直接アクセス|
|Direct.Key|Azure IoT Hub に対称キーを使って直接アクセス|
|Dps.Group.Certification|Azure IoT Hub DPS の登録グループ(証明書)を使ってアクセス|
|Dps.Group.Key|Azure IoT Hub DPS の登録グループ(対称キー)を使ってアクセス|
|Dps.Individual.Certification|Azure IoT Hub DPS の個別登録(証明書)を使ってアクセス|
|Dps.Individual.Key|Azure IoT Hub DPS の個別登録(対称キー)を使ってアクセス|
|Dsp.Individual.Tpm|Azure IoT Hub DPS の個別登録(TPM)を使ってアクセス|