using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.Http;


namespace Ingenica_WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            CheckConfig();

            // Web API routes
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Worker w = new Worker();
            ThreadStart ths = new ThreadStart(() => w.DoWork(config));
            Thread th = new Thread(ths);
            th.Start();

        }

        private static void CheckConfig()
        {
            CheckValue("WebServiceUser");
            CheckValue("WebServiceAccessKey");
            CheckValue("WebServiceUrl");
            CheckValue("FileLocation");
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["KeepInProcessed"]) &&
               (WebConfigurationManager.AppSettings["KeepInProcessed"] != "0"))
            {
                CheckValue("ProcessedFileLocation");
            }
            CheckValue("FailedFileLocation");
            CheckValue("ApplicationEventLogSourceId");


        }

        private static void CheckValue(string value)
        {
            if (string.IsNullOrEmpty(WebConfigurationManager.AppSettings[value]))
            {
                Logger l = new Logger();                
                l.WriteLog("Missing [" + value + "] value in configuration.",System.Diagnostics.EventLogEntryType.Error);                
                throw CreateMissingSettingException(value);

            }
        }

        private static Exception CreateMissingSettingException(string name)
        {
            return new ConfigurationErrorsException(name);
        }

    }
}
