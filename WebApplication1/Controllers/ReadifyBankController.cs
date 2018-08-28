using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Web.Mvc;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers
{
    public class ReadifyBankController : Controller//, Interfaces.IReadifyBank
    {
        
        private string source = "Data Source = /Applications/Rider 2018.1.4.app/Contents/bin/identifier.sqlite";


        //get all of the accounts from the database
        public ActionResult AccountList()
        {
            ViewBag.Message = "List Accounts.";                              
            List<string> accounts = new List<string>();
            try
            {
                DataTable dt = new DataTable();
                SQLiteConnection conn = new SQLiteConnection(source);
                conn.Open();                
                String accountQuery = "select * from customer";               
                SQLiteDataAdapter da = new SQLiteDataAdapter(accountQuery, conn);
                da.Fill(dt);
                foreach (DataRow dataRow in dt.Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        if (dataRow.GetType() != typeof(DateTime))
                        {
                            accounts.Add(item.ToString());
                        }
                        
                    }
                }
                ViewBag.Accounts = accounts;
                return View();
            }
            catch (Exception e)
            {
                Console.WriteLine("bad things happened :(");
                throw;
            }
                 
        }


       
    }
}