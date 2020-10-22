using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WhiteList.IP.Models;

namespace WhiteList.IP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            string clientIPAddress = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
            ViewBag.clientIPAddress = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(clientIPAddress)[0].ToString();
            return View();
        }

        [HttpPost]
        public IActionResult Index(string ip)
        {
            TempData["Message"] = "Your Ip was added succesfully!";
            try
            {
                var ruleName = $"IP-{ip}";
                var azureConnection = _configuration["ConnectionStrings:MasterConnectionString"];
                using (SqlConnection sqlConnection = new SqlConnection(azureConnection))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand =
                        new SqlCommand("sp_set_firewall_rule", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.Add("@name", SqlDbType.NVarChar).Value = ruleName;
                        sqlCommand.Parameters.Add("@start_ip_address", SqlDbType.VarChar).Value = ip;
                        sqlCommand.Parameters.Add("@end_ip_address", SqlDbType.VarChar).Value = ip;
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
