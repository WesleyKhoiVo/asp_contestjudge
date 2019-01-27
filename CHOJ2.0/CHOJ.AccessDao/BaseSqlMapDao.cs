using System;
using System.Collections.Generic;
using IBatisNet.Common.Exceptions;
using IBatisNet.Common.Pagination;
using IBatisNet.DataAccess;
using IBatisNet.DataAccess.DaoSessionHandlers;
using IBatisNet.DataAccess.Interfaces;
using IBatisNet.DataMapper;

namespace CHOJ.AccessDao
{
    public class BaseSqlMapDao:IDao
    {
        protected const int PageSize = 4;

        /// <summary>
        /// Looks up the parent DaoManager, gets the local transaction
        /// (which should be a SqlMapDaoTransaction) and returns the
        /// SqlMap associated with this DAO.
        /// </summary>
        /// <returns>The SqlMap instance for this DAO.</returns>
        protected ISqlMapper GetLocalSqlMap()
        {
            var daoManager = DaoManager.GetInstance(this);
            var sqlMapDaoSession = (SqlMapDaoSession)daoManager.LocalDaoSession;

            return sqlMapDaoSession.SqlMap;
        }

  
        protected IList<T> ExecuteQueryForList<T>(string statementName, object parameterObject)
        {
            var sqlMap = GetLocalSqlMap();
            try
            {
                return sqlMap.QueryForList<T>(statementName, parameterObject);
            }
            catch (Exception e)
            {
                throw new IBatisNetException("以下列表'" + statementName + "'查询时错误。Cause: " + e.Message, e);
            }
        }

        protected IList<T> ExecuteQueryForList<T>(string statementName, object parameterObject, int skipResults, int maxResults)
        {
            var sqlMap = GetLocalSqlMap();
            try
            {
                return sqlMap.QueryForList<T>(statementName, parameterObject, skipResults, maxResults);
            }
            catch (Exception e)
            {
                throw new IBatisNetException("以下列表'" + statementName + "'查询时错误。Cause: " + e.Message, e);
            }
        }
        [Obsolete("别用这个")]
        protected IPaginatedList ExecuteQueryForPaginatedList(string statementName, object parameterObject, int pageSize)
        {
            var sqlMap = GetLocalSqlMap();
            try
            {
                return sqlMap.QueryForPaginatedList(statementName, parameterObject, pageSize);
            }
            catch (Exception e)
            {
                throw new IBatisNetException(
                    "Error executing query '" + statementName + "' for paginated list.  Cause: " + e.Message, e);
            }
        }





        protected T ExecuteQueryForObject<T>(string statementName, object parameterObject)
        {
            var sqlMap = GetLocalSqlMap();

            try
            {
                return sqlMap.QueryForObject<T>(statementName, parameterObject);
            }
            catch (Exception e)
            {
                throw new IBatisNetException("查询对象发生错误：'" + statementName + "' 。 Cause: " + e.Message, e);
            }
        }


        /// <summary>
        /// Simple convenience method to wrap the SqlMap method of the same name.
        /// Wraps the exception with a IBatisNetException to isolate the SqlMap framework.
        /// </summary>
        /// <param name="statementName"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        protected int ExecuteUpdate(string statementName, object parameterObject)
        {
            var sqlMap = GetLocalSqlMap();

            try
            {
                return sqlMap.Update(statementName, parameterObject);
            }
            catch (Exception e)
            {
                throw new IBatisNetException("更新对象时发生错误 '" + statementName + "'。Cause: " + e.Message, e);
            }
        }

        /// <summary>
        /// Simple convenience method to wrap the SqlMap method of the same name.
        /// Wraps the exception with a IBatisNetException to isolate the SqlMap framework.
        /// </summary>
        /// <param name="statementName"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        protected object ExecuteInsert(string statementName, object parameterObject)
        {
            var sqlMap = GetLocalSqlMap();

            try
            {
                return sqlMap.Insert(statementName, parameterObject);
            }
            catch (Exception e)
            {
                throw new IBatisNetException("更新对象时发生错误'" + statementName + "'。Cause: " + e.Message, e);
            }
        }
    }
}