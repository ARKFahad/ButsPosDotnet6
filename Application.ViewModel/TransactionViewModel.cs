namespace Application.ViewModel
{
    public class TransactionViewModel
    {
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ThisPeriodPaid { get; set; }
        public decimal ThisPeriodPurchase { get; set; }
        public decimal Balance { get; set; }
    }
}
