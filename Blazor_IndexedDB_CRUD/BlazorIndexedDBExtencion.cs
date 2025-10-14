using Blazor_IndexedDB_CRUD.Data;
using Blazor.IndexedDB;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

public static class BlazorIndexedDBExtencion
    {
        private static Type _IndexedDbType;
        public static void Use<T>(this IIndexedDbFactory DbFactory) where T : IndexedDb
        {
            _IndexedDbType = typeof(T);        
        }

        private static async Task<IndexedDb> Create(IIndexedDbFactory DbFactory)
        {
            // Calling a static generic method
            MethodInfo metodo = 
                DbFactory.GetType()
                .GetMethods()
                .Where(x => x.Name == "Create")
                .Where(x => x.IsGenericMethod)
                .FirstOrDefault();

            MethodInfo metodoGenerico = metodo.MakeGenericMethod(_IndexedDbType);
            dynamic awaitableTask = metodoGenerico.Invoke(DbFactory, null); // null for instance, null for parameters
            IndexedDb indexedDb = await awaitableTask;
            return indexedDb;                      
        }

        /// <summary>
        /// Elimina todas las entidades.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <returns></returns>
        public static async Task Delete<T>(this IIndexedDbFactory DbFactory) where T : IEntity, new()
        {            
            using (var db = await Create(DbFactory))
            {                
                var entities = GetProperty<T>(db);
                List<T> deleteEntities = entities.ToList();
                entities.RemoveRange(deleteEntities);
                await db.SaveChanges(); // Persist changes
            }
        }

        /// <summary>
        /// Elimina una entidad por su Id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Delete<T>(this IIndexedDbFactory DbFactory, int id) where T : IEntity, new()
        {
            using (var db = await Create(DbFactory))
            {                
                var entities = GetProperty<T>(db);
                T entityInDB = entities.Single(e => e.Id == id);
                entities.Remove(entityInDB);       
                await db.SaveChanges(); // Persist changes
            }
        }

        /// <summary>
        /// Elimina una entidad.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task Delete<T>(this IIndexedDbFactory DbFactory, T entity) where T : IEntity, new()
        {
            await Delete<T>(DbFactory, entity.Id.Value);
        }

        /// <summary>
        /// Modifica una entidad existente, encontrándola por su Id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task Update<T>(this IIndexedDbFactory DbFactory, T entity) where T : IEntity, new()
        {
            //using (var db = await DbFactory.Create<AppDb>())
            using (var db = await Create(DbFactory))
            {
                var entities = GetProperty<T>(db);
                T entityInDB = entities.Single(e => e.Id == entity.Id);
                SetAllPropertys(entity, entityInDB);
                await db.SaveChanges(); // Persist changes
            }
        }

        private static void SetAllPropertys<T>(T origen, T destino)
        {
            PropertyInfo[] properties = origen.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.CanWrite)
                {
                    string propertyName = propertyInfo.Name;                    
                    object? origenValor = origen.GetType().GetProperty(propertyName).GetValue(origen);
                    destino.GetType().GetProperty(propertyName).SetValue(destino, origenValor);
                }
            }
        }

        /// <summary>
        /// Adiciona una nueva entidad, estableciendo el valor del Id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        public static async Task Insert<T>(this IIndexedDbFactory DbFactory, T newEntity) where T : IEntity, new()
        {
            using (var db = await Create(DbFactory))
            {
                var entities = GetProperty<T>(db);
                entities.Add(newEntity);
                await db.SaveChanges(); // Persist changes                
            }
            //Recupera el valor del nuevo Id de la entidad.
            using (var db = await Create(DbFactory))
            {
                var entities = GetProperty<T>(db);
                newEntity.Id = entities.Last().Id;                
            }
        }

        /// <summary>
        /// Realiza un insert o un update dependiendo de la existencia 
        /// o no del valor de la propiedad Id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task Save<T>(this IIndexedDbFactory DbFactory, T entity) where T : IEntity, new()
        {
            if(entity.Id.HasValue)
                await Update(DbFactory, entity);
            else
                await Insert(DbFactory, entity);
        }

        /// <summary>
        /// Busca una entidad por su Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<T> SelectOne<T>(this IIndexedDbFactory DbFactory, int id) where T : IEntity, new()
        {
            //Documentacion
            //https://www.nuget.org/packages/Blazor.IndexedDB
            T entity = new();
            using (var db = await Create(DbFactory))
            {
                var entities = GetProperty<T>(db);
                entity = entities.Single(e => e.Id == id);                               
            }
            return entity;
        }

        /// <summary>
        /// Retorna todas las entidades.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DbFactory"></param>
        /// <returns></returns>
        public static async Task<List<T>> SelectAll<T>(this IIndexedDbFactory DbFactory) where T : new()
        {
            //Documentacion
            //https://www.nuget.org/packages/Blazor.IndexedDB
            List<T> entities = new List<T>();
            using (IndexedDb db = await Create(DbFactory))
            {
                var property = GetProperty<T>(db);
                entities = property.ToList<T>();
            }
            return entities;
        }

        private static IndexedSet<T> GetProperty<T>(IndexedDb db) where T : new()
        {
            object objDB =  Convert.ChangeType(db, _IndexedDbType);

            IndexedSet<T> propertyX = null;
            PropertyInfo[] properties = objDB.GetType().GetProperties();
            PropertyInfo propertyRequerida = null;
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(IndexedSet<T>))
                {
                    propertyRequerida = property;
                }
            }
            var valor = propertyRequerida?.GetValue(objDB);
            propertyX = (IndexedSet<T>)valor;
            return propertyX;
        }
    }

