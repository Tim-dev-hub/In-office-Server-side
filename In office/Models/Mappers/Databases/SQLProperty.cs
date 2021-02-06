using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace In_office.Models.Data
{
    public struct SQLProperty
    {
        public enum DataType
        {
            BIGINT,
            BLOB,
            BOOLEAN,
            CHAR,
            DATE,
            DATETIME,
            DECIMAL,
            DOUBLE,
            INTEGER,
            INT, 
            NONE,
            NUMERIC,
            REAL,
            STRING,
            TEXT,
            TIME,
            VARCHAR
        }

        public string Name;
        public DataType Type;
        public bool PRIMARY_KEY;

        public SQLProperty(DataType type, string name, bool primary)
        {
            this.Name = name;
            this.Type = type;
            this.PRIMARY_KEY = primary;
        }
    }
}
