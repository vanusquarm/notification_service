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
        public int? ExportFormat { get; set; } // e.g., 1-"pdf", 2-"xls"
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
        public DateTime? Date { get; internal set; }
        public DateTime? FromDate { get; internal set; }
        public string? FinacleAccount { get; internal set; }
        public string? ExportFile { get; internal set; }
        public string? DefaultExt { get; internal set; }
        public int Index { get; internal set; }
        public int Branch { get; internal set; }
        public int UserID { get; internal set; }
        public int StmtType { get; internal set; }
        public int CurType { get; internal set; }
        public int ActType { get; internal set; }
        public DateTime? ToDate { get; internal set; }
        public int TryCount { get; internal set; }
    }
}
