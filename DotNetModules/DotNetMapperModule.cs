using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Azure.IoT.Gateway;
using Newtonsoft.Json;
using DotNetModules.Models;
using System.Data;

namespace DotNetModules
{
    //todo:    
    /*load devices on module create
     * mapper receives mac addresses from connected devices
     * mapper receives deviceidentities from IoT Hub module
     * 
     */
    public class DotNetMapperModule : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private String configuration;
        private Dictionary<string, DeviceIdentity> devices;

        public Dictionary<string, DeviceIdentity> Devices { get; }

        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
            this.broker = broker;
            Console.WriteLine("Mapper configuration: " + this.configuration);
            //this.LoadDevices();
            //foreach (var device in this.Devices)
            //{
            //    Console.WriteLine(device.Key + "\t" + device.Value.DeviceId + "\t" + device.Value.DeviceKey);
            //}
            //Console.WriteLine();
        }

        public void Destroy()
        {
            Console.WriteLine("OtherMapperModule.Destroy()!");
        }

        public void Receive(Message received_message)
        {
            if (received_message.Properties["source"] == "simdevice" &&
                received_message.Properties["type"] == "macaddress")
            {
                Console.WriteLine("Other mapper receives message from simdevice: " +
                    Encoding.UTF8.GetString(received_message.Content), 0, received_message.Content.Length);
            }
        }

        public void Start()
        {
            Thread oThread = new Thread(new ThreadStart(ThreadBody));
            oThread.Start();
        }
        private void ThreadBody()
        {
            //try
            //{
            //    this.LoadDevices();
            //    foreach (var device in this.Devices)
            //    {
            //        Console.WriteLine(device.Key + "\t" + device.Value.DeviceId + "\t" + device.Value.DeviceKey);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
            Dictionary<string, string> msgProperties = new Dictionary<string, string>();
            msgProperties["source"] = "mappermodule";
            Message msg = new Message("Mappings:\n" + this.configuration, msgProperties);
            broker.Publish(msg);
        }
        //private void LoadDevices()
        //{
        //    this.devices = new Dictionary<string, DeviceIdentity>();
        //    DataSet configSet = JsonConvert.DeserializeObject<DataSet>(this.configuration.Substring(1));
        //    DataTable deviceTable = configSet.Tables["devices"];
        //    foreach (DataRow row in deviceTable.Rows)
        //    {
        //        devices[row["macAddress"].ToString()] = new DeviceIdentity
        //        {
        //            DeviceKey = row["deviceKey"].ToString(),
        //            DeviceId = row["deviceId"].ToString()
        //        };
        //    }
        //}
    }
}
