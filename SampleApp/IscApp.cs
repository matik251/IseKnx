using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using IIscAppLoaderInterfaces;
using log4net;
using Newtonsoft.Json;
using SampleApp.Serial;

namespace SampleApp
{
    public class IscApp : IIscApp
    {
        
        const string apiUrl = @"https://192.168.1.200:1200";

        const string XmlApiEnding = @"/api/KnxTelegrams";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IscApp));

        private IIscAppConnector _appHost;

        private SerialPortUsageExample _serialPortUsageExample;

        void IIscApp.Init(IIscAppConnector iscAppHost)
        {
            // Put code to initialize stuff managed by this app here.
            // No KNX communication is allowed!
            // Return true if the app is initialized and ready to run.
            // Return false if not ready to run.
            Logger.Info("Initializing KNX ISE sender.");

            _appHost = iscAppHost;

            Logger.Info("Initializing done.");
        }

        void IIscApp.Run()
        {
            // Put code to get app running here.
            // If values on the KNX bus should be initialized, do this right at the beginning.
            // Return true if the app is running, false if not.

            Logger.Info("KNX ISE sender running.");
        }

        void IIscApp.Exit()
        {
            // Put code here that needs to be done to shut-down this app.
            Logger.Info("KNX ISE sender exiting.");
        }

        /// <summary>
        ///   Called by the host when an interesting value changed on KNX.
        /// </summary>
        void IIscApp.OnValueReceived(uint coId, object value)
        {
            Logger.Info(string.Format("Received new value for ConnectionObject {0}: {1}", coId, value));

            var newTelegram = new KnxTelegram();
            newTelegram.Timestamp = DateTime.Now;
            newTelegram.TimestampS = newTelegram.ToString();
            newTelegram.Service = "";
            newTelegram.FrameFormat = "";
            newTelegram.RawData = string.Format("{0}{1}", coId, value);
            newTelegram.FileName = "";
            newTelegram.Processed = 0;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(apiUrl + XmlApiEnding);
                request.Method = "POST";
                request.ContentType = "application/json";
                var payload = JsonConvert.SerializeObject(newTelegram);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(payload);
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string responseJson = sr.ReadToEnd();
                }
            }
            catch(Exception e)
            {
                Logger.Info(string.Format("Failed to POST ConnectionObject {0}: {1}", coId, value));
                if (_appHost.IsSdCardPresent)
                {
                    var sdCardPath = _appHost.SdCardPath;
                    WriteSD(newTelegram, _appHost.SdCardPath, DateTime.Now.ToString("MM_dd_yyyy_TP"+".xml"));
                }
            }
        }

        /// <summary>
        ///   Creates or appends xml telegram file.
        /// </summary>
        public void WriteSD(KnxTelegram newTelegram,string sdCardPath, string fileName)
        {
            var file = string.Format("{0}/{1}.xml", sdCardPath, fileName);
            if (!File.Exists(file))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(file, xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("CommunicationLog");
                    xmlWriter.WriteElementString("Telegram", newTelegram.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                XDocument xDocument = XDocument.Load(file);
                XElement root = xDocument.Element("School");
                IEnumerable<XElement> rows = root.Descendants("Student");
                XElement firstRow = rows.First();
                firstRow.AddBeforeSelf(
                   new XElement("CommunicationLog",
                   new XElement("Telegram", newTelegram.ToString())));
                xDocument.Save(file);
            }
        }

        public object GetValue(uint coId)
        {
            throw new NotImplementedException();
        }
    }


    public partial class KnxTelegram
    {
        public long? Tid { get; set; }
        public string TimestampS { get; set; }
        public DateTime Timestamp { get; set; }
        public string Service { get; set; }
        public string FrameFormat { get; set; }
        public string RawData { get; set; }
        public int? RawDataLength { get; set; }
        public string FileName { get; set; }
        public int? Processed { get; set; }
    }
}