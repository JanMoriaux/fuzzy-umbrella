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

    /* TODO:     
     * 
     * mapper receives deviceidentities from IoT Hub module and automatically maps
     * them to a MAC address
     */
    public class DotNetMapperModule : IGatewayModule //,IGatewayModuleStart
    {
        private Broker broker;
        private String configuration;
        private Dictionary<string, DeviceIdentity> devices;
        //private Boolean gatewayIsReady = false;


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
            //Console.WriteLine("Mapper configuration: " + this.configuration);
        }

        public void Destroy()
        {
            Console.WriteLine("DotNetMapperModule.Destroy()!");
        }

        public void Receive(Message received_message)
        {
            //Mapper module receives message from device
            //and automatically forwards it to the IoTHub module
            //with device identity information
            if (received_message.Properties["source"] == "simdevice" &&
                received_message.Properties["type"] == "d2c")
            {                
                //look up the device's deviceKey and DeviceId
                DeviceIdentity deviceIdentity = devices[received_message.Properties["macAddress"]];

                //Create a message to publish to the broker, that will be received by the IoT-Hub module                                
                Message msg = this.CreateMessage(deviceIdentity.DeviceId, deviceIdentity.DeviceKey,
                    received_message.Content);

                //publish to broker
                this.Publish(msg);
            }
            //mapper receives message from IoTHub 
            //automatically publishes message to broker
            //aimed at a given device
            if (received_message.Properties["source"] == "iothub" &&
               received_message.Properties["type"] == "c2d")
            {
                //look up the MAC-address
                string deviceId = received_message.Properties["deviceName"];
                string macAddress = devices.Keys.Where(p => devices[p].DeviceId == deviceId).
                    Single().ToString();

                //Create a message for the device
                //with the given MAC-address
                Message msg = CreateMessage(macAddress, null,
                    received_message.Content);

                //publish to broker
                this.Publish(msg);
            }
        }
        //public void Start()
        //{
        //    //Thread oThread = new Thread(new ThreadStart(ThreadBody));
        //    //oThread.Start();
        //    this.gatewayIsReady = true;
        //}
        private Message CreateMessage(string deviceName, string deviceKey, byte[] content)
        {
            Dictionary<string, string> msgProperties = new Dictionary<string, string>();
            msgProperties["source"] = "mapping";
            if (deviceKey != null)
            {
                msgProperties["deviceName"] = deviceName;
                msgProperties["deviceKey"] = deviceKey;
                msgProperties["type"] = "d2c";
            }
            else
            {
                msgProperties["macAddress"] = deviceName;
                msgProperties["type"] = "c2d";
            }
            Message msg = new Message(content, msgProperties);
            return msg;
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

        //private void ThreadBody()
        //{
        //    this.LoadDevices();
        //    Dictionary<string, string> msgProperties = new Dictionary<string, string>();
        //    msgProperties["source"] = "mapping";
        //    Message msg = new Message("Mappings:\n" + this.configuration, msgProperties);
        //    broker.Publish(msg);
        //}
        private void LoadDevices()
        {
            try
            {
                //initializes a dictionary with key = macaddress and value = deviceidentity
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

                //prints the mapped devices to console
                //Console.WriteLine("Devices:\n");
                //foreach (var device in this.devices)
                //{
                //    Console.WriteLine(device.Key + "\t" + device.Value.DeviceId + "\t" + device.Value.DeviceKey);
                //}
                //Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
