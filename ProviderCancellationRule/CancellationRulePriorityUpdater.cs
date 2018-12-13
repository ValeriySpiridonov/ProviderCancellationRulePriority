using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using ProviderCancellationRule.Entities;

namespace ProviderCancellationRule
{
    class CancellationRulePriorityUpdater
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly List<int> _specifiedProviders;

        public CancellationRulePriorityUpdater(string connectionString, ILogger logger, List<int> specifiedProviders)
        {
            _connectionString = connectionString;
            _logger = logger;
            _specifiedProviders = specifiedProviders;
        }

        public void Execute()
        {
            List<Provider> providers = GetAllProviders();
            if ( _specifiedProviders != null )
            {
                providers = providers.FindAll( provider => _specifiedProviders.Contains( provider.Id ) );
            }

            foreach (var provider in providers)
            {
                try
                {
                    ProviderCancellationRulePriorityUpdater providerCancellationRulePriorityUpdater =
                        new ProviderCancellationRulePriorityUpdater( provider, _connectionString, _logger );
                    providerCancellationRulePriorityUpdater.Execute();
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);
                }
            }
        }

        List<Provider> GetAllProviders()
        {
            List<Provider> result = new List<Provider>();
            string queryString = @"select p.*, c.tzid  from [provider] p 
                inner join city c on p.id_city = c.id_city 
                where not exists(select * from special_offer so where so.id_provider = p.id_provider and is_default_rate_mix = 1  )
                order by p.id_provider";
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                SqlCommand command = new SqlCommand( queryString, connection );
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while ( reader.Read() )
                    {
                        result.Add( new Provider( Convert.ToString( reader[ "tzid" ] ) )
                        {
                            Id = Convert.ToInt32( reader[ "id_provider" ] ),
                            Name = Convert.ToString( reader[ "name" ] ),
                            ArrivalTime = ( reader[ "arrival_time" ] is DBNull )
                                ? Time.Null
                                : ( Time )Convert.ToInt32( reader[ "arrival_time" ] ),
                            DepartureTime = ( reader[ "departure_time" ] is DBNull )
                            ? Time.Null
                            : ( Time )Convert.ToInt32( reader[ "departure_time" ] ),
                        });
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return result;
        }
    }
}
