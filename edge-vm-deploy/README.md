# Edge module VM deploy

Detailed documentation is available on [Microsoft Docs](https://docs.microsoft.com/azure/iot-edge/how-to-install-iot-edge-ubuntuvm).

This ARM template uses cloud init to install the Azure IoT Edge runtime and configure it to use DPS to connect to IoT Central.

## ARM Template to deploy IoT Edge enabled VM pre-configured for an IoT Central edge module

ARM template to deploy a VM with IoT Edge pre-installed and configured for an IoT Central edge module (via cloud-init)

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fiot-central-docs-samples%main%2Fedge-vm-deploy%2FedgeModuleVMDeploy.json" target="_blank">
    <img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.png" />
</a>

The ARM template visualized for exploration

<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fiot-central-docs-samples%2Fmain%2Fedge-vm-deploy%2FedgeModuleVMDeploy.json" target="_blank">
    <img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/visualizebutton.png" /></a>

## Azure CLI command to deploy IoT Edge enabled VM with IoT Central edge module

```bash
az deployment group create \
  --subscription "<SUBSCRIPTION_NAME>" \
  --name edgeModuleVM \
  --resource-group <REPLACE_WITH_RESOURCE_GROUP_NAME> \
  --template-file edgeModuleVMDeploy.json \
  --parameters dnsLabelPrefix="<AZURE_VM_DNS_PREFIX>" \
  --parameters adminUsername="AzureUser" \
  --parameters adminPassword="<AZURE_VM_USER_PASSWORD>" \
  --parameters scopeId="<IOT_CENTRAL_APP_SCOPE_ID>" \
  --parameters deviceId="<IOT_CENTRAL_DEVICE_ID>" \
  --parameters deviceKey="<IOT_CENTRAL_DEVICE_KEY>"
```
