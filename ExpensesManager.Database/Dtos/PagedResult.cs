using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesManager.Database.Dtos
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationMetadata Pagination { get; set; } = new();
    }
}
