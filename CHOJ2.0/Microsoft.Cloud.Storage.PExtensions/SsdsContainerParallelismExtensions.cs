namespace Microsoft.Cloud.Storage.PExtensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using Microsoft.Samples.Cloud.Data;

    public static class SsdsContainerParallelismExtensions
    {
        public static void Insert<T>(this SsdsContainer source, IEnumerable<SsdsEntity<T>> entities, Func<SsdsEntity<T>, Exception, bool> onError) where T : class
        {
            object concurrency = new object();

            Parallel.ForEach<SsdsEntity<T>>(entities, delegate(SsdsEntity<T> entity, ParallelState ps)
            {
                try
                {
                    source.Insert<T>(entity);
                }
                catch (Exception ex)
                {
                    lock (concurrency)
                    {
                        if (!ps.IsStopped)
                        {
                            if (onError != null)
                            {
                                if (!onError(entity, ex))
                                {
                                    ps.Stop();
                                }
                            }
                        }
                    }
                }
            });
        }

        public static void Insert(this SsdsContainer source, IEnumerable<SsdsBlobEntity> entities, Func<SsdsBlobEntity, Exception, bool> onError)
        {
            object concurrency = new object();

            Parallel.ForEach<SsdsBlobEntity>(entities, delegate(SsdsBlobEntity entity, ParallelState ps)
            {
                try
                {
                    source.Insert(entity);
                }
                catch (Exception ex)
                {
                    lock (concurrency)
                    {
                        if (!ps.IsStopped)
                        {
                            if (onError != null)
                            {
                                if (!onError(entity, ex))
                                {
                                    ps.Stop();
                                }
                            }
                        }
                    }
                }
            });
        }

        public static void InsertAsync<T>(this SsdsContainer source, IEnumerable<SsdsEntity<T>> entities, Action<IEnumerable<SsdsEntity<T>>> onInsertComplete, Func<SsdsEntity<T>, Exception, bool> onError) where T : class
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                Insert(source, entities, onError);
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onInsertComplete != null)
                {
                    onInsertComplete(entities);
                }
            });

            worker.RunWorkerAsync();
        }

        public static void InsertAsync(this SsdsContainer source, IEnumerable<SsdsBlobEntity> entities, Action<IEnumerable<SsdsBlobEntity>> onInsertComplete, Func<SsdsBlobEntity, Exception, bool> onError)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                Insert(source, entities, onError);
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onInsertComplete != null)
                {
                    onInsertComplete(entities);
                }
            });

            worker.RunWorkerAsync();
        }


        public static void Update<T>(this SsdsContainer source, IEnumerable<SsdsEntity<T>> entities, Func<SsdsEntity<T>, Exception, bool> onError) where T : class, new()
        {
            object concurrency = new object();

            Parallel.ForEach<SsdsEntity<T>>(entities, delegate(SsdsEntity<T> entity, ParallelState ps)
            {
                try
                {
                    source.Update<T>(entity.Entity, entity.Id);
                }
                catch (Exception ex)
                {
                    lock (concurrency)
                    {
                        if (!ps.IsStopped)
                        {
                            if (onError != null)
                            {
                                if (!onError(entity, ex))
                                {
                                    ps.Stop();
                                }
                            }
                        }
                    }
                }
            });
        }

        public static void Update(this SsdsContainer source, IEnumerable<SsdsBlobEntity> entities, Func<SsdsBlobEntity, Exception, bool> onError)
        {
            object concurrency = new object();

            Parallel.ForEach<SsdsBlobEntity>(entities, delegate(SsdsBlobEntity entity, ParallelState ps)
            {
                try
                {
                    source.Update(entity);
                }
                catch (Exception ex)
                {
                    lock (concurrency)
                    {
                        if (!ps.IsStopped)
                        {
                            if (onError != null)
                            {
                                if (!onError(entity, ex))
                                {
                                    ps.Stop();
                                }
                            }
                        }
                    }
                }
            });
        }

        public static void UpdateAsync<T>(this SsdsContainer source, IEnumerable<SsdsEntity<T>> entities, Action<IEnumerable<SsdsEntity<T>>> onUpdateComplete, Func<SsdsEntity<T>, Exception, bool> onError) where T : class, new()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                Update(source, entities, onError);
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onUpdateComplete != null)
                {
                    onUpdateComplete(entities);
                }
            });

            worker.RunWorkerAsync();
        }

        public static void UpdateAsync(this SsdsContainer source, IEnumerable<SsdsBlobEntity> entities, Action<IEnumerable<SsdsBlobEntity>> onUpdateComplete, Func<SsdsBlobEntity, Exception, bool> onError)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                Update(source, entities, onError);
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onUpdateComplete != null)
                {
                    onUpdateComplete(entities);
                }
            });

            worker.RunWorkerAsync();
        }


        public static void Delete(this SsdsContainer source, IEnumerable<string> entities, Func<string, Exception, bool> onError)
        {
            object concurrency = new object();

            Parallel.ForEach<string>(entities, delegate(string entityId, ParallelState ps)
            {
                try
                {
                    source.Delete(entityId);
                }
                catch (Exception ex)
                {
                    lock (concurrency)
                    {
                        if (!ps.IsStopped)
                        {
                            if (onError != null)
                            {
                                if (!onError(entityId, ex))
                                {
                                    ps.Stop();
                                }
                            }
                        }
                    }
                }
            });
        }

        public static void DeleteAsync(this SsdsContainer source, IEnumerable<string> entities, Action<IEnumerable<string>> onDeleteComplete, Func<string, Exception, bool> onError)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                Delete(source, entities, onError);
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onDeleteComplete != null)
                {
                    onDeleteComplete(entities);
                }
            });

            worker.RunWorkerAsync();
        }
    }
}
