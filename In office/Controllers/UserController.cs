using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using In_office.Models.Types;

namespace In_office.Controllers
{
    public class UserController : Controller
    {
        [HttpGet("/Users/{id}")]
        public async Task<string> Get(long id)
        {
            var database = new Models.Data.Mappers.DataMapper<User>("UsersExample");
            return JsonConvert.SerializeObject(await database.GetAsync(id));
        }


        /// <summary>
        /// This method used for init new user. 
        /// DONT TRY INVOKE THIS FROM SERVER CODE THITS ONLY INTERFACE FOR HTTP. 
        /// </summary>
        [HttpPost("/Users")]
        public async Task<string> Write()
        {
            var database = new Models.Data.Mappers.DataMapper<User>("UsersExample");
            User user;
            string body;

            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                body = stream.ReadToEnd();
            }

            user = JsonConvert.DeserializeObject<User>(body);


            await database.SaveAsync(user);

            return "Status: 200";
        }

        [HttpPut("/Users/{id}")]
        public string Change(long id)
        {
            //TODO: Realization database REwrite
            return "Status: 200";
        }

        [HttpDelete("/Users/{id}")]
        public string Delete(long id)
        {
            //TODO: Realization database user delete
            return "Status: 200";
        }
    }
}
