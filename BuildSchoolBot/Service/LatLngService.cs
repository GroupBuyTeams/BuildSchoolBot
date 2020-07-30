using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildSchoolBot.Service
{
    public class LatLngService
    {
        public string lat { get; private set; }
        public string lng { get; private set; }
        public LatLngService(string add)
        {
            if (add != null)

            {
                string key = "AIzaSyAlKWP4uWjQIR3WDAWLAu6rUhBfc3_ppag";
                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(add), key);

                WebRequest request = WebRequest.Create(requestUri);
                WebResponse response = request.GetResponse();
                Stream responsestream = response.GetResponseStream();
                XDocument xdoc = XDocument.Load(responsestream);

                XElement result = xdoc.Element("GeocodeResponse").Element("result");
                XElement locationElement = result.Element("geometry").Element("location");

                lat = locationElement.Element("lat").Value;
                lng = locationElement.Element("lng").Value;
            }
            else
            {
                throw new Exception("The address is null, please check.");
            }
        }
    }
}
