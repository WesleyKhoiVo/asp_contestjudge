using System;
using CHOJ.Abstractions;
using IBatisNet.Common.Utilities;
using IBatisNet.DataAccess;
using IBatisNet.DataAccess.Configuration;
using IBatisNet.DataAccess.SessionStore;

namespace CHOJ.Service
{
    public class ServiceConfig
    {
        static private readonly object SynRoot = new Object();
        static private ServiceConfig _instance;

        /// <summary>
        /// Remove public constructor. prevent instantiation.
        /// </summary>
        private ServiceConfig() { }

        static public ServiceConfig GetInstance()
        {
            if (_instance == null)
            {
                lock (SynRoot)
                {
                    if (_instance == null)
                    {
                        var handler = new ConfigureHandler(Reset);
                       
                        try
                        {
                            var builder = new DomDaoManagerBuilder();
                            builder.ConfigureAndWatch("dao.config", handler); 

                             //IBatisNet.DataAccess.DaoManager.ConfigureAndWatch(handler);
          //                  var builder = new DomDaoManagerBuilder();
            //                builder.ConfigureAndWatch(handler);
                          //  builder.Configure();
                          
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                        _instance = new ServiceConfig
                                        {
                                            DaoManager =
                                            IBatisNet.DataAccess.
                                            DaoManager.GetInstance("Ssds")    
                                        };
                       
                    //_instance.DaoManager.SessionStore = new HybridWebThreadSessionStore(_instance.DaoManager.Id);
                    }
                }
            }
            return _instance;
        }


        /// <summary>
        /// Reset the singleton
        /// </summary>
        /// <remarks>
        /// Must verify ConfigureHandler signature.
        /// </remarks>
        /// <param name="obj">
        /// </param>
        static public void Reset(object obj)
        {
            _instance = null;
        }

        public IDaoManager DaoManager { get; private set; }
    }
}