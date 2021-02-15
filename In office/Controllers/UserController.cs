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
using In_office.Models;

namespace In_office.Controllers
{
    public class UserController : Controller
    {
        private  DataMapper<User> _database = new DataMapper<User>("Users");
        private static List<VerificationRequest> _verificationRequests = new List<VerificationRequest>();

        [HttpGet("/Users/{id}")]
        public async Task<string> Get(long id)
        {
            var user = await _database.GetAsync(id);
            user.E_mail = String.Empty;

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

            if (!await _database.Contain("E_mail", user.E_mail))
            {
                Response.StatusCode = 406;
                return "This email already used";
            }

            string code = Mail.GenerateCode(6);
            Mail.Send(user.E_mail, "Verification code", "You verification code : " + code);

            _verificationRequests.Add(new VerificationRequest(user.E_mail, code, user));
            Response.StatusCode = 200;
            return "Ок, ожидай письмо и кинь код на /Users/$EMAIL_ADDRES$/";
        }

        [HttpPost("/Users/{email}")]
        public async Task<string> Verify(string email)
        {
            VerificationRequest verificationRequest = null;
            User mailOwner = null;

            foreach(var request in _verificationRequests)
            {
                if (request.EMail == email)
                {
                    verificationRequest = request;
                    mailOwner = request.User;
                    break;
                }
            }

            if(verificationRequest == null || mailOwner == null)
            {
                Response.StatusCode = 400;
                return "Указанная почта, скорее всего, была подтверждена ранее";
            }

            if(await Read() == verificationRequest.Code)
            {
                var newUserObject = await _database.SaveAsync(mailOwner);

                var responce = JsonConvert.SerializeObject(newUserObject);
                Response.StatusCode = 200;
                _verificationRequests.Remove(verificationRequest);
                return responce;
            }
            else
            {
                Response.StatusCode = 403;
                return "Код был введён неправильно, пройдите регистрацию ещё раз";
            }
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

        public async Task<string> Read()
        {
            string body;

            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                body = await stream.ReadToEndAsync();
            }

            return body;
        }
    }
}
