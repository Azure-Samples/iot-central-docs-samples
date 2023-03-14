// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Learn.CoffeeMaker
{
    public class Program
    {
        private const string ModelId = "dtmi:com:example:ConnectedCoffeeMaker;1";
        private const string DpsEndpoint = "global.azure-devices-provisioning.net";
        public static async Task Main(string[] args)
        {
            //Parse application parameters
            Parameters parameters = null;
            ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(parsedParams =>
                {
                    parameters = parsedParams;
                })
                .WithNotParsed(errors =>
                {
                    Environment.Exit(1);
                });

            //Validate the environment variables
            if (!parameters.Validate())
            {
                Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(result, null, null));
                Environment.Exit(1);
            }

            Console.WriteLine("Press Control+C to quit the sample.");
            using var cts = parameters.ApplicationRunningTime.HasValue
                ? new CancellationTokenSource(TimeSpan.FromSeconds(parameters.ApplicationRunningTime.Value))
                : new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Sample execution cancellation requested; will exit.");
            };

            Console.WriteLine($"Set up the device client.");


            try
            {
                using DeviceClient deviceClient = await SetupDeviceClientAsync(parameters, cts.Token);
                var sample = new CoffeeMaker(deviceClient);
                await sample.PerformOperationsAsync(cts.Token);
                await deviceClient.CloseAsync(CancellationToken.None);
            }
            catch (OperationCanceledException) { } // User canceled operation

        }
        
        //<Provisioning>
        private static async Task<DeviceClient> SetupDeviceClientAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            // Provision a device via DPS, by sending the PnP model Id as DPS payload.
            using SecurityProvider symmetricKeyProvider = new SecurityProviderSymmetricKey(parameters.DeviceId, parameters.DevicePrimaryKey, null);
            using ProvisioningTransportHandler mqttTransportHandler = new ProvisioningTransportHandlerMqtt();
            ProvisioningDeviceClient pdc = ProvisioningDeviceClient.Create(DpsEndpoint, parameters.IdScope,
                symmetricKeyProvider, mqttTransportHandler);

            var pnpPayload = new ProvisioningRegistrationAdditionalData
            {
                JsonData = $"{{ \"modelId\": \"{ModelId}\" }}",
            };

            DeviceRegistrationResult dpsRegistrationResult = await pdc.RegisterAsync(pnpPayload, cancellationToken);

            // Initialize the device client instance using symmetric key based authentication, over Mqtt protocol (TCP, with fallback over Websocket) and setting the ModelId into ClientOptions.
            DeviceClient deviceClient;

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(dpsRegistrationResult.DeviceId, parameters.DevicePrimaryKey);

            var options = new ClientOptions
            {
                ModelId = ModelId,
            };

            deviceClient = DeviceClient.Create(dpsRegistrationResult.AssignedHub, authMethod, TransportType.Mqtt, options);
            return deviceClient;
        }
        //</Provisioning>
    }
}