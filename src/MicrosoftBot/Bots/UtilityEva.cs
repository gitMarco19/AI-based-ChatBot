using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftBot.Bots {

    /**
     * <summary>
     *      La classe astratta <c>UtilityEva</c> eredita la classe <c>ActivityHandler</c>
     *      inoltre implementa tutti i metodi che gestiscono le API di EVA.
     * </summary>
     * 
     * <author>Marco Scanu</author>
     * <author>Elisa Zilich</author>
     */
    public abstract class UtilityEva : ActivityHandler {
        
        ///<summary>Indica l'API KEY di EVA</summary>
        public const string API_KEY = "ef4cfcc3-9d22-48f6-89b3-db97009dda5c";

        /**
         * <summary>
         *      Indica la modalià di utilizzo del bot.
         *      Se <c>true</c> il bot è in modalità sviluppo.
         *      Se <c>false</c> il bot è in modalità produzione.
         * </summary>
         */
        public static bool isDev = false;

        ///<summary>Indica la lingua di utilizzo del bot.</summary>
        public static string lang = "it";

        ///<summary>Indica l'id del bot associato all'account EVA</summary>
        public static string botId = "50f4d02f-fc41-458f-b44c-ea881284551c9";

        ///<summary>Email associata al bot</summary>
        public static string emailOp = "elisa.zilich@hotmail.it";

        //attributi
        private static ITurnContext<IMessageActivity> turnContext;
        private static CancellationToken cancellationToken;
        
        //attributo per richiamare le API
        private static HttpClient client;

        //attributi per evaluate and correct
        private static List<string> faqIdList;
        private static string lastQuery;
        private static bool isToBeCorrected;

        //attributi per recensioni
        private static bool rate;
        private static bool feedback;
        private static int count;

        //attributi per segnalazioni
        private static bool problem;

        /** 
         * <summary>
         *      Metodo che inizializza gli attributi della classe
         * </summary>
         */
        public static void InitializeAPI() {
            faqIdList = new List<string>();
            client = new HttpClient();
            lastQuery = "";
            isToBeCorrected = false;
            rate = false;
            feedback = false;
            count = 0;
            problem = false;
        }

        /**
         * <summary>Metodo che gestisce la logica del chatbot.</summary>
         * 
         * <param name="turnContext">
         *          oggetto che fornisce le informazioni necessarie 
         *          per elaborare un'attività in entrata.</param>
         * <param name="cancellationToken">
         *          oggetto che notifica che le operazioni devono essere annullate.</param>
         * 
         * <returns> 
         *      un'oggetto <c>Task</c> ovvero un'attività
         *      che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        public async Task EvaFunctions(ITurnContext<IMessageActivity> turnContext,
                                            CancellationToken cancellationToken) {
            RefreshToken(turnContext, cancellationToken);
            var text = UtilityEva.turnContext.Activity.Text;
            do {
                if (text == "")
                    text = "stop";
                switch (text) {
                    //caso in cui viene cliccato il tasto "Correggi" (che appare solo in mod. sviluppo)
                    case "Correggi":
                        if (!problem && !rate && 
                                !feedback && !isToBeCorrected) {
                            //se il bot è in mod. sviluppo viene chiesto l'id della risposta corretta
                            if (isDev) 
                                await AskCorrectResponseId();
                            else
                                RetrieveAnswer();
                        } else
                            text = "";
                        break;
                    //caso in cui viene cliccato il tasto "Utile"
                    case "Utile":
                        if (!problem && !feedback && !isToBeCorrected)
                            SelectRate(); 
                        else
                            text = "";
                        break;
                    //caso in cui viene cliccato il tasto "Recensione"
                    case "Recensione":
                        if (!problem && !feedback && !isToBeCorrected)
                            SelectRate();
                        else
                            text = "";
                        break;
                    //caso in cui viene cliccato il tasto "Apri segnalazione"
                    case "Apri segnalazione":
                        if (!problem && !rate && 
                                !feedback && !isToBeCorrected)
                            await AskProblem();
                        else
                            text = "";
                        break;
                        /*
                         * caso in cui l'utente pone una domanda al bot oppure
                         * caso in cui l'utente inserisce l'id della risposta
                         *      corretta (in seguito alla AskCorrectResponseId)
                         * caso in cui si stia lasciando una segnalazione
                         * caso in cui si selezioni il voto della recensione
                         */
                        default:
                            if (isToBeCorrected) {
                                EvaluateAndCorrect();
                                isToBeCorrected = false;
                            } else if (rate) {
                                MakeRate();
                            } else if (problem) {
                                MakeReport();
                            } else {
                                lastQuery = UtilityEva.turnContext.Activity.Text;
                                RetrieveAnswer();
                            }
                            break;
                    }
                } while (text == "");
            }

        /**
         * <summary>Metodo che invia i messaggi di benvenuto all'utente.</summary>
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
        public async Task WelcomeMassages(ITurnContext turnContext,
                                            CancellationToken cancellationToken) {
            string messageOne = "";
            string messageTwo = "";
            if (lang == "it") { 
                messageOne = "Ciao, sono EVA, il tuo virtual assistent!";
                messageTwo = "Chiedimi qualcosa e proverò ad aiutarti!";
            } else if (lang == "en") {
                messageOne = "Hi! I'm your virtual assistant!";
                messageTwo = "Ask me something and I'll try to help you!";
            } else if (lang == "fr") {
                messageOne = "Bonjour! Je suis ton assistant virtuel!";
                messageTwo = "Demandez-moi quelque chose et je vais essayer de vous aider!";
            } else if (lang == "es") {
                messageOne = "Hola!Soy tu asistente virtual!";
                messageTwo = "Pregúntame algo y trataré de ayudarte!";
            } else {
                messageOne = "Supported language is italian";
                messageTwo = "Change lang in the URL";
            }

            //visualizza i puntini di attesa mentre il bot sta scrivendo.
            await turnContext.SendActivitiesAsync(
                    new Activity[] {
                        new Activity { Type = ActivityTypes.Typing },
                        new Activity { Type = "delay", Value = 3000 },
                        MessageFactory.Text(messageOne),
                    }, cancellationToken);
                    await turnContext
                                .SendActivityAsync(MessageFactory
                                        .Text(messageTwo), cancellationToken);
        }

        /**
         * <summary>Metodo che richiama l'API retriveAnswer.</summary>
         */
        private async void RetrieveAnswer() {
            if (lang == "it") {              
                /*
                 * l'API retrieveAnswer quando il messaggio dell'utente 
                 * contiene un apostrofo da errore, perciò lo sostituiamo con uno spazio
                 */
                string messaggio = turnContext.Activity.Text.Replace('\'', ' ');

                //preparazione del request body della query
                var myJson = "{'Query':'" + messaggio + "','" +
                                "apiKey':'" + API_KEY + "','" +
                                "botId':'" + botId + "','" +
                                "showMetadata':'true'" +
                             "}";
                try {
                    //chiamata all'API retrieveAnswer
                    var stringAPI = "https://api.chat2check.com/eva/retrieveAnswer";
                    HttpResponseMessage response = client.PostAsync(stringAPI,
                                                    new StringContent(myJson, 
                                                    Encoding.UTF8, 
                                                     "application/json")).Result;
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();

                    faqIdList.Clear();

                    if (isDev) {
                        //parsing formato json della response body
                        var sent = 0;
                        var parser = JObject.Parse(responseBody);
                        foreach (var risp in parser["answers"]) {
                            var replyText = "Response Id: " + risp["responseId"]
                                            + Environment.NewLine + risp["body"];
                            faqIdList.Add(risp["responseId"].ToString());
                            if (sent == 0) {
                                await AnimatedResponse(RemoveHtmlTag(replyText));
                                sent++;
                            } else {
                                //metodo asincrono che invia il messaggio di risposta all'utente
                                await turnContext
                                        .SendActivityAsync(MessageFactory
                                        .Text(RemoveHtmlTag(replyText)), cancellationToken);
                            }
                        }
                    } else {
                        //parsing formato json della response body per la modalità produzione
                        var parser = JObject.Parse(responseBody);
                        var risp = parser["answers"][0];
                        var replyText = "" + risp["body"];
                        await AnimatedResponse(RemoveHtmlTag(replyText));
                    }
                    //visualizza i pulsanti BotBtn
                    await SendSuggestedActions();
                } catch (Exception e) {
                    if (botId.Length == 0)
                        await AnimatedResponse("Inserire il botId nell'url");
                    else
                        await AnimatedResponse("Mi dispiace non so cosa rispondere 🙁"
                                                + Environment.NewLine + "Aiutami a migliorare 😊");
                    await SendSuggestedActions();
                }
            } else if (lang == "en") {
                var replyText = "I'm sorry, I don't have an answer for you ...";
                await turnContext.SendActivityAsync(MessageFactory
                                    .Text(RemoveHtmlTag(replyText)), cancellationToken);
            } else if (lang == "fr") {
                var replyText = "Je suis désolé, je n'ai pas de réponse pour vous ...";
                await turnContext.SendActivityAsync(MessageFactory
                                    .Text(RemoveHtmlTag(replyText)), cancellationToken);
            } else if (lang == "es") {
                var replyText = "Lo siento, no tengo una respuesta para ti ...";
                await turnContext.SendActivityAsync(MessageFactory
                                    .Text(RemoveHtmlTag(replyText)), cancellationToken);
            } else {
                var replyText = "Error! Change language in the URL";
                await turnContext.SendActivityAsync(MessageFactory
                                    .Text(RemoveHtmlTag(replyText)), cancellationToken);
            }
        }

        /**
         * <summary>Metodo chiamato sul click del tasto "Correggi".
         * Chiede qual è l'id della risposta corretta.</summary>
         * 
         * <returns> un'oggetto <c>Task</c> ovvero un'attività
         *         che rappresenta il lavoro in coda da eseguire.</returns>
         */
        private async Task AskCorrectResponseId() {
            isToBeCorrected = true;
            string message = "Qual'è l'id della risposta corretta?";
            await AnimatedResponse(RemoveHtmlTag(message));
        }

        /**
         * <summary>Metodo che richiama l'API evaluateAndCorrect.</summary>
         */
        private async void EvaluateAndCorrect() {
            var faqId = turnContext.Activity.Text;
            int index = 1;
            bool found = false;
            /*
             * controllo che l'id della risposta corretta sia 
             * uno di quelli forniti dal bot (per il parametro evaluation)
             */
            foreach (var id in faqIdList) {
                if (id == faqId) {
                    found = true;
                    break;
                }
                index++;
            }
            //se l'id della risposta corretta non appartiene a nessuna delle risposte
            if (!found)
                index = 0;
            var myJson = "{'apiKey':'" + API_KEY + "','" +
                            "botId':'" + botId + "','" +
                            "query':'" + lastQuery + "','" +
                            "email':'" + emailOp + "','" +
                            "faqId':'" + faqId + "','" +
                            "evaluation':" + index + 
                         "}";
            try {
                //chiamata all'api evaluateAndCorrect
                var stringAPI = "https://api.chat2check.com/eva/evaluateAndCorrect";
                HttpResponseMessage response = client.PostAsync(stringAPI,
                                                new StringContent(myJson,
                                                Encoding.UTF8, "application/json")).Result;
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                //parsing formato json
                var parser = JObject.Parse(responseBody);
                var replyText = "";

                //traduzione delle risposte di EvaluateAndCorrect in italiano
                switch (parser["result"].ToString()) {

                    case "Evaluation inserted":
                        replyText = "Valutazione inserita! 😊";
                        break;

                    case "Evaluation inserted and KB updated.":
                        replyText = "Valutazione inserita e KB aggiornato. 😊";
                        break;

                    case "Evaluation inserted but cannot update KB, check the faqId param":
                        replyText = "Valutazione inserita ma non è stato possibile aggiornare il KB, " +  
                                "controlla che l'id della risposta specificata sia corretto!";
                        break;

                    case "Error sending evaluation":
                        replyText = "Errore nell'invio della valutazione!";
                        break;
                }
                await AnimatedResponse(RemoveHtmlTag(replyText));
            } catch (Exception e) {
                if (botId.Length == 0)
                    await AnimatedResponse("Inserire il botId nell'url");
                else
                    await AnimatedResponse("Mi dispiace non so cosa rispondere 🙁"
                                            + Environment.NewLine + "Aiutami a migliorare 😊");
                await SendSuggestedActions();
            }
        }

        /**
         * <summary>
         *      Metodo chiamato sul click del pulsante "Utile o Recensione".
         *      Permette di selezionare il voto di una risposta.
         * </summary>
         */
        private async void SelectRate() {
            rate = true;
            var message = "Come valuteresti la nostra conversazione?";
            var reply = MessageFactory.Text("");

            //visualizza le "stelline" per la valutazione
            reply.SuggestedActions = new SuggestedActions() {
                Actions = new List<CardAction>() {
                    new CardAction() {
                        Text = "⭐️",
                        Type = ActionTypes.ImBack, Value = "⭐️"
                    },
                    new CardAction() {
                        Text = "⭐️⭐️",
                        Type = ActionTypes.ImBack, Value = "⭐️⭐️"
                    },
                    new CardAction() { 
                        Text = "⭐️⭐️⭐️", 
                        Type = ActionTypes.ImBack, Value = "⭐️⭐️⭐️"
                    },
                    new CardAction() {
                        Text = "⭐️⭐️⭐️⭐️", 
                        Type = ActionTypes.ImBack, Value = "⭐️⭐️⭐️⭐️"
                    },
                    new CardAction() { 
                        Text = "⭐️⭐️⭐️⭐️⭐️", 
                        Type = ActionTypes.ImBack, Value = "⭐️⭐️⭐️⭐️⭐️"
                    },
                    new CardAction() { 
                        Text = "Non voglio votare", 
                        Type = ActionTypes.ImBack, Value = "Non voglio votare"
                    },
                },
            };
            await AnimatedResponse(message);
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /**
         * <summary>
         *      Metodo chiamato sul click di uno dei pulsanti per la votazione.
         *      Richiama il metodo <c>MakeFeedback</c>.
         *      
         *      Se invece non viene premuto nessun pulsante 
         *      viene richiamato il metodo <c>SelectRate</c>
         * </summary>
         */
        private async void MakeRate() {
            var feed = turnContext.Activity.Text;
            //se è stata selezionata una valutazione 
            if ((feed == "⭐️" || feed == "⭐️⭐️" || feed == "⭐️⭐️⭐️"
                    || feed == "⭐️⭐️⭐️⭐️" || feed == "⭐️⭐️⭐️⭐️⭐️") && !feedback) {
                MakeFeedback(feed);
            }
            //se è stato cliccato il pulsante "Non voglio votare"
            else if (feed == "Non voglio votare" && !feedback) {
                rate = false;
                string message = "Va bene, allora chiedimi qualcosa e proverò ad aiutarti.";
                await AnimatedResponse(RemoveHtmlTag(message));
            }
            /*
             * se non viene cliccato nè il pulsante "si" nè il pulsante "no"
             * in seguito alla domanda "Vuoi lasciarmi anche una recensione?"
             */
            else if (feedback) {
                MakeFeedback(feed);
            }
            //se non è stato cliccato nessun pulsante per la valutazione
            else {
                SelectRate();
            }
        }

        /**
         * <summary>Metodo che permette di lasciare una recensione.</summary>
         * 
         * <param name="feed"> 
         *          oggetto <c>string</c> che indica se si vuole lasciare
         *          o meno una recensione.
         * </param>
         */
        private async void MakeFeedback(string feed) {
            feedback = true;

            //caso che si verifica in seguito all'invio della recensione
            if (count == -1) {
                //qui bisogna richiamare le API per mandare la recensione
                feedback = false;
                rate = false;
                count = 0;
                var message = "Grazie per la valutazione.";
                await AnimatedResponse(RemoveHtmlTag(message));
            } else if (feed == "Si" && count != 0) {
                //caso in cui viene cliccato "Si"
                count = -1;
                var message = "Ok, puoi lasciarmi una recensione " + 
                                "sulla tua esperienza...";
                await AnimatedResponse(RemoveHtmlTag(message));
            } else if (feed == "No" && count != 0) {
                // caso in cui viene cliccato "No"
                feedback = false;
                rate = false;
                var message = "Grazie per la valutazione.";
                await AnimatedResponse(RemoveHtmlTag(message));
            } else {
                // dopo la valutazione viene chiesta una recensione
                count++;
                string message = "Vuoi lasciarmi anche una recensione?";

                var reply = MessageFactory.Text("");
                reply.SuggestedActions = new SuggestedActions() {
                    Actions = new List<CardAction>() {
                        new CardAction() {
                            Text = "Si",
                            Type = ActionTypes.ImBack, Value = "Si"
                        },
                        new CardAction() {
                            Text = "No",
                            Type = ActionTypes.ImBack, Value = "No"
                        },
                    },
                };
                await AnimatedResponse(RemoveHtmlTag(message));
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }

        /**
         * <summary>
         *      Metodo chiamato sul click del pulsante "Apri segnalazione".
         *      Chiede qual è il problema da segnalare.
         * </summary>
         * 
         * <returns>un'oggetto <c>Task</c> ovvero un'attività 
         *          che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        private async Task AskProblem() {
            problem = true;
            var message = "Ok, manderò una email ad un operatore..." 
                    + " Qual è il tuo problema?";
            await AnimatedResponse(RemoveHtmlTag(message));
        }

        /**
         * <summary>
         *      Metodo che viene chiamato in seguito
         *      all'invio di una segnalazione.
         * </summary>
         */
        private async void MakeReport() {
            //qui bisogna richiamare le API per mandare la segnalazione
            problem = false;
            var messageOne = "Ho inviato la mail con il tuo problema, " 
                    + "un operatore ti contatterà appena possibile, grazie!";
            await AnimatedResponse(RemoveHtmlTag(messageOne));
            var messageTwo = "Chiedimi qualcosa e proverò ad aiutarti!";
            await turnContext
                    .SendActivityAsync(MessageFactory
                        .Text(messageTwo), cancellationToken);
        }

        /**
         * <summary>
         *      Metodo che mostra i BotBtn 
         *      dopo ogni risposta del bot.
         * </summary>
         * 
         * <returns>
         *      un'oggetto <c>Task</c> ovvero un'attività
         *      che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        private async Task SendSuggestedActions() {
            var reply = MessageFactory.Text("");

            //caso "modalità sviluppo"
            if (isDev) {
                reply.SuggestedActions = new SuggestedActions() {
                    Actions = new List<CardAction>() {
                        new CardAction() {
                            Text = "Correggi",
                            Type = ActionTypes.ImBack,
                            Value = "Correggi"
                        },
                        new CardAction() {
                            Text = "Utile",
                            Type = ActionTypes.ImBack,
                            Value = "Utile"
                        },
                        new CardAction() {
                            Text = "Recensione",
                            Type = ActionTypes.ImBack,
                            Value = "Recensione"
                        },
                        new CardAction() {
                            Text = "Segnalazione",
                            Type = ActionTypes.ImBack,
                            Value = "Apri segnalazione"
                        },
                    },
                };
            //caso "modalità produzione"
            } else {
                reply.SuggestedActions = new SuggestedActions() {
                    Actions = new List<CardAction>() {
                        new CardAction() {
                            Text = "Utile",
                            Type = ActionTypes.ImBack,
                            Value = "Utile"
                        },
                        new CardAction() {
                            Text = "Recensione",
                            Type = ActionTypes.ImBack,
                            Value = "Recensione"
                        },
                        new CardAction() {
                            Text = "Segnalazione",
                            Type = ActionTypes.ImBack,
                            Value = "Apri segnalazione"
                        },
                    },
                };
            }
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /**
         * <summary>
         *      Metodo che fa comparire l'azione di "dot typing",
         *      ovvero i puntini di attesa mentre il bot sta scrivendo.
         * </summary>
         * 
         * <param name="messaggio">
         *      oggetto <c>string</c> ovvero il messaggio 
         *      da mostrare dopo i dot.
         * </param>  
         * 
         * <returns>
         *      un'oggetto <c>Task</c> ovvero un'attività
         *      che rappresenta il lavoro in coda da eseguire.
         * </returns>
         */
        private async Task AnimatedResponse(string messaggio) {
            await turnContext
                    .SendActivitiesAsync(
                            new Activity[] {
                            new Activity { Type = ActivityTypes.Typing },
                            new Activity { Type = "delay", Value = 3000 },
                            MessageFactory.Text(messaggio),
                    },
                    cancellationToken);
        }

        /**
         * <summary>
         *      Metodo che aggiorna gli oggetti <c>turnContext</c> 
         *      e <c>cancellationToken</c>
         * </summary>
         *  
         * <param name="turnContext">
         *          oggetto che fornisce le informazioni necessarie 
         *          per elaborare un'attività in entrata.</param>
         * <param name="cancellationToken">
         *          oggetto che notifica che le operazioni 
         *          devono essere annullate.
         * </param>
         */
        private void RefreshToken(ITurnContext<IMessageActivity> turnContext,
                                            CancellationToken cancellationToken) {
            UtilityEva.turnContext = turnContext;
            UtilityEva.cancellationToken = cancellationToken;
        }

        /**
         * <summary>
         *      Metodo che rimuove i tag html dal messaggio in input.
         * </summary>
         * 
         * <param name="messaggio">
         *      oggetto <c>string</c> dalla quale rimuovere i tag
         * </param> 
         * 
         * <returns>
         *      un oggetto <c>string</c> ovvero il messaggio 
         *      privo di tag html.
         * </returns>
         */
        private string RemoveHtmlTag(string messaggio) {
            Regex espressioneRegolare = new Regex("<[^>]*>");
            return espressioneRegolare.Replace(messaggio, "");
        } 
    }
}