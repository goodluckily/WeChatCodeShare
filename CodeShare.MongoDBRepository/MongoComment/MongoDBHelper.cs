using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.MongoDBRepository
{
    public class MongoDBHelper<T> where T : class, new()
    {
        private MongoDBHelper()
        {

        }

        public static IMongoCollection<T> GetClient()
        {
            return MongoDBClient<T>.GetCollectionInstance();
        }

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="t">添加的实体</param>
        /// <returns></returns>
        public static bool Add(T t)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            client.InsertOne(t);
            return true;
        }

        /// <summary>
        /// 异步添加一条数据
        /// </summary>
        /// <param name="t">添加的实体</param>

        /// <returns></returns>
        public static async Task<bool> AddAsync(T t)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            await client.InsertOneAsync(t);
            return true;
        }

        public static async Task<T> AddModelAsync(T t)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            await client.InsertOneAsync(t);
            return t;
        }
        #region GridFSBucket byte[] 数据操作


        /// <summary>
        /// 添加byte[] 文件数据
        /// </summary>
        /// <param name=""></param>
        /// <param name="t"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<ObjectId> addFSBucket(T t)
        {
            var clientBucket = MongoDBClient<T>.GetGridFSBucket();
            byte[] s_nameUtf8 = t.ToBson();
            var fileName = typeof(T).Name;
            var id = await clientBucket.UploadFromBytesAsync(fileName, s_nameUtf8);
            return id;
        }


        /// <summary>
        /// 得到Files byte[] 转换的GridFSFileInfo 数据
        /// </summary>
        /// <param name=""></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<GridFSFileInfo> GetFSBucketFilesById(List<ObjectId> ids)
        {
            var clientBucket = MongoDBClient<T>.GetGridFSBucket();
            //查看文件
            var filter = Builders<GridFSFileInfo>.Filter;
            var cursor = clientBucket.Find(filter.Eq("_id", ids)).ToList();
            return cursor;
        }
        /// <summary>
        /// 根据主键批量更新数据
        /// </summary>
        /// <param name="listAdd">更新的数据集合</param>
        /// <param name="changeKey">需要更新的字段</param>
        /// <returns></returns>
        public static async Task<BulkWriteResult> BulkUpdateAsync(IEnumerable<T> listAdd, params string[] changeKey)
        {
            if (!listAdd.Any()) return null;
            var options = new List<UpdateOneModel<T>>();
            var filter = Builders<T>.Filter;
            var update = Builders<T>.Update;
            var listUpdate = new List<UpdateOneModel<T>>();
            foreach (var item in listAdd)
            {
                ObjectId objectId = new ObjectId();
                var props = item.GetType().GetProperties().ToList();
                var attr = props.FirstOrDefault(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute)));
                //根据特性取主键id的值
                if (attr != null)
                {
                    objectId = (ObjectId)attr.GetValue(item);
                    props.Remove(attr);
                }
                if (changeKey != null && changeKey.Length > 0) props = props.Where(x => changeKey.Contains(x.Name)).ToList();
                var list = new List<UpdateDefinition<T>>();
                props.ForEach(x => list.Add(Builders<T>.Update.Set(x.Name, x.GetValue(item))));
                //更新的依据
                var updateFilter = filter.Eq("_id", objectId);
                //构建更新的内容
                var updateDefinition = Builders<T>.Update.Combine(list);
                var updateOneModel = new UpdateOneModel<T>(updateFilter, updateDefinition);
                listUpdate.Add(updateOneModel);
            }
            var client = MongoDBClient<T>.GetCollectionInstance();
            var result = await client.BulkWriteAsync(listUpdate, new BulkWriteOptions() { IsOrdered = true, });
            return result;
        }


        /// <summary>
        /// 得到Chunks byte[] 转换的 T 数据
        /// </summary>
        /// <param name=""></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<T> GetFSBucketChunksById(ObjectId id)
        {
            var clientBucket = MongoDBClient<T>.GetGridFSBucket();
            //得到存入的Byte[]
            var DownloadBytes = await clientBucket.DownloadAsBytesAsync(id);
            // 反序列化bson
            MemoryStream memoryStream = new MemoryStream(DownloadBytes);
            T ConversionsModel = (T)BsonSerializer.Deserialize(memoryStream, typeof(T));
            return ConversionsModel;
        }

        /// <summary>
        /// 得到Chunks byte[] 转换的 T 数据
        /// </summary>
        /// <param name=""></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task DelFSBucketChunksById(ObjectId id)
        {
            var clientBucket = MongoDBClient<T>.GetGridFSBucket();
            //得到存入的Byte[]
            await clientBucket.DeleteAsync(id);
        }

        #endregion


        /// <summary>
        /// 批量插入
        /// </summary>

        /// <param name="t">实体集合</param>
        /// <returns></returns>
        public static bool AddList1(List<T> t)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            client.InsertMany(t);
            return true;
        }


        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="t">实体集合</param>
        /// <returns>bool</returns>
        public static async Task<bool> AddListAsync(IEnumerable<T> t)
        {
            if (t.Count() <= 0) return false;
            var client = MongoDBClient<T>.GetCollectionInstance();
            await client.InsertManyAsync(t);
            return true;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="t">实体集合</param>
        /// <returns>bool</returns>
        public static bool AddList(IEnumerable<T> t)
        {
            if (t.Count() <= 0) return false;
            var client = MongoDBClient<T>.GetCollectionInstance();
            client.InsertMany(t);
            return true;
        }


        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="t">实体集合</param>
        /// <returns>ListT</returns>
        public static async Task<List<T>> AddListModelAsync(List<T> t)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            await client.InsertManyAsync(t);
            return t;
        }

        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="t">实体集合</param>
        /// <returns>ListT</returns>
        public static async Task<IEnumerable<T>> AddListModelAsync(IEnumerable<T> t)
        {
            if (t.Count() <= 0) return t;
            var client = MongoDBClient<T>.GetCollectionInstance();
            await client.InsertManyAsync(t);
            return t;
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="t">添加的实体</param>

        /// <returns></returns>
        public static UpdateResult Update(T t, ObjectId id)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //修改条件
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", id);
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in t.GetType().GetProperties())
            {
                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                if (attr) continue;

                list.Add(Builders<T>.Update.Set(item.Name, item.GetValue(t)));
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return client.UpdateOne(filter, updatefilter);
        }

        /// <summary>
        /// 异步修改一条数据
        /// </summary>
        /// <param name="t">添加的实体</param>

        /// <returns></returns>
        public static async Task<UpdateResult> UpdateAsync(T t, ObjectId id)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //修改条件
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", id);
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in t.GetType().GetProperties())
            {
                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                if (attr) continue;
                list.Add(Builders<T>.Update.Set(item.Name, item.GetValue(t)));
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return await client.UpdateOneAsync(filter, updatefilter);
        }

        public static async Task<T> UpdateModelAsync(T t, ObjectId id)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //修改条件
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", id);
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in t.GetType().GetProperties())
            {
                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                if (attr) continue;
                list.Add(Builders<T>.Update.Set(item.Name, item.GetValue(t)));
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            await client.UpdateOneAsync(filter, updatefilter);
            return t;
        }

        /// <summary>
        /// 单个 依据字段修改
        /// </summary>
        /// <param name=""></param>
        /// <param name="t"></param>
        /// <param name="id"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static async Task<UpdateResult> UpdateModelFieldAsync(ObjectId id, Dictionary<string, object> dic)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //修改条件
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", id);
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            var t = new T();
            foreach (var item in t.GetType().GetProperties())
            {
                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                if (attr) continue;
                //依据字段修改
                if (!dic.ContainsKey(item.Name)) continue;
                var value = dic[item.Name];
                list.Add(Builders<T>.Update.Set(item.Name, value));
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return await client.UpdateOneAsync(filter, updatefilter);
        }

        public static async Task<UpdateResult> UpdateModelFieldAsync(Expression<Func<T, bool>> expression, Dictionary<string, object> dic)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            var builder = Builders<T>.Filter;
            //修改条件
            var filter = builder.And(builder.Where(expression));
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            var t = new T();
            foreach (var item in t.GetType().GetProperties())
            {
                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                if (attr) continue;
                //依据字段修改
                if (!dic.ContainsKey(item.Name)) continue;
                var value = dic[item.Name];
                list.Add(Builders<T>.Update.Set(item.Name, value));
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return await client.UpdateOneAsync(filter, updatefilter);
        }

        /// <summary>
        /// 依据条件修改 用来统一修改某一个值用
        /// </summary>
        /// <param name=""></param>
        /// <param name="filter">条件</param>
        /// <param name="updateDefinition">修改</param>
        /// <returns></returns>
        public static async Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> updateDefinition)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //修改条件
            return await client.UpdateManyAsync(filter, updateDefinition);
        }


        /// <summary>
        /// 批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>
        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        public static UpdateResult UpdateManay(Dictionary<string, string> dic, FilterDefinition<T> filter)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            T t = new T();
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            foreach (var item in t.GetType().GetProperties())
            {
                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                if (attr) continue;
                //依据字段修改
                if (!dic.ContainsKey(item.Name)) continue;
                var value = dic[item.Name];
                list.Add(Builders<T>.Update.Set(item.Name, value));
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return client.UpdateMany(filter, updatefilter);
        }

        /// <summary>
        /// 异步批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>

        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        public static async Task<UpdateResult> UpdateManayAsync(FilterDefinition<T> filter, Dictionary<string, object> dic = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            T t = new T();
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            if (dic != null)
            {
                foreach (var item in t.GetType().GetProperties())
                {
                    var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                    if (attr) continue;
                    //依据字段修改
                    if (!dic.ContainsKey(item.Name)) continue;
                    var value = dic[item.Name];
                    list.Add(Builders<T>.Update.Set(item.Name, value));
                }
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return await client.UpdateManyAsync(filter, updatefilter);
        }

        /// <summary>
        /// 异步批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>

        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        public static async Task<UpdateResult> UpdateManayAsync(Expression<Func<T, bool>> expression, Dictionary<string, object> dic = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            var builder = Builders<T>.Filter;
            //修改条件
            var filter = builder.And(builder.Where(expression));
            T t = new T();
            //要修改的字段
            var list = new List<UpdateDefinition<T>>();
            if (dic != null)
            {
                foreach (var item in t.GetType().GetProperties())
                {
                    var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
                    if (attr) continue;
                    //依据字段修改
                    if (!dic.ContainsKey(item.Name)) continue;
                    var value = dic[item.Name];
                    list.Add(Builders<T>.Update.Set(item.Name, value));
                }
            }
            var updatefilter = Builders<T>.Update.Combine(list);
            return await client.UpdateManyAsync(filter, updatefilter);
        }


        /// <summary>
        /// 异步批量修改数据
        /// </summary>
        /// <param name="dic">要修改的字段</param>
        /// <param name="filter">修改条件</param>
        /// <returns></returns>
        //public static async Task<UpdateResult> UpdateManayAsync(FilterDefinition<T> filter, List<T> listData)
        //{
        //    var client = MongoDBClient<T>.GetCollectionInstance();
        //    //要修改的字段
        //    var t = new T();
        //    var updateList = new List<UpdateDefinition<T>>();
        //    if (listData != null)
        //    {
        //        foreach (var itemU in listData)
        //        {
        //            foreach (var item in itemU.GetType().GetProperties())
        //            {
        //                var attr = item.CustomAttributes.Any(x => x.AttributeType == typeof(BsonIdAttribute));
        //                if (attr) continue;
        //                //id 还得过滤  唉
        //                var type = item.PropertyType.Name;
        //                Console.WriteLine($"属性名称：{item.Name}，类型：{type}，值：{item.GetValue(t)}");
        //                updateList.Add(Builders<T>.Update.Set(item.Name, item.GetValue(t)));
        //            }
        //        }
        //    }
        //    var updatefilter = Builders<T>.Update.Combine(updateList);
        //    return await client.UpdateManyAsync(filter, updatefilter);
        //}

        /// <summary>
        /// 删除一条数据
        /// </summary>

        /// <param name="id">objectId</param>
        /// <returns></returns>
        public static DeleteResult Delete(string id)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            return client.DeleteOne(filter);
        }

        /// <summary>
        /// 异步删除一条数据
        /// </summary>

        /// <param name="id">objectId</param>
        /// <returns></returns>
        public static async Task<DeleteResult> DeleteAsync(ObjectId id, string eqId = "_id")
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //修改条件
            FilterDefinition<T> filter = Builders<T>.Filter.Eq(eqId, id);
            return await client.DeleteOneAsync(filter);
        }

        /// <summary>
        /// 异步删除一条数据
        /// </summary>

        /// <param name="id">objectId</param>
        /// <returns></returns>
        public static async Task<DeleteResult> DeleteModelAsync(FilterDefinition<T> filter)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.DeleteOneAsync(filter);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>

        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        public static DeleteResult DeleteMany(FilterDefinition<T> filter)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return client.DeleteMany(filter);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>

        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        public static DeleteResult DeleteMany(Expression<Func<T, bool>> expression)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.And(builder.Where(expression));
            var client = MongoDBClient<T>.GetCollectionInstance();
            return client.DeleteMany(filter);
        }


        /// <summary>
        /// 异步删除多条数据
        /// </summary>

        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        public static async Task<DeleteResult> DeleteManyAsync(Expression<Func<T, bool>> expression)
        {
            var builder = Builders<T>.Filter;
            var filter = builder.And(builder.Where(expression));
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.DeleteManyAsync(filter);
        }


        /// <summary>
        /// 异步删除多条数据
        /// </summary>

        /// <param name="filter">删除的条件</param>
        /// <returns></returns>
        public static async Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.DeleteManyAsync(filter);
        }

        /// <summary>
        /// 异步删除多条数据
        /// </summary>
        /// <param name="field">字段名称</param>
        /// <param name="value">条件值</param>
        /// <returns></returns>
        public static async Task<DeleteResult> DeleteManyByWhereAsync(string field, object value)
        {
            var builder = Builders<T>.Filter;
            FilterDefinition<T> delfilter = builder.And(builder.Eq(field, value));
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.DeleteManyAsync(delfilter);
        }

        /// <summary>
        /// 根据id查询一条数据
        /// </summary>

        /// <param name="id">objectid</param>
        /// <param name="field">要查询的字段，不写时查询全部</param>
        /// <returns></returns>
        public static T Get(string id, string[] field = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            //指定查询字段
            if (field == null || field.Length == 0)
            {
                return client.Find(filter).FirstOrDefault<T>();
            }

            //不指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            return client.Find(filter).Project<T>(projection).FirstOrDefault<T>();
        }



        /// <summary>
        /// 根据id查询一条数据
        /// </summary>

        /// <param name="id">objectid</param>
        /// <param name="field">要查询的字段，不写时查询全部</param>
        /// <returns></returns>
        public static T Get(Expression<Func<T, bool>> exception, string[] field = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            FilterDefinition<T> filter = Builders<T>.Filter.Where(exception);
            //指定查询字段
            if (field == null || field.Length == 0)
            {
                return client.Find(filter).FirstOrDefault<T>();
            }

            //不指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            return client.Find(filter).Project<T>(projection).FirstOrDefault<T>();
        }

        /// <summary>
        /// 异步根据id查询一条数据
        /// </summary>
        /// <param name="id">objectid</param>
        /// <returns></returns>
        public static async Task<T> GetAsync(string id, string[] field = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            //指定查询字段
            if (field == null || field.Length == 0)
            {
                return await client.Find(filter).FirstOrDefaultAsync();
            }
            //不指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            return await client.Find(filter).Project<T>(projection).FirstOrDefaultAsync();
        }

        public static async Task<T> GetAsync(Expression<Func<T, bool>> exception, string[] field = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            FilterDefinition<T> filter = Builders<T>.Filter.Where(exception);
            //指定查询字段
            if (field == null || field.Length == 0)
            {
                return await client.Find(filter).FirstOrDefaultAsync();
            }
            //不指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            return await client.Find(filter).Project<T>(projection).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 查询集合
        /// </summary>

        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static List<T> GetList(FilterDefinition<T> filter, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return client.Find(filter).ToList();
                //进行排序
                return client.Find(filter).Sort(sort).ToList();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            if (sort == null) return client.Find(filter).Project<T>(projection).ToList();
            //排序查询
            return client.Find(filter).Sort(sort).Project<T>(projection).ToList();
        }

        /// <summary>
        /// 异步查询集合
        /// </summary>

        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static async Task<List<T>> GetListAsync(FilterDefinition<T> filter, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return await client.Find(filter).ToListAsync();
                return await client.Find(filter).Sort(sort).ToListAsync();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            if (sort == null) return await client.Find(filter).Project<T>(projection).ToListAsync();
            //排序查询
            return await client.Find(filter).Sort(sort).Project<T>(projection).ToListAsync();
        }

        /// <summary>
        /// 异步查询集合
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static async Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            var builderSheetCalcChain = Builders<T>.Filter;
            var filter = builderSheetCalcChain.Where(expression);
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return await client.Find(filter).ToListAsync();
                return await client.Find(filter).Sort(sort).ToListAsync();
            }
            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            if (sort == null) return await client.Find(filter).Project<T>(projection).ToListAsync();
            //排序查询
            return await client.Find(filter).Sort(sort).Project<T>(projection).ToListAsync();
        }

        public static async Task<List<T>> GetAllAsync()
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.AsQueryable().ToListAsync();
        }

        public static List<T> GetAll()
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return client.AsQueryable().ToList();
        }


        public static async Task<List<T>> GetAllAsync(FilterDefinition<T> filter)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.Find(filter).ToListAsync();
        }

        public static async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> expression = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            if (expression == null) return await client.AsQueryable().ToListAsync();
            var builderSheetCalcChain = Builders<T>.Filter;
            var filter = builderSheetCalcChain.Where(expression);
            return await client.Find(filter).ToListAsync();
        }

        /// <summary>
        /// 分页查询集合
        /// </summary>

        /// <param name="filter">查询条件</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">页数据量</param>
        /// <param name="count">总条数</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static List<T> GetListByPage(FilterDefinition<T> filter, int pageIndex, int pageSize, out long count, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            count = client.CountDocuments(filter);
            if (pageIndex < 1 || pageSize < 1)
            {
                return null;
            }
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return client.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                //进行排序
                return client.Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();

            //不排序
            if (sort == null) return client.Find(filter).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();

            //排序查询
            return client.Find(filter).Sort(sort).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
        }

        public static List<T> GetListByPage(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, out long count, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            var builderSheetCalcChain = Builders<T>.Filter;
            var filter = builderSheetCalcChain.Where(expression);
            count = client.CountDocuments(filter);
            if (pageIndex < 1 || pageSize < 1)
            {
                return null;
            }
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return client.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                //进行排序
                return client.Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();

            //不排序
            if (sort == null) return client.Find(filter).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();

            //排序查询
            return client.Find(filter).Sort(sort).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
        }

        /// <summary>
        /// 异步分页查询集合
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">页数据量</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static async Task<List<T>> GetListByPageAsync(FilterDefinition<T> filter, int pageIndex, int pageSize, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            if (pageIndex < 1 || pageSize < 1)
            {
                return null;
            }
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return await client.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
                //进行排序
                return await client.Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();

            //不排序
            if (sort == null) return await client.Find(filter).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

            //排序查询
            return await client.Find(filter).Sort(sort).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
        }

        public static async Task<(List<T> Items, long Count)> GetListByPageAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            var builderSheetCalcChain = Builders<T>.Filter;
            var filter = builderSheetCalcChain.Where(expression);
            if (pageIndex < 1 || pageSize < 1)
            {
                return (null, 0);
            }
            var totalCount = await client.Find(filter).CountDocumentsAsync();
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return (await client.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync(), totalCount);
                //进行排序
                return (await client.Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync(), totalCount);
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();

            //不排序
            if (sort == null) return (await client.Find(filter).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync(), totalCount);

            //排序查询
            return (await client.Find(filter).Sort(sort).Project<T>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync(), totalCount);
        }


        /// <summary>
        /// 异步查询集合
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static async Task<T> GetWhereAsync(FilterDefinition<T> filter, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return await client.Find(filter).FirstOrDefaultAsync();
                return await client.Find(filter).Sort(sort).FirstOrDefaultAsync();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            if (sort == null) return await client.Find(filter).Project<T>(projection).FirstOrDefaultAsync();
            //排序查询
            return await client.Find(filter).Sort(sort).Project<T>(projection).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 依据条件得到总数
        /// </summary>
        /// <param name=""></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<long> GetCountWhereAsync(FilterDefinition<T> filter)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            return await client.Find(filter).CountDocumentsAsync();
        }


        /// <summary>
        /// 异步查询集合
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static async Task<List<T>> GetListWhereAsync(FilterDefinition<T> filter, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();
            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return await client.Find(filter).ToListAsync();
                return await client.Find(filter).Sort(sort).ToListAsync();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            if (sort == null) return await client.Find(filter).Project<T>(projection).ToListAsync();
            //排序查询
            return await client.Find(filter).Sort(sort).Project<T>(projection).ToListAsync();
        }

        /// <summary>
        /// 异步查询集合
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="field">要查询的字段,不写时查询全部</param>
        /// <param name="sort">要排序的字段</param>
        /// <returns></returns>
        public static async Task<List<T>> GetListWhereAsync(Expression<Func<T, bool>> exception, string[] field = null, SortDefinition<T> sort = null)
        {
            var client = MongoDBClient<T>.GetCollectionInstance();

            var builderSheetCalcChain = Builders<T>.Filter;
            var filter = builderSheetCalcChain.Where(exception);

            //不指定查询字段
            if (field == null || field.Length == 0)
            {
                if (sort == null) return await client.Find(filter).ToListAsync();
                return await client.Find(filter).Sort(sort).ToListAsync();
            }

            //指定查询字段
            var fieldList = new List<ProjectionDefinition<T>>();
            for (int i = 0; i < field.Length; i++)
            {
                fieldList.Add(Builders<T>.Projection.Include(field[i].ToString()));
            }
            var projection = Builders<T>.Projection.Combine(fieldList);
            fieldList?.Clear();
            if (sort == null) return await client.Find(filter).Project<T>(projection).ToListAsync();
            //排序查询
            return await client.Find(filter).Sort(sort).Project<T>(projection).ToListAsync();
        }



    }
}
