using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using System.IO;
using System.Threading;

namespace DotNetLoggerModule
{
    /*
     * This module logs all the received traffic. 
     * The module has no filtering, so it logs everything into a file. 
     * The file contains a JSON object. (Currently flat text)
     * THe JSON object is an array of individual JSON values. 
     * There are 2 types of such JSON values: markers for being/end of logging 
     * and effective log data.
     */
    public class DotNetLoggerModule : IGatewayModule
    {
        private const String logDirectory = @"c:\temp\";
        private String logFilePath; 
        private String configuration;

        //configuration contains the filepath to log to
        public void Create(Broker broker, byte[] configuration)
        {
            this.configuration = Encoding.UTF8.GetString(configuration);
            logFilePath= logDirectory + configuration;
        }

        public void Destroy()
        {
            Console.WriteLine("This is DotNetLoggerModule.Destroy()!");
        }

        public void Receive(Message received_message)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath);
            }
            using (StreamWriter writer = new StreamWriter(logDirectory + this.configuration, true))
            {
                string content = Encoding.UTF8.GetString(received_message.Content, 0,
                    received_message.Content.Length);
                string source = received_message.Properties["source"];
                writer.WriteLine("Start");
                writer.WriteLine("Source: " + source);
                writer.WriteLine("Content: " + content);
                writer.WriteLine("Stop");
            }
        }        
    }
}
