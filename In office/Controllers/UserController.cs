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
            var user = ReadUser(HttpContext);

            if (!await _database.Contain("PhoneNumber", user.Result.PhoneNumber))
            {
                Response.StatusCode = 401;
                return "This phone number already used";
            }

            var responce = JsonConvert.SerializeObject(_database.SaveAsync(user.Result));
            return responce;
        }

        [HttpPut("/Users/{id}")]
        public async Task<string> Change(long id)
        {
            var user = await ReadUser(HttpContext);

            await _database.ChangeAsync(id, user);
            return "Status: 200";
        }

        [HttpDelete("/Users/{id}")]
        public async Task<string> Delete(long id)
        {
            var user = await ReadUser(HttpContext);

            await _database.DeleteAsync(user);
            return "Status: 200";
        }

        public async Task<User> ReadUser(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string body;

            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                 body = await stream.ReadToEndAsync();
            }
            return JsonConvert.DeserializeObject<User>(body);
        }
    }
}
