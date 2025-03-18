//using System.Transactions;
using SimchaFund.Data;

namespace SimchaFund.Web.Models
{
    public class HistoryViewModel
    {
        public List<Transaction> Transactions { get; set; }
        public Contributor Contributor { get; set; }
    }
}
