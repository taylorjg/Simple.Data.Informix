using System;
using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Informix
{
    public class InformixColumnInfo
    {
        public string Name { get; private set; }
        public bool IsAutoincrement { get; private set; }
        public DbType DbType { get; private set; }
        public int Capacity { get; private set; }

        public InformixColumnInfo(string name, bool isAutoincrement, DbType dbType, int capacity)
        {
            Name = name;
            IsAutoincrement = isAutoincrement;
            DbType = dbType;
            Capacity = capacity;
        }
    }

    public static class InformixColumnInfoCreator
    {
        private static readonly Dictionary<int, DbType> DbTypes = new Dictionary<int, DbType>
        {
            {0,  DbType.StringFixedLength}, // CHAR
            {1,  DbType.Int16},             // SMALLINT
            {2,  DbType.Int32},             // INTEGER
            {3,  DbType.Double},            // FLOAT
            {4,  DbType.Single},            // SMALLFLOAT
            {5,  DbType.Decimal},           // DECIMAL
            {6,  DbType.Int32},             // SERIAL
            {7,  DbType.DateTime},          // DATE
            {8,  DbType.Currency},          // MONEY
            {9,  DbType.Object},            // NULL
            {10, DbType.DateTime},          // DATETIME
            {11, DbType.Binary},            // BYTE
            {12, DbType.String},            // TEXT
            {13, DbType.String},            // VARCHAR
            {14, DbType.String},            // INTERVAL
            {15, DbType.StringFixedLength}, // NCHAR
            {16, DbType.String},            // NVARCHAR
            {17, DbType.Int64},             // INT8
            {18, DbType.Int64},             // SERIAL8
            {19, DbType.String},            // SET
            {20, DbType.String},            // MULTISET
            {21, DbType.String},            // LIST
            {22, DbType.String},            // Unnamed ROW
            {40, DbType.String}             // Variable-length opaque type
        };

        private static DbType GetDbType(int colType)
        {
            // We only pass in the first byte of colType because the
            // second byte can contain additional flags:
            // 
            // Bit Value Significance When Bit Is Set 
            // 0x0100 NULL values are not allowed 
            // 0x0200 Value is from a host variable 
            // 0x0400 Float-to-decimal for networked database server 
            // 0x0800 DISTINCT data type 
            // 0x1000 Named ROW type 
            // 0x2000 DISTINCT type from LVARCHAR base type 
            // 0x4000 DISTINCT type from BOOLEAN base type 
            // 0x8000 Collection is processed on client system 

            DbType clrType;
            int firstByte = colType & 0xFF;
            return DbTypes.TryGetValue(firstByte, out clrType) ? clrType : DbType.String;
        }

        private static bool DetermineIsAutoincrement(int colType)
        {
            // SERIAL (6) or SERIAL8 (18)
            int firstByte = colType & 0xFF;
            return (firstByte == 6 || firstByte == 18);
        }

        public static InformixColumnInfo CreateColumnInfo(string fieldColumnValue, int colType, int colLength)
        {
            var columnName = fieldColumnValue;
            var isAutoIncrement = DetermineIsAutoincrement(colType);
            DbType clrType = GetDbType(colType);

            return new InformixColumnInfo(columnName, isAutoIncrement, clrType, colLength);
        }
    }
}
