using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using In_office.Models.Types;

namespace In_office.Models.Data.Mappers
{
    public class DataMapper : IMapper
    {
        private const string CreateStringFormat = "CREATE TABLE [$NAME$] { $SCHEMA$ }";
        private const string AddElementStringFormat = "INSERT INTO [$NAME$] ($COLUMNS_NAMES$) \nVALUES($VALUES$)";

        private string _path { get; set; }
        public string Name { get; private set; }
        public List<SQLProperty> Properties { get; private set; }


        private Dictionary<Type, SQLProperty.DataType> _sqlTypes = new Dictionary<Type, SQLProperty.DataType>()
        {
            { typeof(int), SQLProperty.DataType.INTEGER},
            { typeof(long), SQLProperty.DataType.INTEGER},
            { typeof(string), SQLProperty.DataType.TEXT},
            { typeof(DateTime), SQLProperty.DataType.DATETIME},
            { typeof(TimeSpan), SQLProperty.DataType.TIME},
            { typeof(float), SQLProperty.DataType.REAL }
            //TODO добавь другие типы, если понадобится 
        };

        public DataMapper(string Name, Type type)
        {
            this.Name = Name;

            //Мне кажется писать строки схем для sql вручную это 
            //а) муторно
            //б) в случае модификации типов класса или добавление новых переменных будет долго и нудно
            //в) вероятность накосячить с порядком типов и не заметить это приближается к сотни 
            //
            //поэтому в этом фрагменте проперти(они же свойства) класса преобразовываются в SQLProperty, 
            //которые я в дальнейшем и юзаю для общения с базой 

            List<SQLProperty> properties = new List<SQLProperty>();

            foreach(var property in type.GetProperties())
            {
                SQLProperty.DataType propType;
                
                if(_sqlTypes.TryGetValue(property.PropertyType, out propType))
                {
                    var name = property.Name;
                    var primaryKey = (property.Name == "ID" ? true : false);

                    properties.Add(new SQLProperty(propType, name, primaryKey));
                }
            }            

            this.Properties = properties.ToList();


            string path = Disk.Root + Name + ".db";
            this._path = path;

            if (!File.Exists(path))
            {
                File.Create(path);

                EnterCommandNonQuary(GenerateCreateString(Name, this.Properties));
            }
        }
       
        private void EnterCommandNonQuary(string command) 
        {
            using(SqliteConnection conn = new SqliteConnection("source=" + _path))
            {
                SqliteCommand comm = new SqliteCommand(command);
                comm.Connection = conn;
                comm.ExecuteNonQuery();
            }        
        }

        public User Get(long id)
        {
            throw new NotImplementedException();
        }

        public void Save(User user)
        {
            var command = AddElementStringFormat;
            command = command.Replace("$NAME$", Name);
            command = command.Replace("$COLUMNS_NAMES$", GenerateColumnsNames(Properties));
            command = command.Replace("$VALUES$", user.ID.ToString() + ", " + user.Name.ToString() + ", " + user.Surname + ", " + user.Nickname + ", " + user.PhoneNumber);


            EnterCommandNonQuary(command);
        }

        public void Change(User original, User alternative)
        {
            throw new NotImplementedException();
        }

        public void Delete(User deletable)
        {
            throw new NotImplementedException();
        }


        private static string GenerateCreateString(string name, List<SQLProperty> properties)
        {
            string schema = "";
            foreach (var prop in properties)
            {
                schema += " " + prop.Name + " " + prop.Type.ToString() + " " + (prop.PRIMARY_KEY ? "PRIMARY KEY" : "");
            }


            return CreateStringFormat.Replace("$NAME$", name).Replace("$SCHEMA$", schema); ;
        }
        
        private static string GenerateColumnsNames(List<SQLProperty> propertyes)
        {
            string result = "";

            foreach(var prop in propertyes)
            {
                result += prop.Name + ", ";
            }

            return result;
        }
    }
}
