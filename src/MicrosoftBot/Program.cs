using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MicrosoftBot.Bots;

namespace MicrosoftBot {

    /**
     * <summary>
     *      La classe <c>Program</c> è la classe principale
     *      che contiene il metodo <c>Main</c>.
     * </summary>
     * 
     * <author>Marco Scanu</author>
     * <author>Elisa Zilich</author>
     */
    public class Program {

        /**
         * <summary>
         *      Metodo <c>Main</c> dal quale parte l'esecuzione.
         *      Chiama i metodi <c>Build</c> ed <c>Run</c> sull'oggetto <c>IWebHostBuilder</c>.
         * </summary>
         * 
         * <param name="args">
         *      <c>string</c> array che indica i parametri
         *      passati da linea di comando
         * </param>
         */
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        /**
         * <summary>
         *      Metodo che chiama il metodo <c>InitializeAPI</c>
         *      e che crea e configura un oggetto <c>WebHostBuilder</c>
         * </summary>
         * 
         * <param name="args">
         *      <c>string</c> array che indica i parametri
         *      passati da linea di comando.
         * </param>
         * 
         * <returns>
         *      un'istanza della classe <c>WebHostBuilder</c>.
         * </returns>
         */
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            UtilityEva.InitializeAPI();
            return WebHost
                    .CreateDefaultBuilder(args)
                    .UseStartup<Startup>();
        }
    }
}
