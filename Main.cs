using Microsoft.Extensions.Logging;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System.Threading.Tasks;

namespace IW4M_Restart
{
    public class Main : IPlugin
    {
        private ILogger<Main> _logger;

        public string Name => "Auto Restart";

        public float Version => 1.0f;

        public string Author => "Diavolo#6969";

        public Main(ILogger<Main> logger)
        {
            _logger = logger;
        }

        public Task OnEventAsync(GameEvent E, Server S)
        {
            if (E.Type == GameEvent.EventType.ConnectionLost)
            {
                FindProcess fProc = new FindProcess
                {
                    MyServer = S
                };

                _logger.LogInformation("Server is lost, attempting to kill it via port");
                fProc.FindAndKillServer();
            }

            return Task.CompletedTask;
        }

        public Task OnLoadAsync(IManager manager) => Task.CompletedTask;

        public Task OnTickAsync(Server S) => Task.CompletedTask;

        public Task OnUnloadAsync() => Task.CompletedTask;
    }
}