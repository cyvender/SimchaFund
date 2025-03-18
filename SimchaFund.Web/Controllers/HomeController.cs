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
                Simchas = sfdb.GetSimchas(),
                TotalContributors = sfdb.GetContributors().Count
            });
        }

        [HttpPost]
        public IActionResult AddSimcha(Simcha simcha)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            sfdb.AddSimcha(simcha);
            return Redirect("/home/index");
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
        public IActionResult EditContributor(Contributor contributor)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            sfdb.EditContributor(contributor);
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
            var contributors = sfdb.ShowSimchaContributors(simchaId);
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
        public IActionResult UpdateContributions(List<Contribution> contributions, int simchaId)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            sfdb.AddContributions(contributions, simchaId);
            return Redirect("/home/index");
        }

        public IActionResult History(int contributorId)
        {
            var sfdb = new SimchaFundDB(_connectionString);
            
            return View(
                new HistoryViewModel
                {
                    Transactions = sfdb.GetHitstory(contributorId),
                    Contributor = sfdb.GetContributorById(contributorId)
                });
        }
    }
}
