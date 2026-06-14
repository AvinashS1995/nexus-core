using System;
using MySql.Data.MySqlClient;
using System.Data;

namespace NexusCore.Common
{
    public class DbContext : IDisposable
    {
        public readonly MySqlConnection _connection;
        public readonly MySqlConnection _readReplicaConnection1;
        //public string _connectionString;

        //public DbContext(string connectionString, string ReadReplicaConnection1)
        //{
        //    //_connectionString = connectionString;
        //    _connection = new MySqlConnection(connectionString);
        //    _readReplicaConnection1 = new MySqlConnection(ReadReplicaConnection1);
        //    _connection.Open();
        //    _readReplicaConnection1.Open();
        //}

        public DbContext(string connectionString, string ReadReplicaConnection1)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("DefaultConnection is missing");

            if (string.IsNullOrWhiteSpace(ReadReplicaConnection1))
                throw new Exception("ReadConnection is missing");


            _connection = new MySqlConnection(connectionString);
            _readReplicaConnection1 = new MySqlConnection(ReadReplicaConnection1);

            _connection.Open();
            _readReplicaConnection1.Open();
        }


        public void ConnectionClosed()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            if (_readReplicaConnection1.State != ConnectionState.Closed)
                _readReplicaConnection1.Close();
        }

        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            if (_readReplicaConnection1.State != ConnectionState.Closed)
                _readReplicaConnection1.Close();
        }
    }
}

