using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using System.IO;
using System.Threading;

namespace DotNetModules
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
            logFilePath = logDirectory + this.configuration;
            WriteToLog("Start of log");
        }

        public void Destroy()
        {
            Console.WriteLine("This is DotNetLoggerModule.Destroy()!");
        }

        public void Receive(Message received_message)
        {
            createLogDirectory();
            try
            {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                string content = Encoding.UTF8.GetString(received_message.Content, 0,
                    received_message.Content.Length);
                string source = received_message.Properties["source"];
                writer.WriteLine("Start of message");
                writer.WriteLine("Source: " + source);
                writer.WriteLine("Content: " + content);
                writer.WriteLine("End of message");
                writer.WriteLine();
            }
            }
            catch (IOException e)
            {
                Console.WriteLine("Unable to write to file: " + logFilePath);
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void WriteToLog(String text)
        {
            createLogDirectory();
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(text);
                    writer.WriteLine("Time: " + DateTime.UtcNow);
                    writer.WriteLine();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Unable to write to file: " + logFilePath);
                Console.WriteLine(e.Message);                
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void createLogDirectory()
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
    }
}
