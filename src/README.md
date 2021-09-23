# Descrizione d'uso
Per prima cosa, bisogna eseguire il download della della ***Cartella WebChat*** perché implementa in back-end il codice contenuto nella cartella ***MicrosoftBot***.
Dopodichè, tramite la command line, accedere alla cartella appena scaricata e digitare:
  
    python3 -m http.server

Adesso, nel proprio browser, digitare quanto segue:

    http://localhost:3000/homePage.html

**N.B.** in questo caso è specificata la porta *:3000* però nel vostro caso potrebbe essere differente.

# Overview
## Cartella MicrosoftBot
- Creazione del chatbot attraverso ***Microsoft Framework SDK***
- Chiamata e gestione delle risposte JSON dell'API EVA ***retrieveAnswer***
- Chiamata e gestione delle risposte JSON dell'API API EVA ***evaluateAndCorrect***

## Cartella WebChat
- Integrazione del chatbot creato in C# con ***Microsoft Framework Web Chat***
