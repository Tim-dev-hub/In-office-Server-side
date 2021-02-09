using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    /// <summary>
    /// Базовый тип любых данных, хранимых на сервере.
    /// </summary>
    public abstract class Data
    {
        /// <summary>
        /// Уникальное ID объекта
        /// </summary>
        [JsonProperty("id")]
        public long ID { get; set; }


        /// <summary>
        /// Токен, который имееть лишь владелец аккаунта и сервер. 
        /// Буквально означает, что "эти данные мои, я могу их изменять и удалять. Вот ключ, подтверждающий это"
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
