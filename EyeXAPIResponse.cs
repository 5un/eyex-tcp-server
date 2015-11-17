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
    class EyeXAPIResponse
    {
        public int requestId;
        public int statusCode = 200;
        public Dictionary<string, object> results;

        public JObject toJson()
        {
            
            JObject rootOject = new JObject();

            rootOject["type"] = "response";
            rootOject["requestId"] = requestId;
            rootOject["statuscode"] = this.statusCode;

            if (this.results != null)
            {
                JObject data = new JObject();
                foreach (KeyValuePair<string, object> entry in this.results)
                {
                    JObject val = new JObject();
                    if (entry.Value is Rect)
                    {
                        Rect r = (Rect)entry.Value;
                        val["x"] = r.X;
                        val["y"] = r.Y;
                        val["width"] = r.Width;
                        val["height"] = r.Height;
                    }
                    else if (entry.Value is Size2)
                    {
                        Size2 s = (Size2)entry.Value;
                        val["width"] = s.Width;
                        val["height"] = s.Height;

                    }

                    data[entry.Key] = val;
                }

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
