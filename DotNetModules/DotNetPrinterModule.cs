﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;

namespace DotNetModules
{
    public class DotNetPrinterModule : IGatewayModule
    {
        private String configuration;

        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
            Console.WriteLine("Printer configuration: " + this.configuration);
        }

        public void Destroy()
        {
            Console.WriteLine("This is DotNetPrinterModule.Destroy()!");         
        }

        public void Receive(Message received_message)
        {
            if (received_message.Properties["source"] == "simdevice")
            {
                Console.WriteLine("Printer Module received message from simulated device. Content " +
                    Encoding.UTF8.GetString(received_message.Content, 0, 
                    received_message.Content.Length));
            }
        }
    }
}
