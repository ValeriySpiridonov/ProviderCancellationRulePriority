namespace ProviderCancellationRule
{
    class Program
    {
//        private const string _connectionString = "Data Source=DEVSQL;Initial Catalog=travelline_spiridonov;Network Library=dbmssocn;Persist Security Info=True;Integrated Security=SSPI";
        private const string _connectionString = "Data Source=DEVHASQL;Initial Catalog=travelline_qa1;Network Library=dbmssocn;Persist Security Info=True;Integrated Security=SSPI";
        // private const string _connectionString = "Data Source = tlsql.travelline.wan; Initial Catalog = travelline; Network Library = dbmssocn; Pooling=true;Integrated Security = SSPI; multisubnetfailover=true";

        static void Main( string[] args )
        {
            CancellationRulePriorityUpdater cancellationRulePriorityUpdater = new CancellationRulePriorityUpdater(_connectionString, new ConsoleLogger());
            cancellationRulePriorityUpdater.Execute();
        }
    }
}
