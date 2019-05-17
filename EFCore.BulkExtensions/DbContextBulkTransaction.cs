using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCore.BulkExtensions
{
    internal static class DbContextBulkTransaction
    {
        public static void Execute<T>(DbContext context, IList<T> entities, OperationType operationType,
            BulkConfig bulkConfig, Action<decimal> progress) where T : class
        {
            if (entities.Count == 0) return;
            var tableInfo = TableInfo.CreateInstance(context, entities, operationType, bulkConfig);

            if (operationType == OperationType.Insert && !tableInfo.BulkConfig.SetOutputIdentity)
                SqlBulkOperation.Insert(context, entities, tableInfo, progress);
            else if (operationType == OperationType.Read)
                SqlBulkOperation.Read(context, entities, tableInfo, progress);
            else
                SqlBulkOperation.Merge(context, entities, tableInfo, operationType, progress);
        }
        
        public static async Task ExecuteAsync<T>(DbContext context, IList<T> entities, OperationType operationType, BulkConfig bulkConfig, Action<decimal> progress) where T : class
        {
            if (entities.Count == 0)
            {
                return;
            }
            TableInfo tableInfo = TableInfo.CreateInstance(context, entities, operationType, bulkConfig);

            if (operationType == OperationType.Insert && !tableInfo.BulkConfig.SetOutputIdentity)
            {
                await SqlBulkOperation.InsertAsync(context, entities, tableInfo, progress).ConfigureAwait(false);
            }
            else if (operationType == OperationType.Read)
            {
                await SqlBulkOperation.ReadAsync(context, entities, tableInfo, progress).ConfigureAwait(false);
            }
            else
            {
                await SqlBulkOperation.MergeAsync(context, entities, tableInfo, operationType, progress).ConfigureAwait(false);
            }
        }
    }
}