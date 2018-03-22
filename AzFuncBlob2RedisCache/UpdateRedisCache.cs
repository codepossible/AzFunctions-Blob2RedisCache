using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzFuncBlob2RedisCache
{
    using Helpers;


    /// <summary>
    /// Azure function to update Azure Redis cache based on the key-value file provided in Azure BLOB storage.
    /// The Azure function code is triggered when the specified file in BLOB storage is updated.
    /// </summary>
    public static class UpdateRedisCache
    {

        #region Function Code

        /// <summary>
        /// Azure function to responds to changes on the specificed blob in Azure Storage containing key-value pair to the update cache.
        /// </summary>
        /// <param name="myBlob">BLOB Stream reference</param>
        /// <param name="log">Trace logger instance</param>
        /// <returns></returns>
        [FunctionName("UpdateRedisCache")]
        public static async Task Run([BlobTrigger("%SourceBlob%", Connection = "SourceAzureBlobStorageConnection")]Stream myBlob, TraceWriter log)
        {
            log.Info($"Data Changed on Blob - {ConfigurationHelper.GetConfigurationValue("SourceBlob")} \t Size: {myBlob.Length} Bytes");

            try
            {
                string redisConnectionString = ConfigurationHelper.GetConnectionString("TargetAzureRedisCacheConnection");
                var redisCacheHelper = new RedisCacheHelper(redisConnectionString);

                int batchSize = int.TryParse(ConfigurationHelper.GetConfigurationValue("BatchSize"), out batchSize) ? batchSize : 1000;
                log.Info($"Processing Batch Size - {batchSize} ");

                int keysWrittenToCache = 0;

                keysWrittenToCache = await ReadBlobAndUpdateRedisCache(myBlob, redisCacheHelper, batchSize);

                log.Info($"Process complete. {keysWrittenToCache} keys written to Redis cache");
            }
            catch (Exception ex)
            {
                log.Error($"Error occurred while processing BLOB changes. Error Info: {ex.Message}", ex);
            }

        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Reads the BLOB, updates the Redis Cache in bulk manner based on batch size.
        /// </summary>
        /// <param name="myBlob">BLOB Stream reference</param>
        /// <param name="redisCacheHelper">Redis Cache loader helper</param>
        /// <param name="batchSize">batch size for bulk load</param>
        /// <returns></returns>
        private static async Task<int> ReadBlobAndUpdateRedisCache(Stream myBlob, RedisCacheHelper redisCacheHelper, int batchSize)
        {

            int keysWrittenToCache = 0;

            /* Read contents of the updated BLOB */
            using (var reader = new StreamReader(myBlob))
            {
                var counter = 0;
                var batch = new Dictionary<string, string>();

                while (!reader.EndOfStream)
                {
                    // assuming data format of xxxx,yyyyy
                    var line = reader.ReadLine()?.Split(new char[] { ',' });

                    /* Ignore lines which could not be read or do not have at least one delimiter. */
                    if (line != null && line.Length > 1)
                    {
                        batch.Add(line[0], line[1]);
                        counter++;
                    }

                    /* if the read count has reached batch size, write the key-value pair to Redis Cache 
                       clear the batch and reset the read counter.
                    */
                    if (counter == batchSize)
                    {
                        await redisCacheHelper.BatchWriteKeyValuePairAsync(batch);
                        keysWrittenToCache += counter;
                        counter = 0;
                        batch.Clear();
                    }
                }

                /* 
                  if items are still left in the batch, as count could not meet 
                  the batch size, write remaining key-value pair to Redis Cache.
                */
                if (batch.Count > 0)
                {
                    await redisCacheHelper.BatchWriteKeyValuePairAsync(batch);
                    keysWrittenToCache += batch.Count;
                    batch.Clear();
                }
            }
            return keysWrittenToCache;
        }

        #endregion

    }
}
