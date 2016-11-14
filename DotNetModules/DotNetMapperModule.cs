using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using Microsoft.Azure.IoT.Gateway;
using Newtonsoft.Json;
using DotNetModules.Models;
using System.Data;

namespace DotNetModules
{
    /*      
     * 
     * mapper receives deviceidentities from IoT Hub module and automatically maps
     * them to a MAC address
     */
    public class DotNetMapperModule : IGatewayModule
    {
        private Broker broker;
        private String configuration;
        private Dictionary<string, DeviceIdentity> devices;

        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
            this.broker = broker;
            try
            {
                LoadDevices();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void Destroy()
        {
            Console.WriteLine("This isDotNetMapperModule.Destroy()!");
        }

        public void Receive(Message received_message)
        {
            /*
             * D2C messages are filtered for a macAddress property and the absence of
             * a deviceName/deviceKey property or a source property set to "mapping"
             */
            bool macInProps = received_message.Properties.ContainsKey("macAddress");
            bool identityinProps = received_message.Properties.ContainsKey("deviceKey") ||
                received_message.Properties.ContainsKey("deviceName");
            bool sourceIsMapping = received_message.Properties["source"] == "mapping";

            if(macInProps && !identityinProps && !sourceIsMapping)
            {
                //look up the device's deviceKey and DeviceId
                DeviceIdentity deviceIdentity = devices[received_message.Properties["macAddress"]];

                //Create a message to publish to the broker, that will be received by the IoT-Hub module                                
                Message msg = this.CreateMessage(deviceIdentity.DeviceId, deviceIdentity.DeviceKey,
                    received_message);

                //publish to broker
                this.Publish(msg);
            }                                        
            /*
             * C2D messages are filtered for a "source" property set to "iothub" and a deviceName property            
            */
            if (received_message.Properties["source"] == "iothub" &&
                received_message.Properties.ContainsKey("deviceName"))
            {
                Console.WriteLine("Mapper receives message from iothub");
                //look up the MAC-address
                string deviceId = received_message.Properties["deviceName"];
                string macAddress = devices.Keys.Where(p => devices[p].DeviceId == deviceId).
                    Single().ToString();

                //Create a message for the device
                //with the given MAC-address
                Message msg = CreateMessage(macAddress, null,
                    received_message);

                //publish to broker
                this.Publish(msg);
            }
        }       
        private void Publish(Message message)
        {
            try
            {
                broker.Publish(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Mapper module: problem trying to publish to broker");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }        
        private void LoadDevices()
        {
            try
            {
                //initializes a dictionary with key = macaddress and value = deviceidentity key/value
                //from the configuration
                this.devices = new Dictionary<string, DeviceIdentity>();
                DataSet configSet = JsonConvert.DeserializeObject<DataSet>(this.configuration);
                DataTable deviceTable = configSet.Tables["devices"];
                foreach (DataRow row in deviceTable.Rows)
                {
                    devices[row["macAddress"].ToString()] = new DeviceIdentity
                    {
                        DeviceKey = row["deviceKey"].ToString(),
                        DeviceId = row["deviceId"].ToString()
                    };
                }               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        private Message CreateMessage(string deviceName, string deviceKey, Message message)
        {
            Dictionary<string, string> msgProperties = message.Properties;
            if (deviceKey != null)
            {
                //add source and registered identity properties
                msgProperties["source"] = "mapping";
                msgProperties["deviceName"] = deviceName;
                msgProperties["deviceKey"] = deviceKey;
                
                //remove the "macAddress" property
                msgProperties.Remove("macAddress");
            }
            else
            {
                //add source and macaddress properties
                msgProperties["source"] = "mapping2device";
                msgProperties["macAddress"] = deviceName;
                
                //remove the deviceName/deviceKey properties
                msgProperties.Remove("deviceName");
                msgProperties.Remove("deviceKey");
            }
            Message msg = new Message(message.Content, msgProperties);
            return msg;
        }
    }
}
