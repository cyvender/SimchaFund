using System.Data.SqlClient;
using System.Dynamic;

namespace SimchaFund.Data
{
    public class SimchaFundDB
    {
        private readonly string _connectionString;

        public SimchaFundDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Contributor> GetContributors()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT C.Id, C.FirstName, C.LastName, C.PhoneNumber, C.AlwaysInclude, SUM(D.Amount) 'Balance' FROM Contributors C
                                JOIN Deposits D
                                ON D.ContributorID = C.Id
                                GROUP BY C.Id, C.LastName, C.FirstName, C.PhoneNumber, C.AlwaysInclude";
            connection.Open();
            var reader = cmd.ExecuteReader();
            List<Contributor> contributors = new List<Contributor>();

            while (reader.Read())
            {
                contributors.Add(new Contributor
                {
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    PhoneNumber = (string)reader["PhoneNumber"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"],
                    Balance = (int)(decimal)reader["Balance"],
                    Id = (int)reader["Id"]
                });
            }

            return contributors;

        }

        public void AddContributor(Contributor contributor, Deposit deposit)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Contributors (FirstName, LastName, PhoneNumber, AlwaysInclude)
                                VALUES (@FirstName, @LastName, @PhoneNumber, @AlwaysInclude)

                                INSERT INTO Deposits (Amount, Date, ContributorID)
                                VALUES (@Amount, @Date, SCOPE_IDENTITY())";
            cmd.Parameters.AddWithValue("@FirstName", contributor.FirstName);
            cmd.Parameters.AddWithValue("@LastName", contributor.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", contributor.PhoneNumber);
            cmd.Parameters.AddWithValue("@AlwaysInclude", contributor.AlwaysInclude);
            cmd.Parameters.AddWithValue("@Amount", deposit.Amount);
            cmd.Parameters.AddWithValue("@Date", deposit.Date);

            cmd.ExecuteNonQuery();
        }

        public void AddListContributors(List<Contributor> contributors)
        {
            foreach(Contributor c in contributors)
            {

            }
        }
        public List<Simcha> GetSimchas()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Simchas";
            connection.Open();
            var reader = cmd.ExecuteReader();
            List<Simcha> simchas = new List<Simcha>();

            while (reader.Read())
            {
                simchas.Add(new Simcha
                {
                    SimchaName = (string)reader["SimchaName"],
                    SimchaDate = (DateTime)reader["SimchaDate"],
                    SimchaId = (int)reader["Id"]
                });
            }

            var simchaContributions = GetSimchaTotalAndCount();

            if (simchaContributions.Count > 0)
            {
                foreach (Simcha s in simchas)
                {
                    var contribution = simchaContributions.Find(sc => sc.SimchaId == s.SimchaId);
                    if (contribution != null)
                    {
                        s.Total = contribution.Total;
                        s.ContributorCount = contribution.ContributorCount;
                    }
                }
            }


            return simchas;
        }

        public List<Simcha> GetSimchaTotalAndCount()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT SimchaID, SUM(Contribution) 'Total' , COUNT(SIMCHAID) AS ContributorCount FROM Contributions
                                GROUP BY SimchaID";
            connection.Open();
            var reader = cmd.ExecuteReader();
            var simchaContributions = new List<Simcha>();

            while (reader.Read())
            {
                simchaContributions.Add(new Simcha
                {
                    SimchaId = (int)reader["SimchaId"],
                    Total = (decimal)reader["Total"],
                    ContributorCount = (int)reader["ContributorCount"]
                });
            }

            return simchaContributions;
        }

        public List<Contributor> GetSimchaContributors(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"SELECT * FROM Contributions
                                WHERE SimchaId = @id";
            cmd.Parameters.AddWithValue("id", id);
            var reader = cmd.ExecuteReader();
            var contributions = new List<Contribution>();

            while (reader.Read())
            {
                contributions.Add(new Contribution
                {
                    SimchaId = (int)reader["SimchaId"],
                    ContributorId = (int)reader["ContributorId"],
                    ContributionAmount = (decimal)reader["Contribution"]
                });
            }

            var contributors = GetContributors();

            foreach (Contributor c in contributors)
            {
                foreach (Contribution contribution in contributions)
                {
                    if (c.Id == contribution.SimchaId)
                    {
                        c.Contributed = true;
                        break;
                    }
                }
            }
            return contributors;
        }

        public void Deposit(Deposit deposit)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Deposits (Amount, Date, ContributorID)
                                VALUES (@Amount, @Date, @Id)";
            cmd.Parameters.AddWithValue("@Amount", deposit.Amount);
            cmd.Parameters.AddWithValue("@Date", deposit.Date);
            cmd.Parameters.AddWithValue("@Id", deposit.Id);

            cmd.ExecuteNonQuery();
        }

        public void AddContributions(List<Contributor> contributors, int simchaId)
        {
            //extracts the contributors who contributed to this simcha and creates a list of contributions
            var contributions = new List<Contribution>();
            foreach(Contributor c in contributors)
            {
                if(c.Contributed)
                {
                    contributions.Add(new Contribution
                    {
                        SimchaId = simchaId,
                        ContributorId = c.Id,
                        ContributionAmount = c.ContributionAmount
                    });
                }
            }

            DeleteContributions(simchaId);

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Contributions (SimchaID, ContributorID, Contribution)
                                VALUES (@SimchaID, @ContributorID, @Contribution)";
            foreach(Contribution c in contributions)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SimchaID", c.SimchaId);
                cmd.Parameters.AddWithValue("@ContributorID", c.ContributorId);
                cmd.Parameters.AddWithValue("@Contribution", c.ContributionAmount);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteContributions(int simchaId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"DELETE Contributions
                                WHERE SimchaID = @SimchaId";
            cmd.Parameters.AddWithValue("@SimchaId", simchaId);
            cmd.ExecuteNonQuery();
        }
    }
}
