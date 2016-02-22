using System;
using System.Configuration;
using Newtonsoft.Json;
using Onspring.ADSync.Models;

namespace Onspring.ADSync
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine(@"Onspring.AdSync.exe [server] [DC=domain,DC=com/BaseDN] [Required AD Group]");
                return;
            }

            var ad = new ActiveDirectory(args[0], args[1]);
            var results = ad.GetUsersByGroupMembership(args[2]);
            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));
            if (results != null && results.Count > 0)
            {
                Console.WriteLine("\n\n************* Onspring ***************\n");
                var apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
                var apiKey = ConfigurationManager.AppSettings["ApiKey"];
                var onspringApi = new OnspringHelper(apiBaseUrl, apiKey);
                foreach (var adUser in results)
                {
                    
                    var onUser = onspringApi.GetUserByUsername(adUser.SamAccountName);
                    if (onUser != null)
                    {
                        Console.WriteLine("Found {0} ({1} {2}) in Onspring.", onUser.Username, onUser.FirstName, onUser.LastName);
                    }
                    else
                    {
                        Console.WriteLine("Didn't find {0}", adUser.SamAccountName);
                        Console.Write("Do you want to create a user account in Onspring for {0}? [y/n]: ", adUser.SamAccountName);
                        if (string.Equals("y", Console.ReadLine(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            onUser = new OnspringUser
                            {
                                Username = adUser.SamAccountName,
                                FirstName = adUser.GivenName,
                                LastName = adUser.SurName,
                                Email = adUser.Mail
                            };
                            var recordId = onspringApi.AddNewUser(onUser);
                            if (recordId.HasValue)
                            {
                                Console.WriteLine("Added user {0} ({1} {2}) with id {3} in Onspring.", onUser.Username, onUser.FirstName, onUser.LastName, recordId);
                            }
                            else
                            {
                                Console.WriteLine("Failed to create user {0}.", adUser.SamAccountName);
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }
}
