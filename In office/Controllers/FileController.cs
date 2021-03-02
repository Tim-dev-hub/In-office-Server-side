using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using In_office.Models.Data.Mappers;
using In_office.Models.Types;
using System.IO;
using Newtonsoft.Json;

namespace In_office.Controllers
{
    public class FileController : Controller
    {
        private static Database<In_office.Models.Types.File> _database = new Database<In_office.Models.Types.File>("Files");

        [RequestSizeLimit(Int32.MaxValue/8)]
        [HttpPost("/Files/name={name};token={token}")]
        public async Task<string> DownLoad(string name, string token)
        {
            In_office.Models.Types.File file = new Models.Types.File();
            file.ID = await _database.GetRandomID();
            file.Path = Disk.Root + "/Files/" + file.ID.ToString() + "_" + name;
            file.Name = name;
            file.Token = token;
         

            FileStream fileStream = new FileStream(file.Path, FileMode.Create);

            byte[] data = new byte[Int32.MaxValue/8];
            int length = -1;
            while (length != 0)
            {
                length = await Request.Body.ReadAsync(data, 0, data.Length);
                await fileStream.WriteAsync(data, 0, length);
            }

            file.Size = fileStream.Length;
            Console.WriteLine(file.Size);

            fileStream.Close();

            await _database.SaveAsync(file, file.ID);
            return file.ID.ToString();
        }


        [HttpGet("/Files/{id}/Details/")]
        public async Task<string> GetDetails(int id)
        {
            In_office.Models.Types.File file;
            try
            {
                file = await _database.GetAsync(id);

                if (file == null)
                {
                    throw new Exception();
                }
            }
            catch
            {
                Response.StatusCode = 404;
                return "";
            }

            return JsonConvert.SerializeObject(file);
        }
        [HttpGet("/Files/{id}")]
        public async Task<IActionResult> UpLoad(long id)
        {
            var file = await _database.GetAsync(id);
            var filestream = new FileStream(file.Path, FileMode.Open);
            byte[] data = new byte[(int)filestream.Length];
            await filestream.ReadAsync(data, 0, (int)file.Size);
            filestream.Close();

            Response.Headers.Add("charset", "utf-8");
            return File(data, "application/octet-stream;", file.Name);
        }
    }
}
