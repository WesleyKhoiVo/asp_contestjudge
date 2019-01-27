using CHOJ.Abstractions;
using IBatisNet.DataAccess;
using IBatisNet.DataAccess.DaoSessionHandlers;
using IBatisNet.DataAccess.Interfaces;
using IBatisNet.DataMapper;
using Microsoft.Samples.Cloud.Data;

namespace CHOJ.SdsDao
{
    public class BaseDao:IDao
    {
         protected string GetLocalSqlMap()
        {
            var daoManager = DaoManager.GetInstance(this);
            var sqlMapDaoSession = (AzureDaoSession)daoManager.LocalDaoSession;

            return sqlMapDaoSession.DataSource.ConnectionString;
        }
        private SsdsContext _dbContext;
        public SsdsContext DbContext
        {
            get
            {
                if(_dbContext==null)
                {
                    _dbContext = new SsdsContext(
                        GetLocalSqlMap()
                        );
                }
                return _dbContext;
            }
        }
    }
}