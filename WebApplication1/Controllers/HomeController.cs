using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.UI;
using WebApplication1.Interfaces;


namespace WebApplication1.Controllers
{
    public class HomeLoanAccount : IAccount
    {
        public DateTimeOffset OpenedDate { get; }
        public string CustomerName { get; }
        public string AccountNumber { get; }
        public decimal Balance { get; }
        
    }
    public class SavingsAccount : IAccount
    {
        public DateTimeOffset OpenedDate { get; }
        public string CustomerName { get; }
        public string AccountNumber { get; }
        public decimal Balance { get; }  
        
    }

    public class Statement : IStatementRow
    {
        public IAccount Account { get; }
        public DateTimeOffset Date { get; }
        public decimal Amount { get; }
        public decimal Balance { get; }
        public string Description { get; }
        
    }
    public class HomeController : Controller//, IReadifyBank
    {
        //This is the database path which needs to be hardcoded initially
        private string source = "Data Source = /Applications/Rider 2018.1.4.app/Contents/bin/identifier.sqlite";

        public IList<IAccount> AccountList { get; }
        public IList<IStatementRow> TransactionLog { get; }
               
        public IAccount OpenHomeLoanAccount(string customerName, DateTimeOffset openedDate)
        {           
            IAccount homeLoan = new HomeLoanAccount();
            
            DataTable dt = new DataTable();
            SQLiteConnection conn = new SQLiteConnection(this.source);
            conn.Open();
            
            //in order to generate account number
            //we need to keep track of the number of account in the db
            String homeQuery = "UPDATE accountType SET counter = counter + 1 where name='homeLoan'";
            SQLiteDataAdapter sda = new SQLiteDataAdapter(homeQuery, conn);
            sda.Fill(dt);
            dt.Clear();
            
            //now select the last element in the db
            //so we can assign the account number correctly
            homeQuery = "SELECT counter FROM accountType where name='homeLoan' ";
            SQLiteDataAdapter da = new SQLiteDataAdapter(homeQuery, conn);
            da.Fill(dt);
            int number = Int32.Parse(dt.Rows[0].ItemArray[0].ToString());
           
            //gather the account number
            string code = "LN";
            string accountNo = accNoGenerator(code, number);
            Console.WriteLine(accountNo);
            dt.Clear();
            string date = DateTime.Now.ToString();
            
            //insert new account into the db
            homeQuery = "Insert into account(dateOpened, accountNumber, balance, customerName) " +
                        "values ('"+date+"', '"+accountNo+"', '"+0.0+"', '"+customerName+"')";
            SQLiteDataAdapter newDa = new SQLiteDataAdapter(homeQuery, conn);
            newDa.Fill(dt);
            Response.Redirect("AccountLister");
            return homeLoan;
        }
        
        public IAccount OpenSavingAccount(string customerName, DateTimeOffset openedDate)
        {
            IAccount savingAccount = new SavingsAccount();                       
            DataTable dt = new DataTable();
            SQLiteConnection conn = new SQLiteConnection(this.source);
            conn.Open();
            //in order to generate account number
            //we need to keep track of the number of account in the db
            String homeQuery = "UPDATE accountType SET counter = counter + 1 where name='savings'";
            SQLiteDataAdapter sda = new SQLiteDataAdapter(homeQuery, conn);
            sda.Fill(dt);
            dt.Clear();
            
            //now select the last element in the db
            //so we can assign the account number correctly
            homeQuery = "SELECT counter FROM accountType where name='savings' ";
            SQLiteDataAdapter da = new SQLiteDataAdapter(homeQuery, conn);
            da.Fill(dt);
            int number = Int32.Parse(dt.Rows[0].ItemArray[0].ToString());
           
            //gather the account number
            string code = "SV";
            string accountNo = accNoGenerator(code, number);
            Console.WriteLine(accountNo);
            dt.Clear();
            string date = DateTime.Now.ToString();
            
            //insert new account into the db
            homeQuery = "Insert into account(dateOpened, accountNumber, balance, customerName, isWithdrawable) " +
                        "values ('"+date+"', '"+accountNo+"', '"+0.0+"', '"+customerName+"', '"+true+"')";
            SQLiteDataAdapter newDa = new SQLiteDataAdapter(homeQuery, conn);
            newDa.Fill(dt);
            Response.Redirect("AccountLister");
            return savingAccount;
        }

