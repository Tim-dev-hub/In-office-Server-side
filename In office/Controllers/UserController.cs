using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using In_office.Models.Types;
using In_office.Models.Data.Mappers;

namespace In_office.Controllers
{
    public class UserController : Controller
    {
        private DataMapper<User> _database = new DataMapper<User>("Users");

        [HttpGet("/Users/{id}")]
        public async Task<string> Get(long id)
        {
            var responce = JsonConvert.SerializeObject(await _database.GetAsync(id));

            if (responce != null)
            {
                return responce;
            }
            else
            {
                Response.StatusCode = 404;
                return null;
            }
        }

        [HttpPost("/Users")]
        public async Task<string> Write()
        {
            User user;
            string body;

            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                body = await stream.ReadToEndAsync();
            }
            user = JsonConvert.DeserializeObject<User>(body);

            if(!await _database.Contain("PhoneNumber", user.PhoneNumber))
            {
                Response.StatusCode = 401;
                return "This phone number already used";
            }

            var responce = JsonConvert.SerializeObject(_database.SaveAsync(user));
            return responce;
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
