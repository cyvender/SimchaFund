using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFund.Data
{
    public class Contribution
    {
        public int SimchaId { get; set; }
        public string SimchaName { get; set; }
        public DateTime SimchaDate { get; set; }
        public int ContributorId { get; set; }
        public decimal ContributionAmount { get; set; }
        public bool Contributed { get; set; }
    }
}
