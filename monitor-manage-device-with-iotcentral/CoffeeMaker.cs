using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Learn.CoffeeMaker
{
    internal enum StatusCode
    {
        Completed = 200,
        InProgress = 202,
        ReportDeviceInitialProperty = 203,
        BadRequest = 400,
        NotFound = 404
    }
    public class CoffeeMaker
    {
        private readonly Random _random = new();

        private readonly DeviceClient _deviceClient;

        private readonly bool _warrantyState;

        //Variables default values
        private double _optimalTemperature = 96d;
        private string _cupState = "detected";
        private string _brewingState = "notbrewing";
        private int _cupTimer = 20;
        private int _brewingTimer = 0;
        private bool _maintenanceState = false;

        public CoffeeMaker(DeviceClient deviceClient)
        {
            _deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));

            //When device starts it randomly sets the warranty state to either true or false.
            _warrantyState = _random.NextDouble() > 0.5 ? true : false;
        }

        //<Workflow>
        public async Task PerformOperationsAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Device successfully connected to Azure IoT Central");
            
            Console.WriteLine($"- Set handler for \"SetMaintenanceMode\" command.");
            await _deviceClient.SetMethodHandlerAsync("SetMaintenanceMode", HandleMaintenanceModeCommand, _deviceClient, cancellationToken);

            Console.WriteLine($"- Set handler for \"StartBrewing\" command.");
            await _deviceClient.SetMethodHandlerAsync("StartBrewing", HandleStartBrewingCommand, _deviceClient, cancellationToken);

            Console.WriteLine($"- Set handler to receive \"OptimalTemperature\" updates.");
            await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(OptimalTemperatureUpdateCallbackAsync, _deviceClient, cancellationToken);

            Console.WriteLine("- Update \"DeviceWarrantyExpired\" reported property on the initial startup.");
            await UpdateDeviceWarranty(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await SendTelemetryAsync(cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }
        }
        //</Workflow>

        //<Telemetry>
        //Send temperature and humidity telemetry, whether it's currently brewing and when a cup is detected.
        private async Task SendTelemetryAsync(CancellationToken cancellationToken)
        {
            //Simulate the telemetry values
            double temperature = _optimalTemperature + (_random.NextDouble() * 4) - 2;
            double humidity = 20 + (_random.NextDouble() * 80);

            // Cup timer - every 20 seconds randomly decide if the cup is present or not
            if (_cupTimer > 0)
            {
                _cupTimer--;

                if(_cupTimer == 0)
                {
                    _cupState = _random.NextDouble()  > 0.5 ? "detected" : "notdetected";
                    _cupTimer = 20;
                }
            }

            // Brewing timer
            if (_brewingTimer > 0)
            {
                _brewingTimer--;

                //Finished brewing
                if (_brewingTimer == 0)
                {
                    _brewingState = "notbrewing";
                }
            }

            // Create JSON message
            string messageBody = JsonConvert.SerializeObject(
                new
                {
                    WaterTemperature = temperature,
                    AirHumidity = humidity,
                    CupDetected = _cupState,
                    Brewing = _brewingState
                });
            using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            //Show the information in console
            double infoTemperature = Math.Round(temperature, 1);
            double infoHumidity = Math.Round(humidity, 1);
            string infoCup = _cupState == "detected" ? "Y" : "N";
            string infoBrewing = _brewingState == "brewing" ? "Y" : "N";
            string infoMaintenance = _maintenanceState ? "Y" : "N";

            Console.WriteLine($"Telemetry send: Temperature: {infoTemperature}ºC Humidity: {infoHumidity}% " +
                $"Cup Detected: {infoCup} Brewing: {infoBrewing} Maintenance Mode: {infoMaintenance}");

            //Send the message
            await _deviceClient.SendEventAsync(message, cancellationToken);
        }
        //</Telemetry>

        //<Commands>
        // The callback to handle "SetMaintenanceMode" command.
        private Task<MethodResponse> HandleMaintenanceModeCommand(MethodRequest request, object userContext)
        {
            try
            {
                Console.WriteLine(" * Maintenance command received");

                if (_maintenanceState)
                {
                    Console.WriteLine(" - Warning: The device is already in the maintenance mode.");
                }

                //Set state
                _maintenanceState = true;

                //Send response
                byte[] responsePayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject("Success"));
                return Task.FromResult(new MethodResponse(responsePayload, (int)StatusCode.Completed));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while handling \"SetMaintenanceMode\" command: {ex}");
                return Task.FromResult(new MethodResponse((int)StatusCode.BadRequest));
            }

        }

        // The callback to handle "StartBrewing" command.
        private Task<MethodResponse> HandleStartBrewingCommand(MethodRequest request, object userContext)
        {
            try
            {
                Console.WriteLine(" * Start brewing command received");

                if (_brewingState == "brewing")
                {
                    Console.WriteLine(" - Warning: The device is already brewing.");
                }

                if (_cupState == "notdetected")
                {
                    Console.WriteLine(" - Warning: There is no cup detected.");
                }

                if (_maintenanceState)
                {
                    Console.WriteLine(" - Warning: The device is in maintenance mode.");
                }

                //Set state - brew for 30 seconds
                if (_cupState == "detected" && _brewingState == "notbrewing" && !_maintenanceState)
                {
                    _brewingState = "brewing";
                    _brewingTimer = 30;
                }

                //Send response
                byte[] responsePayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject("Success"));
                return Task.FromResult(new MethodResponse(responsePayload, (int)StatusCode.Completed));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while handling \"StartBrewing\" command: {ex}");
                return Task.FromResult(new MethodResponse((int)StatusCode.BadRequest));
            }
        }
        //</Commands>

        // The desired property update callback, which receives the OptimalTemperature as a desired property update,
        // and updates the current _optimalTemperature value over telemetry and reported property update.
        //<Properties>
        private async Task OptimalTemperatureUpdateCallbackAsync(TwinCollection desiredProperties, object userContext)
        {
            const string propertyName = "OptimalTemperature";

            (bool optimalTempUpdateReceived, double optimalTemp) = GetPropertyFromTwin<double>(desiredProperties, propertyName);
            if (optimalTempUpdateReceived)
            {
                Console.WriteLine($" * Property: Received - {{ \"{propertyName}\": {optimalTemp}°C }}.");

                //Update reported property to In Progress
                string jsonPropertyPending = $"{{ \"{propertyName}\": {{ \"value\": {optimalTemp}, \"ac\": {(int)StatusCode.InProgress}, " +
                    $"\"av\": {desiredProperties.Version}, \"ad\": \"In progress - reporting optimal temperature\" }} }}";
                var reportedPropertyPending = new TwinCollection(jsonPropertyPending);
                await _deviceClient.UpdateReportedPropertiesAsync(reportedPropertyPending);
                Console.WriteLine($" * Property: Update - {{\"{propertyName} \": {optimalTemp}°C }} is {StatusCode.InProgress}.");

                //Update the optimal temperature
                _optimalTemperature = optimalTemp;

                //Update reported property to Completed
                string jsonProperty = $"{{ \"{propertyName}\": {{ \"value\": {optimalTemp}, \"ac\": {(int)StatusCode.Completed}, " +
                    $"\"av\": {desiredProperties.Version}, \"ad\": \"Successfully updated optimal temperature\" }} }}";
                var reportedProperty = new TwinCollection(jsonProperty);
                await _deviceClient.UpdateReportedPropertiesAsync(reportedProperty);
                Console.WriteLine($" * Property: Update - {{\"{propertyName} \": {optimalTemp}°C }} is {StatusCode.Completed}.");
            }
            else
            {
                Console.WriteLine($" * Property: Received an unrecognized property update from service:\n{desiredProperties.ToJson()}");
            }
        }

        private async Task UpdateDeviceWarranty(CancellationToken cancellationToken)
        {
            const string propertyName = "DeviceWarrantyExpired";

            var reportedProperties = new TwinCollection();
            reportedProperties[propertyName] = _warrantyState;

            await _deviceClient.UpdateReportedPropertiesAsync(reportedProperties, cancellationToken);
            Console.WriteLine($" * Property: Update - {{ \"{propertyName}\": {_warrantyState} }} is {StatusCode.Completed}.");
        }
        //</Properties>

        private static (bool, T) GetPropertyFromTwin<T>(TwinCollection collection, string propertyName)
        {
            return collection.Contains(propertyName) ? (true, (T)collection[propertyName]) : (false, default);
        }
    }
}
