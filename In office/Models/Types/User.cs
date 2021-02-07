using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{

    public class User : Data
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("surname")]
        public string Surname { get; set; }
        [JsonProperty("nickname")]
        public string Nickname{ get; set; }
        [JsonProperty("phone number")]
        public string PhoneNumber { internal get; set; }
    }
}
