using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CodeShare.Model
{
    public class BaseEntity
    {
        /// <summary>
        ///  主键Id
        /// </summary>
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        //[BsonSerializer(typeof(MongoDateTimeSerializer))]
        public DateTime EditDateTime { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        ////[BsonSerializer(typeof(MongoDateTimeSerializer))]
        public DateTime CreateDateTime { get; set; } = DateTime.Now;
    }
}
