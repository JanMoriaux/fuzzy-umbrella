using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace DotNetModules
{
    //A simulated device module
    public class DotNetSimulatedDevice : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private String configuration;

        //configuration used for mapping to an IoT hub device identity (DeviceId/DeviceKey)
        //using real devices, the module would receive this (MAC)address from the device
        /// <summary>
        /// Invoked by the Module Host to create a new DotNetSimulatedDevice instance.
        /// </summary>
        /// <param name="broker">The Broker-instance that brokers messages to/from
        /// other modules in the gateway
        /// </param>        
        /// <param name="configuration">contains a physical address for the 
        /// device
        /// </param>
        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
            this.broker = broker;
            //Console.WriteLine("Simulated device created: " + this.configuration);
        }        
        /// <summary>
        /// Invoked by the Module Host to dispose of the DotNetSimulatedDevice instance.        
        /// </summary>
        public void Destroy()
        {
            Console.WriteLine("This is DotNetSimulatedDevice.destroy()!");
        }
        //The DotNetSimulatedDevice receives a message from the broker.
        //A filter is applied to only accept c2d messages redirected
        //through the mapper module.
        //An acknowledgment message is sent back to the cloud.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="received_message"></param>
        public void Receive(Message received_message)
        {
            if (received_message.Properties["source"] == "dotnetmapper" &&
                received_message.Properties["macAddress"] == this.configuration &&
                received_message.Properties["type"] == "c2d")
            {
                //sends a message to the mapper that cloud info has been received   
                String receivedContent = Encoding.UTF8.GetString(received_message.Content, 0,
                    received_message.Content.Length);                            
                String msgContent = this.CreateJsonString(receivedContent + " returns");
                Message ackMessage = new Message(msgContent, CreateMessageProperties());
                this.Publish(ackMessage);                
            }       
        }
        /// <summary>
        /// Creates and starts a new worker thread for publishing messages to the broker.
        /// </summary>
        public void Start()
        {
            Thread oThread = new Thread(new ThreadStart(this.ThreadBody));
            oThread.Start();
        }
        /// <summary>        
        /// Publishes temperature info to the broker every 10 s.
        /// </summary>
        public void ThreadBody()
        {                        
            //Send a message with temperature information every 10 seconds.
            double avgTemperature = 10.0;
            double addTemp = 0.0;
            double maxTemp = 40.0;
            while (true)
            {
                if (avgTemperature + addTemp > maxTemp)
                {
                    addTemp = 0.0;
                }
                string tempString = (avgTemperature + addTemp).ToString();                
                String msgContent = this.CreateJsonString(tempString + "°C");
                Message temperatureMessage = new Message(msgContent, 
                    CreateMessageProperties());

                this.Publish(temperatureMessage);

                addTemp += 1.0;

                Thread.Sleep(10000);                
            }
        }
        //set messageproperties according to the filter defined in 
        //the Receive()-method of the mapper-module to send messages to cloud           
        private Dictionary<string,string> CreateMessageProperties()
        {
            Dictionary<string, string> messageProperties = new Dictionary<string, string>();
            messageProperties["source"] = "simdevice";
            messageProperties["type"] = "d2c";
            messageProperties["macAddress"] = this.configuration;
            return messageProperties;
        }        
        private String CreateJsonString(string content)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                  
                writer.WriteStartArray();
                writer.WriteStartObject();
                writer.WritePropertyName("device");
                writer.WriteValue(this.configuration);
                writer.WriteEndObject();
                writer.WriteStartObject();
                writer.WritePropertyName("content");
                writer.WriteValue(content);
                writer.WriteEndObject();
                writer.WriteEnd();
            }
            return sb.ToString();
        }
        /// <summary>
        /// Publishes messages to the broker
        /// </summary>
        /// <param name="msg">The Message instance to publish</param>
        private void Publish(Message msg)
        {
            Console.WriteLine(Encoding.UTF8.GetString(msg.Content));
            try
            {
                this.broker.Publish(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine("Device {0} encountered a problem publishing to the broker",
                    this.configuration);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
