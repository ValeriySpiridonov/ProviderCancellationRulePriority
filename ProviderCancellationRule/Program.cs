using  System.Configuration;

namespace ProviderCancellationRule
{
    class Program
    {
        static void Main( string[] args )
        {
            CancellationRulePriorityUpdater cancellationRulePriorityUpdater = new CancellationRulePriorityUpdater( ConfigurationManager.ConnectionStrings["travelline"].ConnectionString, new ConsoleLogger());
            cancellationRulePriorityUpdater.Execute();
        }
    }
}
