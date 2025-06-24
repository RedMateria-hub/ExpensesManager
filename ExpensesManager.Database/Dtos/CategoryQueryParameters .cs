using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesManager.Database.Dtos
{
    public class CategoryQueryParameters : PaginationParameters
    {
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc";
    }
}
