using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{

    /// <summary>
    /// объект данных пользователя
    /// </summary>
    public class User : Data
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("surname")]
        public string Surname { get; set; }
        [JsonProperty("nickname")]
        public string Nickname{ get; set; }
        [JsonProperty("EMail")]
        public string E_mail { get; set; }

        public static bool operator ==(User user0, User user1)
        {
            if (user0 is null && user1 is null)
                return true;
            else if (!(user0 is null) && user1 is null || !(user1 is null) && user0 is null)
                return false;

            return user0.ID == user1.ID;
        }

        public static bool operator !=(User user0, User user1)
        {
            if (user0 is null && user1 is null)
                return false;
            else if (!(user0 is null) && user1 is null || !(user1 is null) && user0 is null)
                return true;

            return user0.ID != user1.ID;
        }
    }
}
