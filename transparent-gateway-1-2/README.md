# IoT Edge module VM deploy

Detailed documentation is available on [Microsoft Docs](https://docs.microsoft.com/azure/iot-edge/how-to-install-iot-edge-ubuntuvm).

This ARM template uses cloud init to install the Azure IoT Edge runtime and configure it to use DPS to connect to IoT Central.

## ARM Template to deploy an IoT Edge 1.2 enabled VM pre-configured for an IoT Central application

ARM template to deploy a VM with IoT Edge pre-installed and configured for an IoT Central application (via cloud-init).

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fiot-central-docs-samples%2Fmaster%2Ftransparent-gateway-1-2%2FDeployGatewayVMs.json" target="_blank">
    <img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.png" />
</a>

The ARM template visualized for exploration

<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fiot-central-docs-samples%2Fmaster%2Ftransparent-gateway-1-2%2FDeployGatewayVMs.json" target="_blank">
    <img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/visualizebutton.png" /></a>

## Azure CLI command to deploy IoT Edge 1.2 enabled VM

```bash
az deployment group create \
  --subscription "<SUBSCRIPTION_NAME>" \
  --name transparentGatewayVMs \
  --resource-group <REPLACE_WITH_RESOURCE_GROUP_NAME> \
  --template-file DeployGatewayVMs.json \
  --parameters dnsLabelPrefix='<AZURE_GATEWAY_VM_DNS_PREFIX>' \
               dnsLabelPrefixLeaf='<AZURE_DOWNSTREAM_DEVICE_VM_DNS_PREFIX>' \
               ubuntuOSVersion='18.04-LTS'\
               adminUsername='AzureUser' \
               authenticationType='<password or sshPublicKey>' \
               scopeId='<IOT_CENTRAL_APP_SCOPE_ID>' \
               deviceId='<IOT_CENTRAL_GATEWAY_DEVICE_ID>' \
               deviceKey='<IOT_CENTRAL_GATEWAY_DEVICE_KEY>' \
               adminPasswordOrKey='<SSH_PUBLIC_KEY_OR_PASSWORD>'
```
