using System;
using System.Collections.Generic;
using StackExchange.Profiling.Storage;

namespace AspNetCore.Profiler.Mvc.Utils
{
    /// <summary>
    /// Custom SqlServerStorage
    /// </summary>
    public class CustomSqlServerStorage : SqlServerStorage, IDisposable
    {
        public CustomSqlServerStorage(string connectionString) : base(connectionString)
        {

        }

        public IEnumerable<string> CreateSqls
        {
            get {
                return base.GetTableCreationScripts();
            }
        }

        public void Dispose()
        {
        }
    }
}
