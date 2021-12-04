using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.MongoDBRepository
{
    public class MongoDBHostClient
    {
        private volatile static MongoDBHostClient singleton;
        private static object syncRoot = new Object();
        private MongoDBHostClient()
        {

        }
        public static MongoDBHostClient getSingleton()
        {
            if (singleton == null)
            {
                lock (syncRoot)
                {
                    if (singleton == null)
                    {
                        singleton = new MongoDBHostClient();
                    }
                }
            }
            return singleton;
        }

        /// <summary>
        /// 初始话 所有相关数据表 并且建立好相关索引
        /// </summary>
        /// <returns></returns>
        public async Task IninintaAllMongoDBTableAndIndexs()
        {
            //var hostAll = GetInitialMongoDBHostAll();

            //初始化数据库表索引的

            //var IndexDocument = await MongoDBClient<LuckDocument>.SettCollectionIndexs(hostAll.hostDocument, "groupGuidKey");
            //var IndexDocument1 = await MongoDBClient<LuckDocument>.SettCollectionIndexs(hostAll.hostDocument, "isDel");

            //var IndexSheet = await MongoDBClient<SheetDocument>.SettCollectionIndexs(hostAll.hostSheet, "groupGuidKey", "index");

            //var IndexCellRecord = await MongoDBClient<SheetCellRecord>.SettCollectionIndexs(hostAll.hostCellRecord, "SheetDocumentId", "r", "c");

            //var IndexCellHistoricalRecord = await MongoDBClient<SheetCellHistoricalRecord>.SettCollectionIndexs(hostAll.hostCellHistoricalRecord, "SheetDocumentId", "r", "c");

            //var IndexConfig = await MongoDBClient<SheetConfig>.SettCollectionIndexs(hostAll.hostConfig, "SheetDocumentId");

            //var IndexCalcChain = await MongoDBClient<SheetCalcChain>.SettCollectionIndexs(hostAll.hostCalcChain, "SheetDocumentId");

            //var IndexImages = await MongoDBClient<SheetImages>.SettCollectionIndexs(hostAll.hostImages, "SheetDocumentId");

            //var IndexSheetLog = await MongoDBClient<LuckSystemLog>.SettCollectionIndexs(hostAll.hostSheetLog, "mark");

            //var IndexLuckFiles = await MongoDBClient<LuckFiles>.SettCollectionIndexs(hostAll.hostLuckFiles, "fileMd5");
        }
    }
}
