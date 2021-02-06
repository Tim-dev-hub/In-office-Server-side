using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    public class Media
    {
        [JsonProperty("id")]
        public long ID;
        [NonSerialized] //Path relative server 
        public string Path;
        [JsonProperty("size")]
        public string Size;
        [JsonProperty("Name")]
        public string Name;
    }
}
