
namespace PXE_Server
{


    class Program
    {


        static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            // NamespaceUpdater.UpdateProjectNamespace();
            PXEConfig? config = PXEConfig.Load();
            config?.Save();

            if(config == null)
            {
                await System.Console.Out.WriteLineAsync("Failed to load configuration");
                return 1;
            }

            PXEServer pxe_server = new PXEServer(config);
            pxe_server.Start();

            await System.Console.Out.WriteLineAsync("Press ENTER to exit");
            await System.Console.In.ReadLineAsync();

            pxe_server.Stop();

            return 0;
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
