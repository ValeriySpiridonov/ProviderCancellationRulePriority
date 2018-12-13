using System;
using System.Collections.Generic;
using  System.Configuration;
using System.Linq;

namespace ProviderCancellationRule
{
    class Program
    {
        static void Main( string[] args )
        {
            string specificProvidersSetting = ConfigurationManager.AppSettings["SpecificProviders"];
            string connectionString = ConfigurationManager.ConnectionStrings[ "travelline" ].ConnectionString;

            List<int> specifiedProviderIds = null; 
            int specifiedProviderId;
            if ( args.Length > 0 && Int32.TryParse( args[0], out specifiedProviderId ) )
            {
                specifiedProviderIds = new List<int> { specifiedProviderId };
            }
            else if ( !String.IsNullOrWhiteSpace( specificProvidersSetting ) )
            {
                specifiedProviderIds = specificProvidersSetting.Split( ',' ).Select( Int32.Parse ).ToList();
            }

            CancellationRulePriorityUpdater cancellationRulePriorityUpdater = new CancellationRulePriorityUpdater(
                connectionString,
                new ConsoleLogger(),
                specifiedProviderIds );
            cancellationRulePriorityUpdater.Execute();
        }
    }
}
