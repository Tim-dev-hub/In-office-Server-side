using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace In_office.Models.Types
{
    public class VerificationRequest
    {
        public User User { get; set; }
        public string EMail { get; set; }
        public string Code { get; set; }

        public VerificationRequest() { }
        public VerificationRequest(string mail, string code, User user)
        {
            EMail = mail;
            Code = code;
            User = user;
        }

        public static bool operator ==(VerificationRequest obj0, VerificationRequest obj1)
        {
            if (obj0 is null && obj1 is null)
                return true;
            else if (!(obj0 is null) && obj1 is null || !(obj1 is null) && obj0 is null)
                return false;

            return obj0.Code == obj1.Code && obj0.EMail == obj1.EMail && obj1.User == obj0.User;
        }

        public static bool operator !=(VerificationRequest obj0, VerificationRequest obj1)
        {
            if (obj0 is null && obj1 is null)
                return false;
            else if (!(obj0 is null) && obj1 is null || !(obj1 is null) && obj0 is null)
                return true;

            return obj0.Code != obj1.Code && obj0.EMail != obj1.EMail && obj1.User != obj0.User;
        }
    }
}
