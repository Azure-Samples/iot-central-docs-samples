using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EdgeHub;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions.Samples
{
    public static class FilterFunction
    {
        [FunctionName("FilterFunction")]
        public static async Task FilterMessageAndSendMessage(
            [EdgeHubTrigger("input1")] Message messageReceived,
            [EdgeHub(OutputName = "output1")] IAsyncCollector<Message> output,
            ILogger logger)
        {
            const int temperatureThreshold = 20;
            byte[] messageBytes = messageReceived.GetBytes();
            var messageString = System.Text.Encoding.UTF8.GetString(messageBytes);

            if (!string.IsNullOrEmpty(messageString))
            {
                logger.LogInformation("Info: Received one non-empty message");
                // Get the body of the message and deserialize it.
                var messageBody = JsonConvert.DeserializeObject<MessageBody>(messageString);

                if (messageBody != null && messageBody.machine.temperature > temperatureThreshold)
                {
                    // Send the message to the output as the temperature value is greater than the threshold.
                    var filteredMessage = new Message(messageBytes);
                    // Copy the properties of the original message into the new Message object.
                    foreach (KeyValuePair<string, string> prop in messageReceived.Properties)
                    {filteredMessage.Properties.Add(prop.Key, prop.Value);}
                    // Add a new property to the message to indicate it is an alert.
                    filteredMessage.Properties.Add("MessageType", "Alert");
                    // Send the message.
                    await output.AddAsync(filteredMessage);
                    logger.LogInformation("Info: Received and transferred a message with temperature above the threshold");
                }
            }
        }
    }
    //Define the expected schema for the body of incoming messages.
    class MessageBody
    {
        public Machine machine {get; set;}
        public Ambient ambient {get; set;}
        public string timeCreated {get; set;}
    }
    class Machine
    {
        public double temperature {get; set;}
        public double pressure {get; set;}
    }
    class Ambient
    {
        public double temperature {get; set;}
        public int humidity {get; set;}
    }
}   