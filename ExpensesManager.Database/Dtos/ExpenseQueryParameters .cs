using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesManager.Database.Dtos
{
    public class ExpenseQueryParameters : PaginationParameters
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? UserId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Receiver { get; set; }
        public string SortBy { get; set; } = "PaymentDate";
        public string SortOrder { get; set; } = "desc";
    }
}
