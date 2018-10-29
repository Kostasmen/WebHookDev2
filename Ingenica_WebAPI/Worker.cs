using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace Ingenica_WebAPI
{    
    public interface IWorker
    {
        void DoWork(object anObject);
    }

    public enum WorkerState
    {
        Starting = 0,
        Started,
        Stopping,
        Stopped,
        Faulted
    }

    public class Worker : IWorker
    {
        public WorkerState State { get; set; }        
        public virtual void DoWork(object obj)
        {
            NAVWSControl NS = new NAVWSControl();            
            while (!_shouldStop)
            {                   
               if (NS.Ping()) {
                    DoWork2();
                }
               else
                {
                    Log.WriteLog("Endpoint is unavailable, re-trying in 1 minute...\n" +
                        WebConfigurationManager.AppSettings["WebServiceUrl"],EventLogEntryType.Error);
                }
                Thread.Sleep(60*1000);
            }            
        }

        public virtual void DoWork2()
        {            
            NAVWSControl NS = new NAVWSControl();
            DirectoryInfo info;
            FileInfo[] files;
            int NoOfDays;
            while (!_shouldStop)
            {
                try
                {
                    info = new DirectoryInfo(WebConfigurationManager.AppSettings["FileLocation"]);
                    files = info.GetFiles("*.xml").OrderBy(p => p.Name).ToArray();

                    if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ProcessedFileLocation"]) &&
                        !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["KeepInProcessed"]))
                    {
                        if (Int32.TryParse(WebConfigurationManager.AppSettings["KeepInProcessed"], out NoOfDays))
                        {
                            Directory.GetFiles(WebConfigurationManager.AppSettings["ProcessedFileLocation"])
                                     .Select(f => new FileInfo(f))
                                     .Where(f => f.CreationTime < DateTime.Now.AddDays(-NoOfDays))
                                     .ToList()
                                     .ForEach(f => f.Delete());
                        }
                    }
                }
                catch
                {
                    Log.WriteLog("Folder unavailable or dont' have access. \n" +
                                 WebConfigurationManager.AppSettings["FileLocation"], EventLogEntryType.Error);
                    return;
                }
                
                foreach (FileInfo file in files)
                {
                    XmlDocument doc = new XmlDocument();
                    if (!IsFileLocked(file))
                    {
                        try
                        {
                            doc.Load(file.FullName);  
                        
                            NS.ProcessRequest(doc.OuterXml);
                            //if (!NS.sentOk)
                            //  {
                                // The NAV WS is unavailable - back to DoWork() where Ping is executed until back online.                            
                            //    return;
                            //  }
                            //if (NS.returnedOk)
                            if (NS.sentOk)
                            {                            
                                MoveToProcessed(file.DirectoryName, file.Name);
                            } 
                            else
                            {                            
                                MoveToFailed(file.DirectoryName, file.Name, NS.GetResponseMessage());
                            }
                        }
                        catch (Exception e)
                        {
                            MoveToFailed(file.DirectoryName, file.Name, NS.GetResponseMessage());
                        }
                    }                    
                }
                Thread.Sleep(1000);
            }                            
        }

        public void RequestStop()
        {
            State = WorkerState.Stopping;
            _shouldStop = true;
        }
        public void MoveToProcessed(string path, string fileName)
        {
            string processedLocation = WebConfigurationManager.AppSettings["ProcessedFileLocation"];
            try
            {
                if (!string.IsNullOrEmpty(processedLocation))
                {                   
                    if (!Directory.Exists(processedLocation))
                    {
                        Directory.CreateDirectory(processedLocation);
                    }

                    File.Move(path + @"\" + fileName, processedLocation + @"\" + fileName);

                }
                else
                {
                    File.Delete(path + fileName);
                }
            }
            catch (Exception e)
            {                
                Log.WriteLog(e.Message, EventLogEntryType.Warning);                
            }
        }

        public void MoveToFailed(string path, string fileName, string errorMessage)
        {
            string failedLocation = WebConfigurationManager.AppSettings["FailedFileLocation"];
            try
            {
                if (!string.IsNullOrEmpty(failedLocation))
                {                    
                    if (!Directory.Exists(failedLocation))
                    {
                        Directory.CreateDirectory(failedLocation);
                    }

                    File.Move(path + @"\" + fileName, failedLocation + @"\" + fileName);

                    Log.WriteLog("The NAV Webservice has returned an error for the following file: " + fileName +
                                 "\nError message: " + errorMessage +
                                 "\nThe file has been skipped and moved to " + failedLocation + @"\" + fileName,
                                 EventLogEntryType.Warning);

                }
            }
            catch (Exception e)
            {
                Log.WriteLog(e.Message, EventLogEntryType.Warning);
            }
        }

        public static bool IsFileLocked(FileInfo file)
        {
            // to avoid picked up the file which is just being written by the api
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        protected volatile bool _shouldStop;        
        protected Logger Log = new Logger();
    }
}