        //this function generates the account number in the required form
        public string accNoGenerator(string code, int number)
        {           
            string accNo = code + "-" + number.ToString().PadLeft(6, '0');
            return accNo;
        }
   
        public ActionResult Login()
        {
            return View();
        }

        //This method handles the login operation
        //Upon success it will redirect to Index page
        //This feature has not been implemented to the project
        [HttpPost]
        public ActionResult PerformLogin(string username, string password)
        {
            Console.WriteLine(username);
            Console.WriteLine(password);
            return View("Index", "_Layout");
        }
        
        //This is the main page which provides access to required actions
        public ActionResult Index()
        {
            return View();
        }

        //This function gathers the balance of the account
        public string GetAvailableFunds(string accNo)
        {
            try
            {
                DataTable dt = new DataTable();           
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select balance from account where accountNumber = '"+accNo+"'";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);          
                string amount = dt.Rows[0].ItemArray[0].ToString();
                return amount;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //This function creates new customer where fields are supplied from the user 
        //and stores into the database under customer table
        public ActionResult NewCustomer(string username, int password)
        {
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();             
                String query = "INSERT INTO customer(name, password) values ('"+username+"', '"+password+"')";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(query, conn);
                sda.Fill(dt);
                
                //Now select all of the customers to list them in the view
                String customerQuery = "select name from customer";
                SQLiteDataAdapter da = new SQLiteDataAdapter(customerQuery, conn);
                da.Fill(dt);
                List<string> customerNames = new List<string>();
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        customerNames.Add((string)item);
                        Console.WriteLine(item);
                    }
                }
                //Render ListCustomer Page with List of the customers
                //so that it can be verified                 
                ViewBag.CustomerList = customerNames;
                return View("ListCustomers");
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }
        }

        //This is Create Customer Interface
        //User is able to add a new customer to db from this view
        public ActionResult CreateCustomer()
        {
            ViewBag.Message = "Create New Customer.";         
            return View();
        }
                
