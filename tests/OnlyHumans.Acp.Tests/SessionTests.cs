using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlyHumans.Acp.Tests
{
    public class SessionTests : TestsRuntime
    {
        static SessionTests()
        {
            client = new Client(agentCmdPath, agentCmdArgs, agentCmdWd)
                .WithConnectionTracing(SourceLevels.Verbose, new ConsoleTraceListener()); ;
            client.InitializeAsync().Succeeded().Wait();
        }

        
        [Fact]
        public async Task CanSetModel()
        {
            var sess = await client.NewSessionAsync(agentCmdWd).Succeeded();
            var ar = await sess.PromptAsync("Hello");
            Assert.True(ar.IsSuccess);  
        }
        
        static Client client;
    }
}
