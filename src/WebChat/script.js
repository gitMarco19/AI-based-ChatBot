var firstStart = true;

var user = {
	id: "user-id",
	name: "user name"
};

var bot = {
	id: "bot-id",
	name: "bot-name"
};

// serve per stabilire una connessione con webchat
var botChat = window.WebChat.createDirectLine({
	token: "85J69tHnrGA.CugjSkkbNRmuUo8OkkI-BKGmmJiY2Eg2WGGq-3Vbw-U",
	user: user
	
});

// inizializzazione di default dei parametri della querystring
var defVideo = 'true';
var defSpeech = 'false';
var defDocuments = 'true';
var defBotId = '50f4d02f-fc41-458f-b44c-ea881284551c';
var defLang = 'it';
var defUserBaloonColor = '48BB92';
var defBotBaloonColor = 'FFE3CA';
var defTitle = 'https://eva.app2check.com/app/img/logoEvaSmall.png';
var defSendBtnColor = '48BB92';
var defServerUserImage = 'https://cdn1.iconfinder.com/data/icons/unique-round-blue/93/user-512.png';
var defBotTextColor = '808080';
var defBotBtnColor = 'FFF';
var defBotBtnTextColor = '808080';
var defHost = 'new-converse.ces.pitneycloud.com';
var defTenantId = 'sab5829';
var defIsDev = 'false';
var defSupportEmail = 'app2check@finsa.it';
var defUrl = "";

var defUserTextColor = 'BLACK';
var defSendTextColor = 'BLUE';
var defSendBackgrColor = 'WHITE';


// parametri di customizzazione che compaiono nell'url
var video = defVideo;
var speech = defSpeech;
var documents = defDocuments;
var botId = defBotId;
var lang = defLang;
var userBaloonColor = defUserBaloonColor;
var botBaloonColor = defBotBaloonColor;
var title = defTitle;
var sendBtnColor = defSendBtnColor;
var serverUserImage = defServerUserImage;
var botTextColor = defBotTextColor;
var botBtnColor = defBotBtnColor;
var botBtnTextColor = defBotBtnTextColor;
var host = defHost;
var tenantId = defTenantId;
var isDev = defIsDev;
var supportEmail = defSupportEmail;
var url = defUrl;

//altri parametri customizzabili da webchat
//non vengono messi di default nell'url (perchè non sono utilizzati nei bot Eva) ma se aggiunti modificano la grafica

//colore del testo nei messaggi dell'utente
var userTextColor = defUserTextColor;

//colore del testo nel box di scrittura del messaggio
var sendTextColor = defSendTextColor;

//colore dello sfondo del box di scrittura del messaggio
var sendBackgrColor = defSendBackgrColor;

//recupera la querystring (tutto ciò che viene dopo il "?")	
var params = location.search;

//se la querystring è vuota viene creata
if (params == "")
	createQueryString();
// altrimenti vengono aggiornati i suoi valori
else
	updateQueryString();

if (url == "")
	document.getElementById("#test").src = "";
else
	document.getElementById("#test").src = url;

var botImage;
if (video == "true")
	if (speech == "true")
		//gif con Eva che parla
		botImage = 'immagini/eva.gif';
	else
		//gif con Eva che sbatte le ciglia
		botImage = 'immagini/eva-mute.gif';
else
	botImage =  serverUserImage;

document.getElementById('logo').src = title;

// permette a webchat di prendere in input la voce dell'utente e convertirla in testo
// sono necessari posizione e chiave dell'oggetto di tipo  creato su Azure
const speechToTextPonyfillFactory = createCognitiveServicesSpeechServicesPonyfillFactory({
   credentials: {
     region: 'westeurope',
     subscriptionKey: 'insert-your-key'
   }
});

// permette al bot, tramite webchat, di leggere la risposta
// sono necessari posizione e chiave dell'oggetto di tipo Voce creato su Azure
const textToSpeechPonyfillFactory = createCognitiveServicesSpeechServicesPonyfillFactory({
   credentials: {
     region: 'westeurope',
     subscriptionKey: 'insert-your-key'
   }
});

