using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using System.Threading;

namespace DotNetModules
{
    //A simulated device module
    public class DotNetSimulatedDevice : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private String configuration;

        /// <summary>
        /// Invoked by the Module Host to create a new DotNetSimulatedDevice instance.
        /// </summary>
        /// <param name="broker">The Broker-instance that brokers messages to/from
        /// other modules in the gateway
        /// </param>        
        /// <param name="configuration">Must a least contain a (physical) address for the 
        /// device, used for mapping to an IoT hub device identity (DeviceId/DeviceKey).
        /// Using real devices, the module would receive this (MAC)address from the device</param>
        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
            this.broker = broker;
            Console.WriteLine("Simulated device created: " + this.configuration);
        }
        /// <summary>
        /// Invoked by the Module Host to dispose of the DotNetSimulatedDevice instance.        
        /// </summary>
        public void Destroy()
        {
            Console.WriteLine("This is DotNetSimulatedDevice.destroy()!");
        }
        /// <summary>
        /// The DotNetSimulatedDevice receives a message from the broker.
        /// A filter is applied to only accept IoTHub-module messages
        /// (for C2D-commands).
        /// </summary>
        /// <param name="received_message"></param>
        public void Receive(Message received_message)
        {
            //if (received_message.Properties["source"] == "iothub")
            //{
            Console.WriteLine("DotNetSimulated device receives message from " +
                received_message.Properties["source"] + ". Content: ");
            Console.WriteLine(Encoding.UTF8.GetString(received_message.Content, 0,
                received_message.Content.Length));
            //}            
        }
        /// <summary>
        /// Creates and starts a new thread that allows the DotNetSimulatedDevice 
        /// to publish messages to the Broker, when the gateway is ready.
        /// </summary>
        public void Start()
        {
            Thread oThread = new Thread(new ThreadStart(this.ThreadBody));
            oThread.Start();
        }
        /// <summary>
        /// When gateway is ready: publishes its MAC address to the broker.
        /// Every 5 seconds: publishes temperature information to the broker.
        /// </summary>
        public void ThreadBody()
        {
            //Send a  message containing the MAC-address of the device
            Dictionary<string, string> messageProperties = new Dictionary<string, string>();
            messageProperties.Add("source", "simdevice");
            messageProperties.Add("type", "macaddress");
            Message macAddressMessage = new Message("MAC Address: " + this.configuration,
                messageProperties);
            this.broker.Publish(macAddressMessage);

            double avgTemperature = 10.0;
            double addTemp = 0.0;
            double maxTemp = 40.0;

            //Send a message with temperature information every 10 seconds.
            while (true)
            {
                if (avgTemperature + addTemp > maxTemp)
                {
                    addTemp = 0.0;
                }
                string tempString = (avgTemperature + addTemp).ToString();
                messageProperties["type"] = "temperaturedata";
                Message temperatureMessage = new Message("SimDevice Data: " + tempString + " Kelvin", 
                    messageProperties);
                this.broker.Publish(temperatureMessage);

                addTemp += 1.0;
                
                Thread.Sleep(10000);                
            }
        }
    }
}
