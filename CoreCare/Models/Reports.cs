namespace CoreCare.Models
{
    public class Report
    {
        public string Id { get; set; }
        public string BenchmarkName { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public string CompanyIcon { get; set; }
        public string Status { get; set; } // "pending", "in-review", "resolved", "rejected"
        public string Date { get; set; }
        public string LastUpdate { get; set; }
        public string Response { get; set; }
        public string TicketId { get; set; }
    }
}