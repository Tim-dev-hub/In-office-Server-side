using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace In_office.Models.Types
{
    public class Media : Data
    {
        /// <summary>
        /// Путь к фото на сервере
        /// </summary>
        [NonSerialized] 
        public string Path;
        /// <summary>
        /// Размер фото
        /// </summary>
        [JsonProperty("size")]
        public int Size;
        /// <summary>
        /// Имя фото, с которым оно было получено на сервер
        /// </summary>
        [JsonProperty("Name")]
        public string Name;
    }
}
