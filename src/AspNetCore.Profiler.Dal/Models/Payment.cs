using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore.Profiler.Dal.Models
{
    /// <summary>
    /// Transactions
    /// </summary>
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [Column(Order = 1)]
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Item { get; set; }

        [Column(TypeName = "DECIMAL(18,0)")]
        public decimal Amount { get; set; } = 0;

        [Required]
        public DateTimeOffset CreateOn { get; set; }
    }
}
