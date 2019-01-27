namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Samples.Cloud.Data.Linq;    

    public delegate void BlobOperationComplete(SsdsBlobEntity blob, Exception exception);    

    /// <summary>
    /// Represents an strongly typed facade for querying Ssds.
    /// </summary>
    /// <typeparam name="T">The return type of the queries and used to insert.</typeparam>
    public class SsdsContainer
    {
        /// <summary>
        /// Holds a reference to the username used to connect to Ssds.
        /// </summary>
        private string userName;

        /// <summary>
        /// Holds a reference to the password used to connect to Ssds.
        /// </summary>
        private string password;

        /// <summary>
        /// Holds a reference to the uri used to connect to Ssds.
        /// </summary>
        private Uri authority;

        /// <summary>
        /// reference to the container we are working with.
        /// </summary>
        private string container;

        /// <summary>
        /// Creates an instance of SsdsContainer.
        /// </summary>
        /// <param name="userName">The username used to connect to Ssds.</param>
        /// <param name="password">The password used to connect to Ssds.</param>
        /// <param name="location">The uri used to connect to Ssds.</param>
        public SsdsContainer(string userName, string password, Uri authority, string container)
        {
            this.userName = userName;
            this.password = password;
            this.authority = authority;
            this.container = container;
        }

        /// <summary>
        /// Inserts an item of type T on Ssds.
        /// </summary>
        /// <param name="item">The object to be persisted on Ssds.</param>
        public void Insert<T>(T item, string id) where T : class
        {
            SsdsEntitySerializer<T> serializer = new SsdsEntitySerializer<T>();
            string payload = serializer.Serialize(item, id);
            SsdsRestFacade facade = this.CreateFacade();
            facade.Insert(payload);
        }

        public void Insert<T>(SsdsEntity<T> entity) where T : class
        {
            SsdsEntitySerializer<T> serializer = new SsdsEntitySerializer<T>();
            string payload = serializer.Serialize(entity);
            SsdsRestFacade facade = this.CreateFacade();
            facade.Insert(payload);
        }

        public string Insert(SsdsBlobEntity blob)
        {
            SsdsRestFacade facade = this.CreateFacade();
            return facade.Insert(blob);
        }

        public void InsertAsync(SsdsBlobEntity blob, BlobOperationComplete onComplete)
        {
            Exception exception = null;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                try
                {
                    this.Insert(blob);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onComplete != null)
                {
                    onComplete(blob, exception);
                }
            });

            worker.RunWorkerAsync();
        }
        
        /// <summary>
        /// Retrieves a single object from the container.
        /// </summary>
        /// <param name="expression">The query used as filtering expression to determine which will be the object.</param>
        /// <returns>An instance of T populated with data from Ssds.</returns>
        public SsdsEntity<T> Single<T>(Expression<Func<SsdsEntity<T>, bool>> expression) where T : class
        {
            return Query(expression).FirstOrDefault();
        }

        public SsdsBlobEntity GetBlob(string entityId)
        {
            SsdsRestFacade facade = this.CreateFacade();
            return facade.GetBlob(entityId); 
        }

        public void GetBlobAsync(string entityId, BlobOperationComplete onComplete)
        {
            SsdsBlobEntity blob = null;
            Exception exception = null;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                try
                {
                    blob = this.GetBlob(entityId);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onComplete != null)
                {
                    onComplete(blob, exception);
                }
            });

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Retrieves multiple object from the container when the match with a criteria.
        /// </summary>
        /// <param name="expression">The expression of the criteria used to determine which will be the object.</param>
        /// <returns>IEnumerable of T populated with data from Ssds.</returns>
        public IEnumerable<SsdsEntity<T>> Query<T>(Expression<Func<SsdsEntity<T>, bool>> expression) where T : class
        {
            SsdsRestFacade facade = this.CreateFacade();

            //since we are querying by kind... add it in
            expression = AddKindToExpression<T>(expression);

            SsdsExpressionVisitor translator = new SsdsExpressionVisitor();
            string criteria = translator.Translate(expression);
            
            var response = facade.Get(criteria);

            if (response == null)
            {
                return null;
            }

            SsdsEntitySerializer<T> serializer = new SsdsEntitySerializer<T>();
            IEnumerable<SsdsEntity<T>> items = serializer.DeserializeMany(response);
            return items;
        }

        /// <summary>
        /// Performs a paged query, retrieving all objects from SSDS of type T
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve</typeparam>
        /// <param name="expression">Selection criteria</param>
        /// <param name="page">Action to execute per page return</param>
        public void PagedQuery<T>(Expression<Func<SsdsEntity<T>, bool>> expression, Action<IEnumerable<SsdsEntity<T>>> page) where T : class
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            var items = Query<T>(expression);

            page(items);

            while (items.Count() == 500)
            {
                var lastid = items.Last().Id;                
                items = Query<T>(AddPageTerm<T>(expression, lastid));
                page(items);
            }
        }

        /// <summary>
        /// Retrieves multiple items of types T and U
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public SsdsEntityBucket Query<T, K>(Expression<Func<SsdsEntity<T>, SsdsEntity<K>, bool>> expression)
            where T : class
            where K : class
        {
            return GetBucketFromExpression(expression);
        }

        public SsdsEntityBucket Query<T, K, V>(Expression<Func<SsdsEntity<T>, SsdsEntity<K>, SsdsEntity<V>, bool>> expression)
            where T : class
            where K : class
            where V : class
        {
            return GetBucketFromExpression(expression);
        }

        private SsdsEntityBucket GetBucketFromExpression(Expression expression)
        {
            SsdsRestFacade facade = this.CreateFacade();
            SsdsExpressionVisitor translator = new SsdsExpressionVisitor();
            string criteria = translator.Translate(expression);
            var response = facade.Get(criteria);

            var types = response.Elements().GroupBy(g => g.Name).Select(s => Type.GetType(s.Key.LocalName));

            return new SsdsEntityBucket { Response = response, Types = types.ToArray() };
        }

        /// <summary>
        /// Deletes an item from the container.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        public void Delete(string id)
        {
            this.Delete(id, ConcurrencyPattern.Always, null);
        }

        public void Delete<T>(SsdsEntity<T> entity, ConcurrencyPattern concurrencyPattern) where T : class, new()
        {
            this.Delete(entity.Id, concurrencyPattern, entity.Version.ToString());
        }

        public void Delete(SsdsBlobEntity entity, ConcurrencyPattern concurrencyPattern)
        {
            this.Delete(entity.Id, concurrencyPattern, entity.Version);
        }

        private void Delete(string id, ConcurrencyPattern concurrencyPattern, string version)
        {
            SsdsRestFacade facade = this.CreateFacade();
            facade.Delete(id, concurrencyPattern, version);
        }

        /// <summary>
        /// Updates the item on the container with concurrency check
        /// </summary>
        /// <param name="item">The item with the information to be updated.</param>
        /// <param name="id">Id associated with item in SSDS (unique)</param>
        public void Update<T>(T item, string id) where T : class, new()
        {
            Update(new SsdsEntity<T> { Id = id, Entity = item }, ConcurrencyPattern.IfMatch);
        }

        /// <summary>
        /// Updates the entity with the given concurrency pattern
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="id"></param>
        /// <param name="pattern"></param>
        public void Update<T>(T item, string id, ConcurrencyPattern pattern) where T : class, new()
        {
            Update(new SsdsEntity<T> { Id = id, Entity = item }, pattern);
        }

        /// <summary>
        /// Updates entity using concurrency
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Update<T>(SsdsEntity<T> entity) where T: class, new()
        {
            Update(entity, ConcurrencyPattern.IfMatch);
        }

        /// <summary>
        /// Updates the item on the container.
        /// </summary>
        /// <param name="entity">The item with the information to be updated.</param>
        public void Update<T>(SsdsEntity<T> entity, ConcurrencyPattern concurrencyPattern) where T : class, new()
        {
            Uri updateLocation = HttpRestUriTemplates.UpdateTemplate.BindByPosition(this.authority, this.container, entity.Id);
            SsdsRestFacade facade = this.CreateFacade(updateLocation);
            SsdsEntitySerializer<T> serializer = new SsdsEntitySerializer<T>();
            string payload = serializer.Serialize(entity);
            facade.Update(payload, concurrencyPattern, entity.Version.ToString());
        }

        public void Update(SsdsBlobEntity blob)
        {
            Update(blob, ConcurrencyPattern.Always);
        }

        public void Update(SsdsBlobEntity blob, ConcurrencyPattern concurrencyPattern)
        {
            SsdsRestFacade facade = this.CreateFacade();
            facade.Update(blob, concurrencyPattern);
        }        

        public void UpdateAsync(SsdsBlobEntity blob, BlobOperationComplete onComplete)
        {
            this.UpdateAsync(blob, ConcurrencyPattern.Always, onComplete);
        }

        public void UpdateAsync(SsdsBlobEntity blob, ConcurrencyPattern concurrencyPattern, BlobOperationComplete onComplete)
        {
            Exception exception = null;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
            {
                try
                {
                    this.Update(blob, concurrencyPattern);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (onComplete != null)
                {
                    onComplete(blob, exception);
                }
            });

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Creates an instance of SsdsRestFacade used to interact with the container.
        /// </summary>
        /// <returns>An instance of SsdsRestFacade.</returns>
        private SsdsRestFacade CreateFacade()
        {
            Uri location = HttpRestUriTemplates.ContainerTemplate.BindByPosition(this.authority, this.container);
            return this.CreateFacade(location);
        }

        /// <summary>
        /// Create an instance of the SsdsRestFacade used to interact with the container.
        /// </summary>
        /// <param name="scope">The scope where the facade will be interacting with.</param>
        /// <returns>An instance of SsdsRestFacade.</returns>
        private SsdsRestFacade CreateFacade(Uri scope)
        {
            SsdsRestFacade facade = new SsdsRestFacade(this.userName, this.password, scope);
            return facade;
        }

        /// <summary>
        /// Adds the Paging syntax into a given expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="lastid"></param>
        /// <returns></returns>
        private static Expression<Func<SsdsEntity<T>, bool>> AddPageTerm<T>(Expression<Func<SsdsEntity<T>, bool>> expression, string lastid) where T : class
        {
            //TODO: should probably visit expression to make sure paging term isn't already there
            Expression<Func<SsdsEntity<T>, bool>> func = f => f.Id.GreaterThan(lastid);

            if (expression != null)
            {
                return Expression.Lambda<Func<SsdsEntity<T>, bool>>(
                    Expression.AndAlso(
                        expression.Body,
                        func.Body
                        ),
                        expression.Parameters[0]
                        );
            }

            return Expression.Lambda<Func<SsdsEntity<T>, bool>>(func.Body, func.Parameters[0]);
        }

        /// <summary>
        /// Adds the Kind into the expression to limit results for typed Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static Expression<Func<SsdsEntity<T>, bool>> AddKindToExpression<T>(Expression<Func<SsdsEntity<T>, bool>> expression) where T : class
        {
            if (expression != null)
            {
                return Expression.Lambda<Func<SsdsEntity<T>, bool>>
                    (
                    Expression.AndAlso(
                        expression.Body,
                        Expression.Equal(
                            Expression.Property(
                                expression.Parameters[0],
                                "Kind"),
                            Expression.Constant(typeof(T).Name))
                        ),
                        expression.Parameters[0]
                    );
            }

            Expression<Func<SsdsEntity<T>, bool>> expr = f => f.Kind == typeof(T).Name;
            return expr;
        }
    }
}
