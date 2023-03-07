using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace Habby.SQL
{
    public class SQLTable
    {
        public string tableName { get; private set; }
        public string DBName { get; private set; }
        public SQLDBObject DB { get; private set; }

        protected SQLTable(string pTableName, string pDBName)
        {
            tableName = pTableName;
            DBName = pDBName;

            DB = SQLDBManager.GetDB(DBName);
        }
        
    }
}