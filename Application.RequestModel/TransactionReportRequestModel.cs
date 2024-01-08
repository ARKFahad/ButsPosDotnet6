namespace Application.RequestModel
{
    public class TransactionReportRequestModel
    {
        public string FiscalYearId { get; set; } = "1";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
    }
}
