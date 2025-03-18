using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Transactions;

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
            Balance(contributors);
            return contributors;
        }
        public Contributor GetContributorById(int contributorId)
        {
            var contributor = GetContributors().FirstOrDefault(c => c.Id == contributorId);
            return contributor;
        }
        //Because I couldn't figure out how to get this all with one Sql query
        public void Balance(List<Contributor> contributors)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT ContributorID, SUM(C.ContribUtion) 'TotalContributions' FROM Contributions C
                                GROUP BY ContributorID";
            connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read()) 
            {
                var totalContributions = (int)(decimal)reader["TotalContributions"];
                var contributorID = (int)reader["ContributorID"];
                foreach (Contributor c in contributors)
                {
                    if(c.Id == contributorID)
                    {
                        c.Balance -= totalContributions;
                    }
                }
            }
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
        public void EditContributor(Contributor contributor)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"UPDATE Contributors
                                SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber, AlwaysInclude = @AlwaysInclude
                                WHERE Id = @id";
            cmd.Parameters.AddWithValue("@FirstName", contributor.FirstName);
            cmd.Parameters.AddWithValue("@LastName", contributor.LastName);
            cmd.Parameters.AddWithValue("@PhoneNumber", contributor.PhoneNumber);
            cmd.Parameters.AddWithValue("@AlwaysInclude", contributor.AlwaysInclude);
            cmd.Parameters.AddWithValue("@id", contributor.Id);

            cmd.ExecuteNonQuery();
        }

        public List<Transaction> GetHitstory(int contributorId)
        {
            List<Transaction> transactions = GetContributionHistoryById(contributorId);

            transactions.AddRange(GetDepositHistoryById(contributorId));

            return transactions.OrderByDescending(t => t.Date).ToList(); 
        }
        public List<Transaction> GetContributionHistoryById(int contributorId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM SIMCHAS S
                                JOIN Contributions C
                                ON S.Id = C.SimchaID
                                WHERE ContributorID = @contributorId";
            connection.Open();
            cmd.Parameters.AddWithValue("@contributorId", contributorId);

            var reader = cmd.ExecuteReader();
            var contributions = new List<Transaction>();

            while (reader.Read())
            {
                contributions.Add(new Transaction
                {
                    Action = $"Contribution for {(string)reader["SimchaName"]}",
                    Date = (DateTime)reader["SimchaDate"],
                    Amount = -(decimal)reader["Contribution"]
                });
            }

            return contributions;
        }
        public List<Transaction> GetDepositHistoryById(int contributorId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Deposits
                                WHERE ContributorID = @ContributorID";
            connection.Open();
            cmd.Parameters.AddWithValue("@contributorId", contributorId);

            var reader = cmd.ExecuteReader();
            var deposits = new List<Transaction>();

            while (reader.Read())
            {
                deposits.Add(new Transaction
                {
                    Action = "Deposit",
                    Date = (DateTime)reader["Date"],
                    Amount = (decimal)reader["Amount"]
                });
            }

            return deposits;
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
        //Because I couldnt figure out how to get this all with one Sql query
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
        public void AddSimcha(Simcha simcha)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Simchas (SimchaName, SimchaDate)
                                VALUES (@SimchaName, @SimchaDate)";
            cmd.Parameters.AddWithValue("@SimchaName", simcha.SimchaName);
            cmd.Parameters.AddWithValue("@SimchaDate", simcha.SimchaDate);

            cmd.ExecuteNonQuery();
        }
        
     
        public List<Contributor> ShowSimchaContributors(int id)
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
                    ContributionAmount = (decimal)reader["Contribution"],
                    Contributed = false
                });
            }

            var contributors = GetContributors();


            //should be able to do if contains or find and then set, instead
            foreach (Contributor contributor in contributors)
            {
                foreach (Contribution contribution in contributions)
                {
                    if (contributor.Id == contribution.ContributorId)
                    {
                        contributor.Contributed = true;
                        contributor.ContributionAmount = contribution.ContributionAmount;
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


        public void AddContributions(List<Contribution> contributions, int simchaId)
        {

            DeleteContributions(simchaId);

            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Contributions (SimchaID, ContributorID, Contribution)
                                VALUES (@SimchaID, @ContributorID, @Contribution)";
            foreach(Contribution c in contributions)
            {
                if(c.Contributed)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@SimchaID", c.SimchaId);
                    cmd.Parameters.AddWithValue("@ContributorID", c.ContributorId);
                    cmd.Parameters.AddWithValue("@Contribution", c.ContributionAmount);
                    cmd.ExecuteNonQuery();
                }
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