// oggetto che comprende i parametri customizzabili da webchat
var styleOptions = {
	avatarBorderRadius: '0%',
	avatarSize: 100,
	botAvatarImage: botImage,
	bubbleBorderRadius: 10,
	bubbleBackground: toHexColor(botBaloonColor),
	bubbleFromUserBorderRadius: 10,
	bubbleFromUserBackground: toHexColor(userBaloonColor),
	bubbleTextColor: toHexColor(botTextColor),
	bubbleFromUserTextColor: toHexColor(userTextColor),
	sendBoxBackground: toHexColor(sendBackgrColor),
	sendBoxTextColor: toHexColor(sendTextColor),
	sendBoxButtonColor: toHexColor(sendBtnColor),
	suggestedActionBackground: toHexColor(botBtnColor),
	suggestedActionTextColor: toHexColor(botBtnTextColor),
	suggestedActionBorderColor: toHexColor(botBtnTextColor),
	suggestedActionBorderRadius: 10,
	hideUploadButton: true,
};

// la renderWebChat serve per passare a webchat tutte le opzioni di customizzazione (di grafica e voce)
window
	.WebChat
	.renderWebChat({
			directLine: botChat,
			webSpeechPonyfillFactory: options => {
				const { SpeechGrammarList, SpeechRecognition } = speechToTextPonyfillFactory(options);
				const { speechSynthesis, SpeechSynthesisUtterance } = textToSpeechPonyfillFactory(options);
				
				if (speech == "true") {
					// viene mostrato il microfono per inviare il messaggio al bot
					return {
						SpeechGrammarList,
						SpeechRecognition,
						speechSynthesis,
						SpeechSynthesisUtterance
					};
				} else {
					return {
						SpeechGrammarList: null,
						SpeechRecognition: null,
						speechSynthesis: null,
						SpeechSynthesisUtterance: null
					};
				}
			},
			user: user,
			bot: bot,
			locale: 'it-IT',
			language: 'it-IT',
			selectVoice: ( voices, activity ) => 
						voices.find( ( { name } ) => /ElsaNeural/iu.test( name ) ),
			styleOptions
	}, document.getElementById("webchat"));

var x = document
			.getElementById("webchat")
			.getElementsByClassName("webchat__defaultAvatar");

var y = document
			.getElementById("webchat")
			.getElementsByClassName("webchat__initialsAvatar");

// cancella l'avatar da tutti i messaggi precedenti in modo che venga visualizzato solo per l'ultimo messaggio
// fatto tramite javascript e css perchè questa funzionalità non è customizzabile attraverso webchat
setInterval(function() {
	for (var j = 0; j < y.length; j++) {
		y[j].remove();
	}

	for (var i = 0; i < x.length - 1; i++) {
		if (x[i] != null)
			x[i].remove();
	}

	if (x[i] != null) {
		x[i].style.height = "150px";
		x[i].style.visibility = "visible";
	}
}, 50);

// apre la chat del bot, viene chiamata quando al click dell'icona di Eva
function openChat() {
	document.getElementById("chatBox").style.display = "block";
	document.getElementById("imgEVA").style.display = "none";

	//parametri da passare al progetto in C#
	var param = isDev + ' ' + botId + ' ' + lang;
	
	if (firstStart) {
		// attraverso la funzione postActivity passiamo il campo name e value al progetto in C#
		botChat
			.postActivity({
					from: user,
					name: "requestWelcomeMessage",
					type: "event",
					value: param
			}).subscribe(() => console.log("Evento inviato!"));

		firstStart = false;
	}
}

//funzione che chiude la chat sul click della X in alto a destra
function closeChat() {
	document.getElementById("imgEVA").style.display = "block";
	document.getElementById("chatBox").style.display = "none";
}

