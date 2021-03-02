using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    public class File : Data
    {
        [JsonProperty("size")]
        public long Size { get; set; }
        [JsonIgnore]
        public string Path { get; set; }

        public string Name { get; set; }
    }
}
