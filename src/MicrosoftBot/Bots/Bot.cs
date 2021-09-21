using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace MicrosoftBot.Bots {

    /**
     * <summary>
     *      La classe <c>Bot</c> eredita la classe <c>UtilityEva</c>.
     *      Contiene l'override dei metodi:
     *      <list type="bullet">
     *          <item>
     *              <term><c>OnMessageActivityAsync</c></term>
     *          </item>
     *          <item>
     *              <term><c>OnEventActivityAsync</c></term>
     *          </item>
     *          <item>
     *              <term><c>OnMembersAddedAsync</c></term>
     *          </item>
     *      </list>
     *      ereditati dalla classe <c>ActivityHandler</c>
     *      che è la classe padre di <c>UtilityEva</c>.
     * </summary>
     * 
     * <author>Marco Scanu</author>
     * <author>Elisa Zilich</author>
     */
    public class Bot : UtilityEva {

        /**
         * <summmary>
         *      Questo metodo è un <c>override</c> del metodo <c>OnMessageActivityAsync</c>.
         *      Viene chiamato ogni volta che viene ricevuto nuovo input dell'utente.
         *      Gestisce la logica del bot per la risposta ai messaggi.
         * </summmary>
         * 
         * <param name="turnContext">
         *      oggetto che fornisce le informazioni necessarie 
         *      per elaborare un'attività in entrata.
         * </param>
         * <param name="cancellationToken">
         *      oggetto che notifica che le operazioni devono essere annullate.
         * </param>
         * 
         * <returns>
         *      un'oggetto <c>Task</c> ovvero un'attività
         *      che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        protected override async Task
            OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
                                   CancellationToken cancellationToken) {
            await EvaFunctions(turnContext, cancellationToken);
        }

        /**
         * <summary>
         *      Questo metodo è un <c>override</c> del metodo <c>OnEventActivityAsync</c>.
         *      Viene chiamato quando si verifica un evento asinrono.
         *      Utilizzato per mandare i messaggi di benvenuto su web chat.
         * </summary>
         * 
         * <param name="turnContext">
         *      oggetto che fornisce le informazioni necessarie 
         *      per elaborare un'attività in entrata.
         * </param>
         * <param name="cancellationToken">
         *      oggetto che notifica che le operazioni devono essere annullate.
         * </param>
         * 
         * <returns>
         *      un'oggetto <c>Task</c> ovvero un'attività
         *      che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        protected override async Task 
            OnEventActivityAsync(ITurnContext<IEventActivity> turnContext,
                                 CancellationToken cancellationToken) {
            /*
             * <c>requestWelcomeMessage<c> è il parametro name passato 
             * tramite la funzione postActivity nel file javascript
             */
            if (turnContext.Activity.Name == "requestWelcomeMessage") {

                /*
                 * viene recuperata la stringa param passata 
                 * dalla funzione postActivity nel file javascript
                 */
                string parStr = turnContext.Activity.Value.ToString();
                string[] par = parStr.Split(" ");
                if (par.Length > 0) {
                    //viene recuperato il parametro isDev
                    if (par[0] == "true")
                        isDev = true;
                    else
                        isDev = false;
                    if (par.Length > 1)
                        //viene recuperato il parametro botId
                        botId = par[1];
                    if (par.Length > 2)
                        //viene recuperato il parametro lang
                        lang = par[2];
                }
                InitializeAPI();
                await WelcomeMassages(turnContext, cancellationToken);
            }           
        }

        /**
         * <summary>
         *      Questo metodo è un <c>override</c> del metodo <c>OnMembersAddedAsync</c>.
         *      Viene chiamato ogni volta che un membro viene aggiunto alla conversazione. 
         *      Serve per stampare i messaggi di benvenuto se si testa il bot con L'emulator
         * </summary>
         * 
         * <param name="membersAdded">
         *      un elenco di tutti i membri aggiunti alla conversazione.
         * </param>
         * <param name="turnContext">
         *      oggetto che fornisce le informazioni necessarie 
         *      per elaborare un'attività in entrata.
         * </param>
         * <param name="cancellationToken">
         *      oggetto che notifica che le operazioni devono essere annullate.
         * </param>
         * 
         * <returns>
         *      un'oggetto <c>Task</c> ovvero un'attività
         *      che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        protected override async Task 
            OnMembersAddedAsync(IList<ChannelAccount> membersAdded,
                                ITurnContext<IConversationUpdateActivity> turnContext,
                                CancellationToken cancellationToken) {
            if (turnContext.Activity.ChannelId != "webchat" 
                    && turnContext.Activity.ChannelId != "directline") 
                foreach (var member in membersAdded)
                    if (member.Id != turnContext.Activity.Recipient.Id)
                        await WelcomeMassages(turnContext, cancellationToken);
        } 
    }
}
