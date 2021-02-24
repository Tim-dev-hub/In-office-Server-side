using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace In_office.Models
{

    public static class Mail
    {
        static MailAddress ServerMail = new MailAddress("InOfficeMessanger@gmail.com");

        static SmtpClient Client = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(ServerMail.Address, "no, no, no, fuck you")
        };
        public static void Send(string mail, string subject, string message)
        {
            Client.Send(new MailMessage(ServerMail.Address, mail, subject, message));
        }

        public static string GenerateCode(int count)
        {
            string code = "";
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                code += random.Next().ToString()[0];
            }

            return code;
        }
    }
}
