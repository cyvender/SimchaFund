using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFund.Data
{
    public class Contributor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool AlwaysInclude { get; set; }
        public int Balance { get; set; }
        public int Id { get; set; }

        public bool Contributed { get; set; }
        public decimal ContributionAmount { get; set; }
        public int Index { get; set; }
    }
}
