using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimchaFund.Data
{
    public class Simcha
    {
        public string SimchaName { get; set; }
        public DateTime SimchaDate { get; set; }
        //public List<Contributor> SimchaContributors {get;set;}
        public decimal Total { get; set; }
        public int ContributorCount { get; set; }
        public int SimchaId { get; set; }
    }
}
