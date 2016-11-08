using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;

namespace DotNetPrinterModule
{
    public class DotNetPrinterModule : IGatewayModule
    {
        private String configuration;

        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
        }

        public void Destroy()
        {            
        }

        public void Receive(Message received_message)
        {
            if (received_message.Properties["source"] == "sensor")
            {
                Console.WriteLine("Printer Module received message from Sensor. Content " +
                    Encoding.UTF8.GetString(received_message.Content, 0, 
                    received_message.Content.Length));
            }
        }
    }
}
