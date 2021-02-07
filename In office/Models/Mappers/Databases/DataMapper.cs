using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using In_office.Models.Types;

namespace In_office.Models.Data.Mappers
{
    public class DataMapper<T> : IAsyncMapper<T> where T : BaseServerType, new() 
    {
        private const string CreateStringFormat = "CREATE TABLE $NAME$ ( $SCHEMA$ )";
        private const string AddElementStringFormat = "INSERT INTO $NAME$ ($COLUMNS_NAMES$) VALUES($VALUES$)";
        private const string GetElementStringFormat = "SELECT * FROM $NAME$ WHERE $CONDITION$";

        private Type DataObjectType;
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
            //TODO: добавь другие типы, если понадобится 
        };

        public DataMapper(string Name)
        {
            this.Name = Name;
            this.DataObjectType = typeof(T);
            List<SQLProperty> properties = new List<SQLProperty>();

            var props = typeof(T).GetProperties();

            foreach (var property in props)
            {
                SQLProperty.DataType propType;

                if (_sqlTypes.TryGetValue(property.PropertyType, out propType))
                {
                    var name = property.Name;
                    var primaryKey = (property.Name == "ID" ? true : false);

                    properties.Add(new SQLProperty(propType, name, primaryKey));
                }
            }

            this.Properties = properties.ToList();


            string path = Disk.Root + @"\" + Name + ".sqlite";
            this._path = path;

            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
                EnterCommandNonQuaryAsync(GenerateCreateString(Name, this.Properties));
            }
        }

        private async Task EnterCommandNonQuaryAsync(string command)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _path))
            {
                await conn.OpenAsync();
                SQLiteCommand comm = new SQLiteCommand(command);
                comm.Connection = conn;
                await comm.ExecuteReaderAsync();//ExecuteNonQuery();
                await conn.CloseAsync();
            }
        }

        private async Task<object> ExecuteReaderAsync(string command)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + _path))
            {
                SQLiteCommand comm = new SQLiteCommand(command);
                comm.Connection = connection;
                await connection.OpenAsync();// Open();

                SQLiteDataReader reader = await comm.ExecuteReaderAsync() as SQLiteDataReader;//ExecuteReader();
                await reader.ReadAsync();//Read();

                var instance = new T();
                var type = instance.GetType();

                var props = DataObjectType.GetProperties();
                for (int i = 0; i < DataObjectType.GetProperties().Length - 1; i++)
                {
                    //А почему рот в рефлексии? 
                    //не верю что оно заработает...
                    var prop = type.GetProperty(DataObjectType.GetProperties()[i].Name);
                    var value = reader.GetValue(i);
                    prop.SetValue(instance, value);
                }

                await reader.CloseAsync();
                await connection.CloseAsync();

                return instance;
            }
        }


        public async Task<object> GetAsync(long id)
        {
            var command = GetElementStringFormat;
            command = command.Replace("$NAME$", Name);
            command = command.Replace("$CONDITION$", "ID = " + id);

            return await ExecuteReaderAsync(command);
        }

        public async Task SaveAsync(T obj)
        {
            var command = AddElementStringFormat;
            command = command.Replace("$NAME$", Name);
            command = command.Replace("$COLUMNS_NAMES$", GenerateColumnsNames(Properties));
            command = command.Replace("$VALUES$", GenerateValues(obj));

            await  EnterCommandNonQuaryAsync(command);
        }

        private static string GenerateValues(T obj)
        {
            var props = obj.GetType().GetProperties();

            string str = "";

            for (int i = 0; i < props.Length; i++)
            {
                bool typeIsString = props[i].PropertyType == typeof(string);

                str += (typeIsString ? "'" : "") + props[i].GetValue(obj) + (typeIsString ? "'" : "") + (i == props.Length - 1 ? "" : ",");
            }

            return str;
        }

        private static string GenerateCreateString(string name, List<SQLProperty> properties)
        {
            string schema = "";

            for (int i = 0; i < properties.Count; i++)
            {
                schema += " " + properties[i].Name + " " + properties[i].Type.ToString() + " " + (properties[i].PRIMARY_KEY ? "PRIMARY KEY" : "") + (i == properties.Count - 1 ? "" : ",");
            }

            return CreateStringFormat.Replace("$NAME$", name).Replace("$SCHEMA$", schema); ;
        }

        private static string GenerateColumnsNames(List<SQLProperty> propertyes)
        {
            string result = "";

            for (int i = 0; i < propertyes.Count; i++)
            {
                result += propertyes[i].Name + (i == propertyes.Count - 1 ? "" : ",");
            }

            return result;
        }

        public Task ChangeAsync(T original, T alternative)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(T deletable)
        {
            throw new NotImplementedException();
        }
    }
}
