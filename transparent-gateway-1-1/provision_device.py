# -------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# --------------------------------------------------------------------------

from azure.iot.device import ProvisioningDeviceClient
import os

provisioning_host = (
    os.getenv("IOTHUB_DEVICE_DPS_ENDPOINT")
    if os.getenv("IOTHUB_DEVICE_DPS_ENDPOINT")
    else "global.azure-devices-provisioning.net"
)
id_scope = os.getenv("IOTHUB_DEVICE_DPS_ID_SCOPE")
registration_id = os.getenv("IOTHUB_DEVICE_DPS_DEVICE_ID")
symmetric_key = os.getenv("IOTHUB_DEVICE_DPS_DEVICE_KEY")

provisioning_device_client = ProvisioningDeviceClient.create_from_symmetric_key(
    provisioning_host=provisioning_host,
    registration_id=registration_id,
    id_scope=id_scope,
    symmetric_key=symmetric_key,
)

registration_result = provisioning_device_client.register()

print("The registration status is:")
print(registration_result.status)

# print("The registration state is:")
# print(registration_result.registration_state)
