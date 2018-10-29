using Ingenica_WebAPI.DynamicsNAVWS;
using System.Web.Configuration;
using System.Net;
using System;

namespace Ingenica_WebAPI
{
    public class NAVWSControl
    {       
        public WebHookTest DynamicsNAVWS;
        public bool sentOk;
        public bool returnedOk;
        public string responseMessage;
        public NAVWSControl()
        {
            this.DynamicsNAVWS = new WebHookTest();
            DynamicsNAVWS.UseDefaultCredentials = false;            
            DynamicsNAVWS.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["WebServiceUser"], WebConfigurationManager.AppSettings["WebServiceAccessKey"]);
            DynamicsNAVWS.Url = WebConfigurationManager.AppSettings["WebServiceUrl"];                        
        }

        public bool SentOk()
        {
            return this.sentOk;
        }

        public bool ReturnedOk()
        {
            return this.returnedOk;
        }

        public string GetResponseMessage()
        {
            return this.responseMessage;
        }

        public void ProcessRequest(string xml)
        {
            this.responseMessage = "";
            try
            {
                this.DynamicsNAVWS.ProcessRequest(xml, ref this.responseMessage);
                this.sentOk = true;
            }
            catch(Exception e)
            {
                this.sentOk = false;
            }
            this.returnedOk = (this.responseMessage == "");
        }

        public bool Ping()
        {            
            try
            {
                this.DynamicsNAVWS.Ping();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}