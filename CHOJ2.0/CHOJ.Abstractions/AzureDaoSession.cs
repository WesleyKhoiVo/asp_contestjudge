using System;
using System.Data;
using IBatisNet.Common;
using IBatisNet.DataAccess;

namespace CHOJ.Abstractions
{
    public class AzureDaoSession : DaoSession

    {
        public AzureDaoSession(DaoManager daoManager, IDataSource dataSource)
            : base(daoManager)
        {
            _dataSource = dataSource;
        }

        public override void Complete()
        {
         
        }

        public override void OpenConnection()
        {
           
        }

        public override void OpenConnection(string connectionString)
        {
           
        }

        public override void CloseConnection()
        {
           
        }

        public override void BeginTransaction()
        {
           
        }

        public override void BeginTransaction(string connectionString)
        {
        
        }

        public override void BeginTransaction(bool openConnection)
        {
           
        }

        public override void BeginTransaction(string connectionString, IsolationLevel isolationLevel)
        {
        
        }

        public override void BeginTransaction(bool openConnection, IsolationLevel isolationLevel)
        {
        
        }

        public override void CommitTransaction()
        {
        
        }

        public override void CommitTransaction(bool closeConnection)
        {
          
        }

        public override void RollBackTransaction()
        {
            
        }

        public override void RollBackTransaction(bool closeConnection)
        {
            
        }

        public override IDbCommand CreateCommand(CommandType commandType)
        {
            throw new NotImplementedException();
        }

        public override IDbDataParameter CreateDataParameter()
        {
            throw new NotImplementedException();
        }

        public override IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        private IDataSource _dataSource;
        public override IDataSource DataSource
        {
            get { return _dataSource; }
        }
        public override IDbConnection Connection
        {
            get { throw new NotImplementedException(); }
        }
        public override IDbTransaction Transaction
        {
            get { throw new NotImplementedException(); }
        }
        public override bool IsTransactionStart
        {
            get { throw new NotImplementedException(); }
        }

        public override IDbDataAdapter CreateDataAdapter()
        {
            throw new NotImplementedException();
        }

        public override void BeginTransaction(string connectionString, bool openConnection, IsolationLevel isolationLevel)
        {
          
        }

        public override void BeginTransaction(IsolationLevel isolationLevel)
        {
           
        }
    }
}