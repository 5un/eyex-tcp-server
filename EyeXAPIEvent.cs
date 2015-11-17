using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeXFramework;
using Tobii.EyeX.Framework;
using Tobii.EyeX.Client;

namespace EyeXTcpServer
{
    class EyeXAPIEvent
    {
        public String eventType;
        public JObject data;

        public JObject toJson()
        {

            JObject rootOject = new JObject();

            rootOject["type"] = "event";
            rootOject["event_type"] = this.eventType;

            if (this.data != null)
            {
                rootOject["data"] = data;
            }
            return rootOject;
        }

        public string toJsonString()
        {
            return this.toJson().ToString();
        }

    }
}
