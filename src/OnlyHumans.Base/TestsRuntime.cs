namespace OnlyHumans;

using Microsoft.Extensions.Configuration;   
public class TestsRuntime : Runtime
{
    static TestsRuntime()
    {
        
            Initialize("OnlyHumans", "Tests", true);
            // Build configuration
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testappsettings.json", optional: false, reloadOnChange: true)
                .Build();

            user = config["Email:User"] ?? throw new ArgumentNullException("Email:User");
            password = config["Email:Password"] ?? throw new ArgumentNullException("Email:Password"); ;
            displayName = config["Email:DisplayName"] ?? throw new ArgumentNullException("Email:DisplayName");
            me = config["Email:ManagerEmail"] ?? throw new ArgumentNullException("Email:DisplayName");
        
    }

    static protected IConfigurationRoot config;
    static string user, password, displayName, me;
}

