using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Ingenica_WebAPI
{
    public class Logger
    {
        public void WriteLog(string value, EventLogEntryType type)
        {
            string Source;
            string Log;

            Source = WebConfigurationManager.AppSettings["ApplicationEventLogSourceId"];
            Log = "Application";

            try
            {
                if (!EventLog.SourceExists(Source))
                    EventLog.CreateEventSource(Source, Log);
                EventLog.WriteEntry(Source, value, type);
            }
            catch
            {

            }           

        }
    
    }
}