//funzione che crea la querystring con tutti i parametri di default al primo avvio
function createQueryString() {
	var queryParams = new URLSearchParams(params);
		
	queryParams.set("video", video);
	queryParams.set("speech", speech);
	queryParams.set("documents", documents);
	queryParams.set("botId", botId);
	queryParams.set("lang", lang);
	queryParams.set("userBaloonColor", userBaloonColor);
	queryParams.set("botBaloonColor", botBaloonColor);
	queryParams.set("title", title);
	queryParams.set("sendBtnColor", sendBtnColor);
	queryParams.set("serverUserImage", serverUserImage);	
	queryParams.set("botTextColor", botTextColor);
	queryParams.set("botBtnColor", botBtnColor);
	queryParams.set("botBtnTextColor", botBtnTextColor);
	queryParams.set("host", host);
	queryParams.set("tenantId", tenantId);
	queryParams.set("isDev", isDev);
	queryParams.set("supportEmail", supportEmail);
	queryParams.set("url", url);
	
	//queryParams.set("sendTextColor", sendTextColor);
	//queryParams.set("sendBackgrColor", sendBackgrColor);
	//queryParams.set("userTextColor", userTextColor);

	history.replaceState(null, null, "?" + queryParams.toString());
}

//funzione che aggiorna il valore dei parametri nella querystring
function updateQueryString() {
	var queryParams = new URLSearchParams(params);

	video = queryParams.get("video");
	if (video == null)
		video = defVideo;
	speech = queryParams.get("speech");
	if (speech == null)
		speech = defSpeech;
	documents = queryParams.get("documents");
	if (documents == null)
		documents = defDocuments;
	botId = queryParams.get("botId");
	if (botId == null)
		botId = defBotId;
	lang = queryParams.get("lang");
	if (lang == null)
		lang = defLang;
	userBaloonColor = queryParams.get("userBaloonColor");
	if (userBaloonColor == null)
		userBaloonColor = defUserBaloonColor;
	botBaloonColor = queryParams.get("botBaloonColor");
	if (botBaloonColor == null)
		botBaloonColor = defBotBaloonColor;
	title = queryParams.get("title");
	if (title == null)
		title = defTitle;
	sendBtnColor = queryParams.get("sendBtnColor");
	if (sendBtnColor == null)
		sendBtnColor = defSendBtnColor;
	serverUserImage = queryParams.get("serverUserImage");	
	if (serverUserImage == null)
		serverUserImage = defServerUserImage;
	botTextColor = queryParams.get("botTextColor");
	if (botTextColor == null)
		botTextColor = defBotTextColor;
	botBtnColor = queryParams.get("botBtnColor");
	if (botBtnColor == null)
		botBtnColor = defBotBtnColor;
	botBtnTextColor = queryParams.get("botBtnTextColor");
	if (botBtnTextColor == null)
		botBtnTextColor = defBotBtnTextColor;
	host = queryParams.get("host");
	if (host == null)
		host = defHost;
	tenantId = queryParams.get("tenantId");
	if (tenantId == null)
		tenantId = defTenantId;
	isDev = queryParams.get("isDev");
	if (isDev == null)
		isDev = defIsDev;
	supportEmail = queryParams.get("supportEmail");
	if (supportEmail == null)
		supportEmail = defSupportEmail;
	
	sendTextColor = queryParams.get("sendTextColor");
	if (sendTextColor == null)
		sendTextColor = defSendTextColor;
	sendBackgrColor = queryParams.get("sendBackgrColor");
	if (sendBackgrColor == null)
		sendBackgrColor = defSendBackgrColor
	userTextColor = queryParams.get("userTextColor");
	if (userTextColor == null)
		userTextColor = defUserTextColor;

	url = queryParams.get("url");
	if (url == null)
		url = "";
}

function toHexColor(str) {
	//se il colore passato nell'url è esadecimale aggiunge #
	if (/^[A-F0-9]+$/i.test(str))
		return '#' + str;
	//se il colore passato nell'url è un rgb setta il colore di default
	//commentare l'if se si desidera accettare anche colori rgb
	if(str.startsWith("rgb"))
		return '#' + defBotBaloonColor;
	else
		return str ;
}