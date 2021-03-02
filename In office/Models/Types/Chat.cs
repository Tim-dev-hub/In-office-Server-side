using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    public class Chat : Data
    {
        public enum ChatType
        {
            Private,
            Channel,
            Open
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("members")]
        public User[] Members { get; set; } 
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("type")]
        public Chat.ChatType Type { get; set; }

    }
}
