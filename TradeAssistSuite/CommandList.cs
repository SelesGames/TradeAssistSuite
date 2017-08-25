using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TradeAssistSuite.User;
using TradeGuard.Logic;

namespace TradeAssistSuite
{
    class CommandList
    {
        /*

        create user                     // generate an id, save to database, output userid to window
        set user [userid]                // set environment user
        get user
        add bittrex [apikey] [secret]   

        check th   // check trade history

        check tg // check tradeguard
        tg [currencypair] [stop price]   // tg BTC-ETH 0.056  
        tg [currencypair] [stop percent] // tg BTC-ETH 50%

        */

        List<string> regexCommands = new List<string>();
        List<Action<dynamic>> commandAction = new List<Action<dynamic>>();
        List<(string, Action<dynamic>)> commandAndActions = new List<(string, Action<dynamic>)>();


        public CommandList()
        {
            commandAndActions = new List<(string, Action<dynamic>)>
            {
                ("create user", CreateUser),
                ("set user (?'userid'[a-zA-Z0-9]*)", o => SetEnvironmentUser(o.userid)),
                ("get user", o => GetEnvironmentUser()),
                ("add bittrex (?'apikey'[a-zA-Z0-9]*) (?'secret'[a-zA-Z0-9]*)", o => AddBittrex(o.apikey, o.secret)),
                ("check tg", o => GetBalances())
            };
        }

        public void ProcessCommand(string command)
        {
            foreach (var ca in commandAndActions)
            {
                var regex = new Regex(ca.Item1);
                var match = regex.Match(command);
                if (match.Success)
                {
                    var input = new ExpandoObject();

                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        var group = match.Groups[i];
                        var captureGroupName = group.Name;
                        var capturedValue = group.Value;//.Captures[0].Value;

                        addProperty(input, captureGroupName, capturedValue);
                    }

                    var action = ca.Item2;
                    action(input);
                    return;
                }
            }

            Console.WriteLine($"Error: unrecognized command: {command}");

            void addProperty(ExpandoObject expando, string propertyName, object propertyValue)
            {
                // ExpandoObject supports IDictionary so we can extend it like this
                var expandoDict = expando as IDictionary<string, object>;
                if (expandoDict.ContainsKey(propertyName))
                    expandoDict[propertyName] = propertyValue;
                else
                    expandoDict.Add(propertyName, propertyValue);
            }
        }

        void CreateUser(dynamic _)
        {
            var repo = new Repository();
            var createTask = repo.CreateUser();
            createTask.Wait();
            var userId = createTask.Result;

            Console.WriteLine($"User created with ID: {userId}");
        }
        
        void SetEnvironmentUser(string userId)
        {
            try
            {
                var repo = new Repository();
                var getTask = repo.GetUser(userId);
                getTask.Wait();
                var user = getTask.Result;
                Environment.Current.UserId = user.id;
                Environment.Current.BittrexApiKey = user.Bittrex.ApiKey;
                Environment.Current.BittrexSecret = user.Bittrex.Secret;
            }
            catch
            {
                Console.WriteLine("No such userId exists.  Try creating one first");
            }
        }

        void GetEnvironmentUser()
        {
            Console.WriteLine($"{Environment.Current.UserId}");
        }

        void AddBittrex(string apiKey, string secret)
        {
            var repo = new Repository();
            var createTask = repo.AddBittrex(apiKey, secret);
            createTask.Wait();
            var result = createTask.Result;

            if (result)
                Console.WriteLine($"bittrex credentials added to environment user");
            else
                Console.WriteLine("Failed to add bittrex credentials");
        }

        void GetBalances()
        {
            var getTask = Functions.GetBalances(Environment.Current.UserId, Environment.Current.BittrexApiKey, Environment.Current.BittrexSecret);
            getTask.Wait();
            var summary = getTask.Result.summary;

            Console.WriteLine(summary.Id);
            foreach (var holding in summary.Holdings)
            {
                Console.WriteLine($"  {holding.CurrencyPair}: {holding.TotalUnits}, {holding.TotalProtected} protected ({holding.TotalPercentProtected})");
            }
        }
    }
}
