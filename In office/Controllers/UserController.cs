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
        private  Database<User> _database = new Database<User>("Users");
        private static List<VerificationRequest> _verificationRequests = new List<VerificationRequest>();

        
        [HttpPut("/Users/{id}/Password/")]
        public async Task<string> ChangePassword(long id)
        {
            //IDEA: Сделать настройку подтвреждения смены пароля через почту.
            var user = await ReadUser();
            
            if(await _database.IsDataOwnership(user.Token, id))
            {
                var dbUser = await _database.GetAsync(id);
                dbUser.Password = user.Password;
                dbUser.Token = In_office.Models.Serucity.Encryption.Generate512ByteKey();
                await _database.ChangeAsync(id, dbUser);
                Response.StatusCode = 200;
                return dbUser.Token;
            }

            Response.StatusCode = 403;
            return "Указанный токен не принаддежит указанному айди";
        }

        /// <summary>
        /// Получает общедоступные данные о пользователе
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/Users/{id}")]
        public async Task<string> Get(long id)
        {
            var user = await _database.GetAsync(id);
            user.E_mail = String.Empty;
            user.Password = String.Empty;
            user.Token = String.Empty;

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

        /// <summary>
        /// Создаёт новый аккаунт. Требует верификации
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Users")]
        public async Task<string> Write()
        {
            var user = await ReadUser();
            
            string coms;
            if(!In_office.Models.Serucity.Encryption.ValidPassword(user.Password, out coms))
            {
                Response.StatusCode = 418;
                return coms;
            }


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

        /// <summary>
        /// Верифицирует почту при создании аккаунта
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
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
                Response.StatusCode = 201;
                _verificationRequests.Remove(verificationRequest);
                return responce;
            }
            else
            {
                Response.StatusCode = 403;
                return "Код был введён неправильно, пройдите регистрацию ещё раз";
            }
        }

        /// <summary>
        /// Авторизует пользователя и возвращает токен клиенту
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("/Users/{email}")]
        public async Task<string> Autorize(string email)
        {
            string password = new StreamReader(HttpContext.Request.Body).ReadToEnd();
            if(await _database.Contain("E_mail", email) && await _database.Contain("Password", password))
            {
                Response.StatusCode = 200;
                var user = await _database.GetObjectByPropertyAsync("E_mail", email);
                return user.Token;
            }

            Response.StatusCode = 406;
            return "Указанный пароль не подходит";
        }

        /// <summary>
        /// Изменяет данные о пользователе
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("/Users/{id}")]
        public async Task<string> Change(long id)
        {
            var original = await _database.GetAsync(id);
            var user = await ReadUser();

            if(user.Password != original.Password)
            {
                return "Если ты хочешь поменять пароль используй PUT запрос по адрессу /Users/{id}/Password/";
            }

            user.ID = id;
            user.Token = original.Token;
            

            //Поскольку API будет открытым для использования, 
            //стоит позаботься о безопасности
            //так для изменения любых данных нужно подтверждение о их владении.
            //Сделать это можно предоставив ID пользователя-автора и 512-байтовый HEX ключ, 
            //которые выдаются пользователю при регистрации и каждой авторизации аккаунта.
            //Ключ будет менятся при смене пароля

            //upd: У такого способа есть минус 
            //      -Невозможно отследить всех людей у которых есть ключ
            //      и плюс
            //      -Невозможно отследить всех людей у которых есть ключ

            // -Что делать если кто-то со стороны узнал токен?
            // -Поменять пароль, токен будет менятся вместе с ним.

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

        /// <summary>
        /// Удаляет запись о пользователе 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("/Users/{id}")]
        public async Task<string> Delete(long id)
        {
            var user = await ReadUser();

            if (await _database.IsDataOwnership(user.Token, user.ID))
            {
                await _database.DeleteAsync(user);
                Response.StatusCode = 200;
                return "-No, you can't just delete my personal data\n" +
                       "-hahah SQL tabel make wrum-wrum.";
            }
            else
            {
                Response.StatusCode = 403;
                return "No https://i.kym-cdn.com/photos/images/newsfeed/001/506/783/c3c.jpg";
            }
        }

        private async Task<User> ReadUser()
        {
            string body;

            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                 body = await stream.ReadToEndAsync();
            }
            return JsonConvert.DeserializeObject<User>(body);
        }

        private async Task<string> Read()
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
