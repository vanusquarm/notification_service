using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTBStatementService.Models
{
    [Table("Profile", Schema = "dbo")]
    public class CustomerProfile
    {
        [Key]
        [Column("CustomerNo")]
        public string CustomerNo { get; set; } = string.Empty;

        public string? BranchCode { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? ExportFormat { get; set; } // e.g., "pdf", "xls"
        public int Status { get; set; } // e.g., 3 for Active
        public string? Allaccounts { get; set; }

        // Frequency Flags
        public int Daily { get; set; }
        public int Weekly { get; set; }
        public int Monthly { get; set; }

        // Frequency Options (e.g., day of week or day of month)
        public string? DailyOpt { get; set; }
        public string? WeeklyOpt { get; set; }
        public string? MonthlyOpt { get; set; }

        // Tracking last sent dates
        public DateTime? LastsentDaily { get; set; }
        public DateTime? LastsentWeekly { get; set; }
        public DateTime? LastsentMonthly { get; set; }
    }
}
