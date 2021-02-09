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
            var user = await _database.GetAsync(id);
            user.PhoneNumber = String.Empty;

            var responce = JsonConvert.SerializeObject(user);

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
            var user = await ReadUser(HttpContext);
            user.Token = In_office.Models.Serucity.Encryption.Generate512ByteKey();

            if (!await _database.Contain("PhoneNumber", user.PhoneNumber))
            {
                Response.StatusCode = 401;
                return "This phone number already used";
            }

            var newUserObject = await _database.SaveAsync(user);

            var responce = JsonConvert.SerializeObject(newUserObject);
            return responce;
        }

        [HttpPut("/Users/{id}")]
        public async Task<string> Change(long id)
        {
            var user = await ReadUser(HttpContext);
            user.ID = id;

            //Поскольку API будет открытым для использования, 
            //стоит позаботься о безопасности
            //так для изменения любых данных нужно подтверждение о их владении.
            //Сделать это можно предоставив ID пользователя-автора и 512-байтовый HEX ключ, 
            //которые выдаются пользователю при регистрации и каждой авторизации аккаунта.

            //является ли пользователь владельцем аккаунта? 
            if (await _database.IsDataOwnership(user.Token, user.ID))
            {
                await _database.ChangeAsync(id, user);
                Response.StatusCode = 200;
                return "OK, user parametres was modified";
            }
            else
            {
                Response.StatusCode = 403;
                return "This data not available for your modification, you need to be its ♂master♂";
            }
        }

        [HttpDelete("/Users/{id}")]
        public async Task<string> Delete(long id)
        {
            var user = await ReadUser(HttpContext);

            if (await _database.IsDataOwnership(user.Token, user.ID))
            {
                await _database.DeleteAsync(user);
                Response.StatusCode = 200;
                return "OK, user was already delete";
            }
            else
            {
                Response.StatusCode = 403;
                return "-No, you can't just delete my personal data\n" +
                       "-hahah SQL tabel make wrum-wrum.";
            }
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
