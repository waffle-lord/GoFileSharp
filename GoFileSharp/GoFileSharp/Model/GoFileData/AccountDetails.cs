using System;
using System.Collections.Generic;

namespace GoFileSharp.Model.GoFileData
{
    public class AccountDetails
    {
        public string Id { get; set; }
        public string Token { get; set; }

        public string Email { get; set; }

        public AccountType Tier { get; set; }

        public string RootFolder { get; set; }

        public ulong FilesCount { get; set; }
        
        public ulong TotalSize { get; set; }

        public ulong Total30DDLTraffic { get; set; }
        public ulong Credit { get; set; }
        public string Currency { get; set; }
        public string CurrencySign { get; set; }
        
        public StatTotals StatsCurrent { get; set; }
        
        //todo: this is an assumption to be the same as statsCurrent for now - no data yet
        public StatTotals StatsHistory { get; set; }
        
        public Dictionary<string, Dictionary<string, Dictionary<string, Stats>>> Statistics { get; set; }

        public Stats? GetStats(DateTime date)
        {
            try
            {
                var stats = Statistics[date.Year.ToString()][date.Month.ToString()][date.Day.ToString()];

                stats.Date = date;

                return stats;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public ulong FileCountLimit { get; set; }
        public ulong TotalSizeLimit { get; set; }
        public ulong Total30DDLTrafficLimit { get; set; }
    }
}