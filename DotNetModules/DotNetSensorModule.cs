using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.IoT.Gateway;

namespace DotNetModules
{
    public class DotNetSensorModule : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private String configuration;

        public void Create(Broker broker, byte[] configuration)
        {
            this.broker = broker;
            this.configuration = Encoding.UTF8.GetString(configuration);
            Console.WriteLine("Sensor configuration: " + this.configuration);
        }

        public void Destroy()
        {
            Console.WriteLine("This is C# Sensor Module Destroy!");
        }

        public void Receive(Message received_message)
        {
            //just ignore the Message, sensor doesn't need to print
        }

        public void Start()
        {
            Thread oThread = new Thread(new ThreadStart(this.threadBody));
            oThread.Start();
        }
        public void threadBody()
        {
            Random r = new Random();
            int n = r.Next();

            while (true)
            {
                Dictionary<string, string> thisIsMyProperty = new Dictionary<string, string>();
                thisIsMyProperty.Add("source", "sensor");

                Message messageToPublish = new Message("SensorData: " + n, thisIsMyProperty);

                this.broker.Publish(messageToPublish);

                //Publish a message every 5 seconds.
                Thread.Sleep(5000);
                n = r.Next();
            }
        }
    }
}
