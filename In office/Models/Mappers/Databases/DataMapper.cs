using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using In_office.Models.Types;

namespace In_office.Models.Data.Mappers
{
    public class DataMapper<T> : IAsyncMapper<T> where T : Types.Data, new() 
    {
        private const string CreateStringFormat = "CREATE TABLE $NAME$ ( $SCHEMA$ )";
        private const string AddElementStringFormat = "INSERT INTO $NAME$ ($COLUMNS_NAMES$) VALUES($VALUES$)";
        private const string GetElementStringFormat = "SELECT * FROM $NAME$ WHERE $CONDITION$";
        private const string ChangeElementStringFormat = "UPDATE $NAME$ SET $COLUMN AND VALUES$ WHERE $CONDITION$";
        private const string DeleteElementStringFormat = "DELETE FROM $NAME$ WHERE $CONDITION$";
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
        };

        public DataMapper(string Name)
        {
            this.Name = Name;
            List<SQLProperty> properties = new List<SQLProperty>();

            var props = typeof(T).GetProperties();

            for(int i = 0; i < props.Length; i++)
            {
                SQLProperty.DataType propType;

                if (_sqlTypes.TryGetValue(props[i].PropertyType, out propType))
                {
                    var name = props[i].Name;
                    var primaryKey = (props[i].Name == "ID" ? true : false);

                    properties.Add(new SQLProperty(propType, name, primaryKey));
                }
            }

            this.Properties = properties.ToList();


            string path = Disk.Root + @"\" + Name + ".sqlite";
            this._path = path;

            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
                var comm = GenerateCreateString(Name, this.Properties);
                Task.Run(() => EnterCommandNonQuaryAsync(comm));
            }
        }

        private async Task EnterCommandNonQuaryAsync(string command)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _path))
            {
                await conn.OpenAsync();
                SQLiteCommand comm = new SQLiteCommand(command);
                comm.Connection = conn;
                await comm.ExecuteReaderAsync();
                await conn.CloseAsync();
            }
        }

        /// <summary>
        /// Current database contain object with entered value property?  
        /// </summary>
        /// <param name="propertyName">Name of property</param>
        /// <param name="propertyValue">Value of property</param>
        /// <returns>Contain?</returns>
        public async Task<bool> Contain(string propertyName, string propertyValue)
        {
            var result = await ExecuteReaderAsync(GetElementStringFormat.Replace("$NAME$", Name).Replace("$CONDITION$", propertyName + "=" + propertyValue));
            return result == null;
        }

        private async Task<T> ExecuteReaderAsync(string command)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + _path))
            {
                SQLiteCommand comm = new SQLiteCommand(command);
                comm.Connection = connection;
                await connection.OpenAsync();

                SQLiteDataReader reader = await comm.ExecuteReaderAsync() as SQLiteDataReader;
                await reader.ReadAsync();

                if (reader.HasRows)
                {

                    var instance = new T();
                    var type = instance.GetType();

                    for (int i = 0; i < typeof(T).GetProperties().Length - 1; i++)
                    {
                        var prop = type.GetProperty(typeof(T).GetProperties()[i].Name);
                        var value = reader.GetValue(i);
                        prop.SetValue(instance, value);
                    }

                    await reader.CloseAsync();
                    await connection.CloseAsync();

                    return instance;
                }

                return null;
            }
        }

        /// <summary>
        /// find object in database with entered ID
        /// </summary>
        /// <param name="id">Identeficator</param>
        /// <returns>finded object
        /// Can be null, if object doesn't exists</returns>
        public async Task<T> GetAsync(long id)
        {
            var command = GetElementStringFormat;
            command = command.Replace("$NAME$", Name);
            command = command.Replace("$CONDITION$", "ID = " + id);
            
            return await ExecuteReaderAsync(command);
        }

        /// <summary>
        /// Does the user exist with the given key and ID?
        /// </summary>
        /// <param name="key">user token</param>
        /// <param name="pretendentID">him ID</param>
        /// <returns></returns>
        public async Task<bool> IsDataOwnership(string key, long pretendentID)
        {
            if(key == null)
            {
                return false;
            }

            var command = GetElementStringFormat;
            command = command.Replace("$NAME$", this.Name).Replace("$CONDITION$", "Token = '" + key + "' AND ID = " + pretendentID);

            var res = (await ExecuteReaderAsync(command));
            return res != null;
        }

        /// <summary>
        /// Save object in databse
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>return entered object with datanase's id</returns>
        public async Task<T> SaveAsync(T obj)
        {
            obj.ID = await GetID();
            var command = AddElementStringFormat;
            command = command.Replace("$NAME$", Name);
            command = command.Replace("$COLUMNS_NAMES$", GenerateColumnsNames(Properties));
            command = command.Replace("$VALUES$", GenerateValues(obj));

            await  EnterCommandNonQuaryAsync(command);
            return obj;
        }

        /// <summary>
        /// Generate random ID not previously used in current database
        /// </summary>
        /// <returns>random ID</returns>
        private async Task<long> GetID()
        {
            int result = 0;

            while (result == 0)
            {
                var variant = new Random().Next(Int32.MinValue, Int32.MaxValue);

                if (await GetAsync(variant) == null)
                {
                    result = variant;
                }
            }

            return result;
        }


        /// <summary>
        /// Change user values with entered ID
        /// No, it is impossible to change the object ID, it is ignored
        /// </summary>
        /// <param name="originalID">Original object ID</param>
        /// <param name="alternative">alternative values of entered object ID</param>
        /// <returns></returns>
        public async Task ChangeAsync(long originalID, T alternative)
        {
            alternative.ID = originalID;

            var command = ChangeElementStringFormat;
            command = command.Replace("$NAME$", this.Name);
            command = command.Replace("$COLUMN AND VALUES$", GenerateColumnNamesWithValues(alternative));
            command = command.Replace("$CONDITION$", "ID = " + originalID);

            await EnterCommandNonQuaryAsync(command);
        }

        /// <summary>
        /// Delete object from database
        /// </summary>
        /// <param name="deletable">object, wich needed delete</param>
        /// <returns></returns>
        public async Task DeleteAsync(T deletable)
        {
            var commmand = DeleteElementStringFormat;
            commmand = commmand.Replace("$NAME$", this.Name);
            commmand = commmand.Replace("$CONDITION$", "ID = " + deletable.ID);

            await EnterCommandNonQuaryAsync(commmand);
        }

        /// <summary>
        /// to map obj values to format : 
        /// Prop1 = propValue1, Prop2 = propValue2, Prop3 = propValue3... 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GenerateColumnNamesWithValues(T obj)
        {
            var props = obj.GetType().GetProperties();

            string str = "";

            for (int i = 0; i < props.Length; i++)
            {
                bool typeIsString = props[i].PropertyType == typeof(string);

                str += props[i].Name + " = " + (typeIsString ? "'" : "") + props[i].GetValue(obj) + (typeIsString ? "'" : "") + (i == props.Length - 1 ? "" : ",");
            }

            return str;
        }


        /// <summary>
        /// to map obj values to format : 
        /// propValue1, propValue2, propValue3... 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

        /// <summary>
        /// to map obj values to format : 
        /// CREATE $NAME$ (PropName1 Type1, PropName2 Type2, PropName3 Type3...)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GenerateCreateString(string name, List<SQLProperty> properties)
        {
            string schema = "";

            for (int i = 0; i < properties.Count; i++)
            {
                schema += " " + properties[i].Name + " " + properties[i].Type.ToString() + " " + (properties[i].PRIMARY_KEY ? "PRIMARY KEY" : "") + (i == properties.Count - 1 ? "" : ",");
            }

            return CreateStringFormat.Replace("$NAME$", name).Replace("$SCHEMA$", schema); ;
        }

        /// <summary>
        /// to map obj values to format : 
        /// PropName1, PropName2, PropName3...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GenerateColumnsNames(List<SQLProperty> propertyes)
        {
            string result = "";

            for (int i = 0; i < propertyes.Count; i++)
            {
                result += propertyes[i].Name + (i == propertyes.Count - 1 ? "" : ",");
            }

            return result;
        }

        //TODO: придумать как то избавится от копирования кода в функциях с префиксами Generate.
    }
}

/*
 Я : просто пишу код
 Полтора читателя моего гитхаба:
    https://i.kym-cdn.com/photos/images/newsfeed/001/528/504/195.png
 */