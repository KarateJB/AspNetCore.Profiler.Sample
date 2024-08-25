using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Profiler.Dal.Models
{
    /// <summary>
    /// View: INFORMATION_SCHEMA.TABLES
    /// </summary>
    [Table("TABLES")]
    [Keyless]
    public class Table
    {
        [Column("TABLE_CATALOG")]
        public string TableCatalog { get; set; }

        [Column("TABLE_SCHEMA")]
        public string TableSchema { get; set; }

        [Column("TABLE_NAME")]
        public string TableName { get; set; }

        [Column("TABLE_TYPE")]
        public string TableType { get; set; }
    }
}
