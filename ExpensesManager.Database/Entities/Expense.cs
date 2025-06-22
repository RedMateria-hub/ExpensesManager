using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesManager.Database.Entities
{
    public class Expense : BaseEntity
    {
        public double Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Receiver { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
