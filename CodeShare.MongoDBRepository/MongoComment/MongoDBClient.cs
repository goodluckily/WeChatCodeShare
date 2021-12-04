using CodeShare.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.MongoDBRepository
{
    public class MongoDBClient<T> where T : class
    {
        private MongoDBClient()
        {
        }

        private static IMongoDatabase _database { get; set; }
        private static readonly object lockObj = new object();

        private static IMongoDatabase GetMongoDB()
        {
            if (_database == null)
            {
                lock (lockObj)
                {
                    if (_database == null)
                    {
                        var ConnectionString = MongoDBAppSetting.ConnectionString;
                        var client = new MongoClient(ConnectionString);
                        var DataBase = MongoDBAppSetting.DataBase;
                        _database = client.GetDatabase(DataBase);
                    }
                }
            }
            return _database;
        }

        /// <summary>
        /// 获取集合实例
        /// </summary>
        /// <param name="host">连接字符串，库，集合</param>
        /// <returns></returns>
        public static IMongoCollection<T> GetCollectionInstance(string Collection = "")
        {
            var _db = GetMongoDB();
            if (string.IsNullOrWhiteSpace(Collection)) Collection = typeof(T).Name;
            var coliection = _db.GetCollection<T>(Collection);
            return coliection;
        }

        /// <summary>
        /// 初始化GridFSBucket
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static GridFSBucket GetGridFSBucket(string BucketName = "")
        {
            var _db = GetMongoDB();
            if (string.IsNullOrWhiteSpace(BucketName))
                BucketName = "File";
            var bucket = new GridFSBucket(_db, new GridFSBucketOptions
            {
                BucketName = BucketName,         //设置根节点名
                ChunkSizeBytes = 1024 * 1024,   //设置块的大小为1M
                //WriteConcern = WriteConcern.WMajority,     //写入确认级别为majority
                //ReadPreference = ReadPreference.Secondary  //优先从从节点读取
            }); //这个是初始化gridFs存储的
            return bucket;
        }

        /// <summary>
        /// 创建排序索引可以多个
        /// </summary>
        /// <param name="host">实例</param>
        /// <param name="fields">索引字段名称 可以多个</param>
        /// <returns>索引别名</returns>
        public static async Task<string> SettCollectionIndexs(params string[] fields)
        {
            var clientDocument = GetCollectionInstance();
            var indexBuilder = Builders<T>.IndexKeys;
            //组装多个
            var IndexKeys = new List<IndexKeysDefinition<T>>();
            var filedList = new List<string>(fields);
            foreach (var filed in filedList)
            {
                IndexKeys.Add(indexBuilder.Ascending(filed));
            }
            var createModelIndex = new CreateIndexModel<T>(indexBuilder.Combine(IndexKeys));
            var resilt = await clientDocument.Indexes.CreateOneAsync(createModelIndex);
            return resilt;
        }
    }
}
