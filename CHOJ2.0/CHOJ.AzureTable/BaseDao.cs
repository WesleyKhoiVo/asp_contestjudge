using System;
using IBatisNet.DataAccess.Interfaces;
using Microsoft.Samples.ServiceHosting.StorageClient;

namespace CHOJ.AzureTable
{
    public class BaseDao:IDao
    {
        private TableStorageDataServiceContext _dbContext;
        public TableStorageDataServiceContext DbContext
        {
            get
            {
                if(_dbContext==null)
                {
                    _dbContext = new TableStorageDataServiceContext(StorageAccountInfo);
                }
                return _dbContext;
            }
        }

        private StorageAccountInfo _storageAccountInfo;
        public StorageAccountInfo StorageAccountInfo
        {
            get
            {
                if (_storageAccountInfo == null)
                {
                    _storageAccountInfo = new StorageAccountInfo(
                        new Uri("http://ojstore.table.core.windows.net/")
                        , null
                        , "ojstore"
                        , "WvPgjqtm9IqOVOETZZTcmutTJFbyjzr6ENttjX1b8eeZoMDlMj9SI8dqI1szR0sBYEpdJkuYdcosGvZ1iXtPLg=="
                        );
                }
                return _storageAccountInfo;
            }
        }
    }
}