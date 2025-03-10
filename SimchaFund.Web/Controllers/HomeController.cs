using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using SimchaFund.Data;
using SimchaFund.Web.Models;

namespace SimchaFund.Web.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString =
            "Data Source=.\\sqlexpress;Initial Catalog=SimchaFund;Integrated Security=True;";

        public IActionResult Index()
        {
            var sfdb = new SimchaFundDB(_connectionString);

            return View(new SimchaViewModel
            {
                Simchas = sfdb.GetSimchas()
            });
        }

        public IActionResult Contributors()
        {
            var sfdb = new SimchaFundDB(_connectionString);
            
            return View(new ContributorViewModel
            {
                Contributors = sfdb.GetContributors()
            });
        }

        [HttpPost]
        public IActionResult AddContributor(Contributor contributor, Deposit deposit)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            sfdb.AddContributor(contributor, deposit);

            return Redirect("/home/contributors");
        }

        [HttpPost] 
        public IActionResult Deposit(Deposit deposit)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            sfdb.Deposit(deposit);
            return Redirect("/home/contributors");
        }

        public IActionResult SimchaContributors(int simchaId)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            var contributors = sfdb.GetSimchaContributors(simchaId);
            for (int i = 0; i < contributors.Count; i++)
            {
                contributors[i].Index = i;
            }

            return View(new SimchaContributorViewModel
            {
                Contributors = contributors,
                SimchaId = simchaId
            });
        }

        [HttpPost]
        public IActionResult UpdateContributions(List<Contributor> contributors, int simchaId)
        {

            var sfdb = new SimchaFundDB(_connectionString);
            sfdb.AddContributions(contributors, simchaId);
            return Redirect("/home/index");
        }

    }
}
