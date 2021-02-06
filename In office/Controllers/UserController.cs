using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace In_office.Controllers
{
    public class UserController : Controller
    {
        [HttpGet("/Users/{id}")]
        public string Get(long id)
        {
            //TODO: Realization database read
            return "Status: 200 \n*here your user with id "+id+"*";
        }


        /// <summary>
        /// This method used for init new user. 
        /// DONT TRY INVOKE THIS FROM SERVER CODE THITS ONLY INTERFACE FOR HTTP. 
        /// </summary>
        [HttpPost("/Users")]
        
        public string Write()
        {
            //TODO: Realization database write
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
