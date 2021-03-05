using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;
using System.Threading.Tasks;

namespace IW4M_Restart.Commands
{
    public class KillManually : Command
    {
        private readonly CommandConfiguration _MyConfig;

        public KillManually(CommandConfiguration config, ITranslationLookup translationLookup) : base(config, translationLookup)
        {
            Name = "killserver";
            Description = "Kills the server";
            Alias = "ks";
            Permission = EFClient.Permission.Owner;

            _MyConfig = config;
        }

        public override Task ExecuteAsync(GameEvent E)
        {
            var server = E.Owner;
            FindProcess fProc = new FindProcess
            { 
                MyServer = server
            };

            E.Origin.Tell("Attempting to close this server");
            server.Logger.WriteDebug("Owner is trying to kill the server");
            fProc.FindAndKillServer();

            return Task.CompletedTask;
        }
    }
}
