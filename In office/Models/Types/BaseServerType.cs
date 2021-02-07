using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    public abstract class BaseServerType 
    {
        [JsonProperty("id")]
        public long ID { get; set; }
    }
}
