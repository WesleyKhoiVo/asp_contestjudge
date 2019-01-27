using System.Collections;
using System.Collections.Specialized;
using IBatisNet.Common;
using IBatisNet.DataAccess;
using IBatisNet.DataAccess.DaoSessionHandlers;
using IBatisNet.DataAccess.Exceptions;
using IBatisNet.DataAccess.Interfaces;

namespace CHOJ.Abstractions
{
    public class AzureDaoSessionHandler : IDaoSessionHandler
    {
        // Fields
        private DataSource _dataSource;

        // Methods
        public void Configure(NameValueCollection properties, IDictionary resources)
        {
            this._dataSource = (DataSource)resources["DataSource"];
        }

        public DaoSession GetDaoSession(DaoManager daoManager)
        {
            return new AzureDaoSession(daoManager, this._dataSource);
        }
    }



}
