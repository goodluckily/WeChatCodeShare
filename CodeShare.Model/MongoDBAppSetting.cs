namespace CodeShare.Model
{
    public  class MongoDBAppSetting
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public static string ConnectionString { get; set; } = "mongodb://127.0.0.1:27017";

        /// <summary>
        /// 库
        /// </summary>
        public static string DataBase { get; set; } = "CodeShare";

        /// <summary>
        /// 集合(表)
        /// </summary>
        //public string Collection { get; set; }
    }
}
