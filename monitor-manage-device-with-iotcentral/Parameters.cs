// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using CommandLine;

namespace Learn.CoffeeMaker
{
    /// <summary>
    /// Command line parameters for the SimulatedDevice sample
    /// </summary>
    internal class Parameters
    {
        [Option(
            'i',
            "DpsIdScope",
            HelpText = "(Required if DeviceSecurityType is 'dps'). \nThe DPS ID Scope to use during device provisioning." +
            "\nDefaults to environment variable 'IOTHUB_DEVICE_DPS_ID_SCOPE'.")]
        public string DpsIdScope { get; set; } = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_DPS_ID_SCOPE");

        [Option(
            'd',
            "DeviceId",
            HelpText = "(Required if DeviceSecurityType is 'dps'). \nThe device registration Id to use during device provisioning." +
            "\nDefaults to environment variable 'IOTHUB_DEVICE_DPS_DEVICE_ID'.")]
        public string DeviceId { get; set; } = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_DPS_DEVICE_ID");

        [Option(
            'k',
            "DeviceSymmetricKey",
            HelpText = "(Required if DeviceSecurityType is 'dps'). \nThe device symmetric key to use during device provisioning." +
            "\nDefaults to environment variable 'IOTHUB_DEVICE_DPS_DEVICE_KEY'.")]
        public string DeviceSymmetricKey { get; set; } = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_DPS_DEVICE_KEY");

        [Option(
            'n',
            "Application running time (in seconds)",
            Required = false,
            HelpText = "The running time for this console application. Leave it unassigned to run the application until it is explicitly canceled using Control+C.")]
        public double? ApplicationRunningTime { get; set; }

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(DpsIdScope)
                    && !string.IsNullOrWhiteSpace(DeviceId)
                    && !string.IsNullOrWhiteSpace(DeviceSymmetricKey);
        }
    }
}
