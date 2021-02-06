using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    public class User
    {
        [JsonProperty("id")]
        public long ID; 
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("surname")]
        public string Surname;
        [JsonProperty("nickname")]
        public string Nickname;
        [NonSerialized] //мы же не собираемся отправлять номер первому встречному, лол 
        public string PhoneNumber; 
    }
}