        //get all of the accounts from the database
        public ActionResult AccountLister()      
        {
            ViewBag.Message = "List Accounts.";                                          
            try
            {
                DataTable dt = new DataTable();
                
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from account ORDER BY customerName ASC;";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);               
                return View(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }
                 
        }
        
        //List all of the transactions in the database 
        //Transaction Log
        public ActionResult AllTransactions()      
        {
            ViewBag.Message = "List of Transactions.";                                          
            try
            {
                DataTable dt = new DataTable();                
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from transactions ORDER BY date ASC;";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);                
                return View(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }
                 
        }
        
        //Get most recent 5 transactions from the database for a given account
        //Mini Statement
        public ActionResult TransactionLister(string account)      
        {
            ViewBag.Message = "List of Most Recent Transactions";                                          
            try
            {
                DataTable dt = new DataTable();                
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from transactions where account='"+account+"' ORDER BY date ASC limit 5;";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);                
                return View(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }
                 
        }

        //Open a new account page
        public  ActionResult OpenAccount()
        {
            
            ViewBag.Message = "Open a New Account.";                      
            List<string> customerNames = new List<string>();
            List<string> accountTypes = new List<string>();
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                Console.WriteLine("good things happened :)");
                String customerQuery = "select name from customer";
                String accountQuery = "select name from accountType";               
                SQLiteDataAdapter sda = new SQLiteDataAdapter(customerQuery, conn);
                sda.Fill(dt);
                
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        customerNames.Add((string)item);
                    }
                }
                dt.Clear();
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);
                da.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accountTypes.Add((string)item);
                    }
                }
       
                ViewBag.CustomerList = customerNames;
                ViewBag.AccountTypes = accountTypes;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }

            return View("OpenAccount");
        }

        //This function creates new home loan account
        //Since there is no setter given in the interface, a custom function is developed & used
        public ActionResult NewHomeLoan()
        {
            ViewBag.Message = "Open a New Home Loan.";                      
            List<string> customerNames = new List<string>();
            List<string> accountTypes = new List<string>();
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                Console.WriteLine("good things happened :)");
                String customerQuery = "select name from customer";
                String accountQuery = "select name from accountType";               
                SQLiteDataAdapter sda = new SQLiteDataAdapter(customerQuery, conn);
                sda.Fill(dt);
                
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        customerNames.Add((string)item);
                    }
                }
                dt.Clear();
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);
                da.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accountTypes.Add((string)item);
                    }
                }
       
                ViewBag.CustomerList = customerNames;
                ViewBag.AccountTypes = accountTypes;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }

            return View();
        }
        //This function creates new savings account
        //Since there is no setter given in the interface, a custom function is developed & used
        public ActionResult NewSavingsAccount()
        {
            ViewBag.Message = "Open a New Savings Account.";                      
            List<string> customerNames = new List<string>();
            List<string> accountTypes = new List<string>();
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                Console.WriteLine("good things happened :)");
                String customerQuery = "select name from customer";
                String accountQuery = "select name from accountType";               
                SQLiteDataAdapter sda = new SQLiteDataAdapter(customerQuery, conn);
                sda.Fill(dt);
                
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        customerNames.Add((string)item);
                    }
                }
                dt.Clear();
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);
                da.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accountTypes.Add((string)item);
                    }
                }
       
                ViewBag.CustomerList = customerNames;
                ViewBag.AccountTypes = accountTypes;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }

            return View();
        }
        
        //Deposit page
        public ActionResult MakeDeposit()
        {
            ViewBag.Message = "Deposit into a Selected Account.";                      
            List<string> accounts = new List<string>();

            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                String accountQuery = "select accountNumber from account";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(accountQuery, conn);
                sda.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accounts.Add((string)item);
                    }
                }
          
            }
            catch (Exception e)
            {
                throw;
            }
            ViewBag.Accounts = accounts;

            return View();
        }
        
        //Balance page
        public ActionResult Balance()
        {
            ViewBag.Message = "Display Balance";                      
            List<string> accounts = new List<string>();

            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                String accountQuery = "select accountNumber from account";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(accountQuery, conn);
                sda.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accounts.Add((string)item);
                    }
                }
          
            }
            catch (Exception e)
            {
                throw;
            }
            ViewBag.Accounts = accounts;

            return View();
        }
        
        //Withdraw page
        //Only savings accounts will be selected!
        public ActionResult Withdrawal()
        {
            ViewBag.Message = "Withdraw from the Selected Account.";                      
            List<string> accounts = new List<string>();

            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                String accountQuery = "select accountNumber from account where isWithdrawable='True' ";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(accountQuery, conn);
                sda.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accounts.Add((string)item);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            ViewBag.Accounts = accounts;

            return View();
        }
       
        //Transfer page
        //from accounts are selected from savings accounts only!
        public ActionResult Transfer()
        {
            ViewBag.Message = "Withdraw from the Selected Account.";                      
            List<string> fromAccounts = new List<string>();
            List<string> toAccounts = new List<string>();

            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();
                String accountQuery = "select accountNumber from account where isWithdrawable='True' ";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(accountQuery, conn);
                sda.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        fromAccounts.Add((string)item);
                    }
                }
                dt.Clear();
                accountQuery = "select accountNumber from account ";
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);
                da.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        toAccounts.Add((string)item);
                    }
                }
            }
            
            catch (Exception e)
            {
                throw;
            }
            ViewBag.From = fromAccounts;
            ViewBag.To = toAccounts;

            return View();
        }
        //This function performs deposit to a selected account
        //Since there is no setter given in the interface, a custom function is developed & used
        public void Depositer(string accNo, decimal depositAmount, string description)
        {
            try
            {
                DataTable dt = new DataTable();           
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from account where accountNumber = '"+accNo+"'";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);          
                string dateOpened = dt.Rows[0].ItemArray[0].ToString();
                DateTime parsedDate = DateTime.Parse(dateOpened);
                string customerName = dt.Rows[0].ItemArray[3].ToString();
                decimal balance = decimal.Parse(dt.Rows[0].ItemArray[2].ToString());
                decimal targetBalance = balance + depositAmount;             
                dt.Clear();
                
                //now update account balance in the database
                //this is where I perform the deposit
                String depositQuery = "UPDATE account SET balance = '"+targetBalance+"' where accountNumber ='"+accNo+"' ";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(depositQuery, conn);               
                sda.Fill(dt);
                string date = DateTimeOffset.Now.ToString();
                //now document the action
                //save them onto the database
                dt.Clear();
                String logQuery = "INSERT INTO transactions(account, date, amount, balance, description) values ('"+accNo+"', '"+date+"', '"+depositAmount+"', '"+targetBalance+"', '"+description+"')";
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(logQuery, conn);               
                dataAdapter.Fill(dt);
         
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        //This function performs withdrawal from the selected account
        //Since there is no setter given in the interface, a custom function is developed & used
        public void MakeWithdraw(string accNo, decimal withdrawAmount, string description)
        {
            try
            {
                DataTable dt = new DataTable();           
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from account where accountNumber = '"+accNo+"'";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);          
                string dateOpened = dt.Rows[0].ItemArray[0].ToString();
                DateTime parsedDate = DateTime.Parse(dateOpened);
                string customerName = dt.Rows[0].ItemArray[3].ToString();
                decimal balance = decimal.Parse(dt.Rows[0].ItemArray[2].ToString());
                decimal targetBalance = balance - withdrawAmount;    
                dt.Clear();
                
                //now update account balance in the database
                //this is where I perform the deposit
                String depositQuery = "UPDATE account SET balance = '"+targetBalance+"' where accountNumber ='"+accNo+"' ";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(depositQuery, conn);               
                sda.Fill(dt);
                string date = DateTimeOffset.Now.ToString();
                //now document the action
                //save them onto the database
                dt.Clear();
                String logQuery = "INSERT INTO transactions(account, date, amount, balance, description) values ('"+accNo+"', '"+date+"', '-"+withdrawAmount+"', '"+targetBalance+"', '"+description+"')";
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(logQuery, conn);               
                dataAdapter.Fill(dt);
         
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        //This function transfers the entered amount which is entered by the user
        //Since there is no setter given in the interface, a custom function is developed & used
        public void MakeTransfer(string from, string to, decimal transferAmount, string description)
        {
            Depositer(to, transferAmount, description);
            MakeWithdraw(from, transferAmount, description);         
        }
        
        public void PerformDeposit(IAccount account, decimal amount, string description, DateTimeOffset depositDate)
        {
            
            
        }

        public ActionResult MiniStatement()
        {
            ViewBag.Message = "Display Recent Transactions of an Account";                      
            List<string> accounts = new List<string>();
            List<string> transactions = new List<string>();
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();               
                String accountQuery = "select accountNumber from account";                            
                SQLiteDataAdapter sda = new SQLiteDataAdapter(accountQuery, conn);
                sda.Fill(dt);
                              
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accounts.Add((string)item);
                    }
                }

               
                ViewBag.Accounts = accounts;
                return View(dt);

            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }        
        }

        public ActionResult Calculate()
        {                     
            List<string> accounts = new List<string>();

            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(this.source);
                conn.Open();
                String accountQuery = "select accountNumber from account";
                SQLiteDataAdapter sda = new SQLiteDataAdapter(accountQuery, conn);
                sda.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        accounts.Add((string)item);
                    }
                }
          
            }
            catch (Exception e)
            {
                throw;
            }
            ViewBag.Accounts = accounts;

            return View();
        }

        //This function returns the interest rate of an account type
        public string GetInterestRate(string accNo)
        {
            try
            {
                string typeName;
                typeName = accNo[0] == 'L' ?  "homeLoan" : "savings";
                DataTable dt = new DataTable();           
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select rate, term from accountType where name = '"+typeName+"'";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);          
                string rate = dt.Rows[0].ItemArray[0].ToString();
                string term = dt.Rows[0].ItemArray[1].ToString();
                return rate +"% "+ term;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        
        //This function calculates the interest amount for certain number of days
        //Interest rates are stored in the database
        //User enters the number of days from the view
        public decimal CalculateInterest(string account, int numberofDays, decimal amount)
        {   
            //Function GetInterestRate will return rate as a string for the given account number
            //We need to split the string and store into an array
            //First element in the array gives us the rate
            string[] rater = GetInterestRate(account).Split('%');
            double rate = Double.Parse(rater[0]);
            //Home loan rate is annual
            rate = rater[1] == " monthly" ? (rate/100) : ((rate / 100)/12);   
            //Calculate interest by multiplying initial amount by the rate times number of days
            //Monthly rate is divided by number of days in a month
            decimal total = (decimal)rate/30 * amount * numberofDays;
            return total;
        }
        
        public decimal CalculateInterestToDate(IAccount account, DateTimeOffset toDate)
        {
         
            decimal interest = 3.66m;
            return interest;
        }
        public ActionResult Close()      
        {
            try
            {
                DataTable dt = new DataTable();
                
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from account ORDER BY customerName ASC;";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);               
                da.Fill(dt);               
                return View(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }
                 
        }
        public void Closer(string accNo)
        {
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();
                String accountQuery = "delete from account where accountNumber = '" + accNo + "'";
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);
                da.Fill(dt);
                dt.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
