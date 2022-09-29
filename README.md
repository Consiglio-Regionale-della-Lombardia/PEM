# Portale digitalizzazione atti
# Portale PEM - Presentazione Emendamenti
  
Gli emendamenti sono proposte di modifica riferite ad uno specifico punto di un progetto di legge (titolo, articolo, comma, allegato, ecc.) prima che questo venga votato dall’assemblea legislativa regionale. I subemendamenti sono invece proposte di modifica riferite ad un emendamento precedentemente presentato. Il Portale PEM aiuta a de-materializzare e informatizzare le procedure del Consiglio Regionale per la presentazione di emendamenti/subemendamenti (EM/SUBEM).


### Note sul copyright
- Copyright: Consiglio regionale della Lombardia
- Stato del progetto: Beta
- Mantenimento in carico 2C Solution Srl a socio unico - https://2csolution.it/
- Per segnalare CVE e problemi di sicurezza scrivere a opensource@2Csolution.it
 
# Contenuti

- [Introduzione](#introduzione)
- [Struttura del repository](#struttura-del-repository)
- [Struttura del progetto](#struttura-del-progetto)
- [Installazione](#installazione)
  - [Note sulla release](#note-sulla-release)
  - [Requisiti del sistema](#requisiti-del-sistema)
  - [Procedura di installazione](#procedura-di-installazione)
- [Licenza](#licenza)
  - [Autore / Copyright](#autore---copyright)
  - [Licenze dei componenti di terze parti](#licenze-dei-componenti-di-terze-parti)
  - [Dettagli della licenza](#dettagli-della-licenza)

# Introduzione

Questo repository contiene il codice sorgente e la documentazione del portale PEM - Presentazione Emendamenti. 
L'obiettivo del Portale PEM è di rendere più efficiente e funzionale l’intera procedura di gestione degli emendamenti ai progetti di legge, con un’applicazione multiutente con livelli di informatizzazione e automazione più o meno ampi, a seconda delle varie fasi del processo.
 
Il Portale PEM permette di:
- gestire da remoto la predisposizione dei testi degli EM/SUBEM, la firma e l’operazione di deposito;
- garantire autenticità del testo, intesa come integrità del testo, provenienza del documento dal suo autore e certezza temporale sulla sua presentazione;
- snellire le operazioni per la lavorazione degli EM/SUBEM (numerazione, marcatura, autenticazione, ecc…);
- ridurre i tempi necessari alla generazione di output finalizzati alla discussione in aula e alle fasi post-seduta, attraverso la loro generazione automatica;
- agevolare i lavori d'aula: visualizzare, gestire e modificare in Aula il testo degli emendamenti;
- ridurre l’utilizzo della carta e dei supporti per la stampa.

Maggiori dettagli sulle funzionalità possono essere lette nella documentazione per l’utente finale:

- [Documentazione](/Documentazione/README.md)

# Struttura del repository

  All'interno della release, oltre al presente file README.md e al file di licenza LICENSE.md, sono presenti le seguenti cartelle:
  
  - Database: struttura di configurazione del database e sua documentazione
  
  - Documentazione: documentazione varia sull'installazione e sull'utilizzo del Portale PEM
  
  - Sorgenti API: sorgenti dei moduli API utilizzati dal Portale PEM
  
  - Sorgenti Client: sorgenti della parte client del Portale PEM
  
  - Sorgenti modulo di stampa asincrona: Sorgenti del modulo di stampa asincrona degli Emendamenti in formato pdf
  
# Struttura del progetto

#### INTRODUZIONE

La versione pubblicata del software PEM è l’evoluzione di una prima versione di PEM, sviluppata con tecnologia Microsoft ASP.net su Framework .NET 4.5.
La nuova versione è stata realizzata con l’obiettivo di migliorare e superare alcuni limiti del vecchio portale offrendo i seguenti vantaggi:
-	Eliminazione di librerie di terze parti coperte da licenza non opensource;
-	Separazione della parte client da quella server realizzando API dedicate che gestiscono tutta la logica applicativa di PEM e facilitano l'eventuale sviluppo di applicazioni mobile per dispositivi Apple e Android;
-	Aumento della modularità per consentire l’evoluzione del portale per la gestione di altre tipologie di ATTI (es. atti d’indirizzo e di sindacato ispettivo);
-	Miglioramento delle performance soprattutto nella parte di generazione delle stampe;
-	Miglioramento della sicurezza;
-	Introduzione di funzionalità che permetto di utilizzare la piattaforma in modalità stand-alone, gestendo in modalità nativa le funzioni di autenticazione, profilazione e di anagrafica.
 
La nuova versione è sviluppata utilizzando la tecnologia Microsoft MVC (model view controller) utilizzando C# come linguaggio di programmazione e il Framework .NET 4.7.2. 

#### STRUTTURA DEL SISTEMA
Come accennato, il nuovo portale PEM è stato progettato e sviluppato in modo da separare in maniera netta la parte server da quella client. 

![Struttura_sistema](/Documentazione/Screenshot/Struttura_sistema.jpg)
 
Le componenti principale del sistema risultano essere quelle in figura e descritte di seguito:
-	DATABASE:
Motore di database contenente i dati dell’applicazione, le funzioni e le procedure di basso livello. Si è scelto di utilizzate MS Sql Server come DBMS.
-	API:
È la parte core del sistema che contiene tutta la logica applicativa e di interfacciamento ad alto livello, tramite la modellazione di opportune classi, con la base dati di PEM. L’interfacciamento con la base dati è stato realizzato utilizzando un layer con EntityFramework 6
-	CLIENT:
È il modulo che interroga l’API e genera l’output finale (html, javascript, css) da inviare ai dispositivi client.
-	PROXYAD (WEBSERVICES DI AUTENTICAZIONE):
È un servizio web di tipo soap che si occupa di effettuare l’autenticazione e la profilazione degli utenti del sistema interfacciandosi con il repository Active Directory delle utenze di rete del CRL.
-	STAMPER (SCHEDULER DELLE STAMPE):
Al fine di migliorare le performance, le stampe in pdf generate da PEM avvengono principalmente in modalità asincrona utilizzando il modulo STAMPER che si occupa di generare i pdf e di inviarli via email al richiedente. Il modulo STAMPER è schedulato in modo da eseguire periodicamente il controllo della coda di stampa.

NOTA: 
Il progetto in produzione presso il Consiglio regionale della Lombardia utilizza un ulteriore webservices per la pubblicazione dei dai dati relativi agli emendamenti sul dataset dedicato all’interno del portale www.dati.lombardia.it. Questa funzionzionalità non è attiva nel sorgente pubblicato e tutte le chiamate al webservices sono state disabilitate attraverso l’impostazione della chiave presente nel web.config (AbilitaOpenData = 0)

#### API
Come accennato il modulo API è la parte core della soluzione PEM e contiene tutta la logica applicativa e di interfacciamento alla base dati.
L’API dialoga pertanto sia con il modulo client, per l’invio di tutte le informazioni necessarie alla creazione delle pagine web finali, sia con il DBMS per la lettura e la memorizzazione dei dati.
Per l’interfacciamento con il database è stato sviluppato un layer con EntityFramework 6. Questo agevola l’utilizzo anche di altri provider nel caso non si voglia usare Microsoft SQL server. Per tutti i database supportati fare riferimento alla guida [Panoramica di Entity Framework 6 - EF6 | Microsoft Docs](https://docs.microsoft.com/it-it/ef/ef6/)

#### API – AUTENTICAZIONE
Le richieste all’API vengono soddisfatte solo se il richiedente risulta correttamente autenticato ed autorizzato ad accedere alla risorsa/funzionalità richiesta. Per l’autenticazione, l’API fornisce un endpoint (di tipo allow-anonimous) che permette il riconoscimento tramite username e password. In caso di corretta autenticazione viene fornito un token (JWT) che dovrà essere utilizzato per tutte le successive richieste all’API. Il token è una stringa crittografata di caratteri con una scadenza configurabile e contiene i dati dell’utente (ruoli e gruppi di appartenenza).
Per informazioni più dettagliate su JWT token si può consultare la seguente guida: [JSON Web Tokens - jwt.io](https://jwt.io/)

Per evitare il proliferare di utenze e password, in Consiglio regionale della Lombardia, si è scelto di utilizzare, come primo livello di accesso al portale PEM, gli stessi user name e password utilizzati per l’accesso al dominio. Per effettuare l’autenticazione viene utilizzato un webservice soap che si interfaccia con il repository delle utenze di rete.

Per rendere il portale PEM immediatamente riusabile, è possibile utilizzare delle credenziali (username e password) memorizzate sul database interno di PEM. Per attivare questo tipo di autenticazione è necessario impostare la chiave AutenticazioneAD = 0 nel web.config dell’applicazione.

#### API – STRUTTURA (SOTTO-PROGETTI)
Il modulo API è stato sviluppato secondo una logica di sotto-progetti per separare logicamente le diverse tipologie di operazioni.
Tale scelta è stata effettuata seguendo la logica del riuso in modo da consentire la sostituzione/rielaborazione di una singola componente facilitando l’integrazione con le tecnologie in uso presso le diverse amministrazioni. 
I sotto-progetti realizzati sono i seguenti:
-	API: amministra il routing, valida i token, gestisce le configurazioni
-	BAL: (Business Access Layer): amministra ed elabora i dati scambiati
-	DTO: (Data Transfer Object): contiene i modelli che mappano in classi gli oggetti di database (table e view)
-	DOMAIN: contiene i modelli del database
-	CONTRACTS: contiene le interfacce per accedere ai dati e consente/nega l’esecuzione di operazioni specifiche sui diversi oggetti dell’applicazione
-	PERSISTANCE: implementazione delle interfacce, e accesso ai dati chiamando il modulo Database
-	DATABASE: contesto che accede e modella il provider dati interfacciandosi direttamente con la base dati.
-	COMMON: contiene funzionalità comuni a tutte i sotto-progetti e le rende disponibili
-	LOGGER: gestisce i log dell’applicazione. log4net. I log sono stati gestiti utilizzando la libreria log4net ( [Apache log4net – Apache log4net: Home - Apache log4net](http://logging.apache.org/log4net/) )
-	EXPRESSION BUILDER: gestire i filtri (query di interrogazione). E’ stato realizzato personalizzando il progetto opensouce ExpressionBuilder ( [dbelmont/ExpressionBuilder: A library that provides a simple way to create lambda expressions to filter lists and database queries. (github.com)](https://github.com/dbelmont/ExpressionBuilder) )

La soluzione del progetto in Visual Studio risulta quella in figura:

![Soluzione_VisualStudio_API](/Documentazione/Screenshot/Soluzione_VisualStudio_API.jpg)

#### API - FUNZIONI
Il sotto-progetto API costituisce la parte principale della parte API in quanto contiene e rende disponibili tutte le funzioni necessarie per gestire i vari oggetti del progetto. Secondo la logica MVC sono stati sviluppati i seguenti controller:
-	SEDUTECONTROLLER: contiene tutte le operazioni per gestire le sedute
-	ATTICONTROLLER: Contiene tutte le operazioni per gestire gli atti (Gestione articoli/commi/lettere, salvataggio relatori, gestione dei fascicoli in ordine di votazione/presentazione, ecc.)
-	EMENDAMENTICONTROLLER: Contiene tutte le operazioni per gestire gli emendamenti (Gestione firme/inviti/depositi, visualizzazioni di preview, modifica metadati, gestione stati, ordinamenti e fascicolazione, ecc.)
-	DASICONTROLLER: contiene tutte le operazioni per gestire gli atti di sindacato ispettivo.
-	PERSONECONTROLLER: contiene tutte le operazioni inerenti gli utenti del sistema e la gestione dei relativi ruoli e gruppi di appartenenza (swap ruoli/gruppi, visualizzazione utenti e ruoli del sistema, cambio pin, ecc.)
-	NOTIFICHECONTROLLER: contiene tutte le operazioni sulle notifiche
-	AUTENTICAZIONECONTROLLER: contiene tutte le operazioni di autenticazione al portale
-	ESPORTACONTROLLER: contiene le operazioni per le esportazioni di emendamenti (excel/word)
-	STAMPECONTROLLER: contiene tutte le operazioni per la gestione delle stampe
-	JOBCONTROLLER: contiene tutte le operazioni per i servizi esterni di stampa (effettua l’autenticazione utilizzando il ruolo SERVICE_JOB)
-	UTILCONTROLLER: contiene tutte le operazioni comuni dell’applicazione (es. invio mail)
-	ADMINCONTROLLER: contiene le funzioni per la gestione amministrativa del portale (definizione di utenti e password, impostazione dei ruoli, reset pin, configurazione gruppi politici, ecc.)

#### CLIENT
Il modulo client si occupa si generare le pagine web finali composte da html e librerie javscript e css. Le pagine vengono inviate ai web-browser per la visualizzazione. Il modulo CLIENT dialoga con il modulo API per la creazione delle pagine e la gestione dei diversi comandi e funzionalità del portale PEM. Come detto tutta la logica applicativa, la gestione dei permessi e l’interfacciamento con il database viene effettuato dal modulo API. Questo tipo di struttura separa in maniera netta l’interfaccia utente dalle logiche di business consentendo un’agevole sostituzione della parte client, ad esempio con un’App per dispositivi mobili Apple o Android.

#### CLIENT – STRUTTURA (SOTTO-PROGETTI)
Così come effettuato per il modulo API anche il modulo CLIENT è stato sviluppato secondo una logica di sotto-progetti per separare logicamente le diverse tipologie di operazioni per agevolare il riuso dell’applicazione permettendo la sostituzione/rielaborazione di singole componenti. 
I sotto-progetti realizzati nel modulo CLIENT sono i seguenti:
-	CLIENT: contiene le routine per generare l’interfaccia utente del portale
-	COMMON: contiene funzionalità comuni a tutte i sotto-progetti e le rende disponibili
-	DTO: (Data Transfer Object): contiene i modelli che mappano in classi gli oggetti di
-	GATEWAY: mette a disposizione le interfacce per la comunicazione tra API e CLIENT
-	LOGGER: gestisce i log dell’applicazione. log4net. I log sono stati gestiti utilizzando la libreria log4net

La soluzione del progetto in Visual Studio risulta quella in figura:

![Soluzione_VisualStudio_Client](/Documentazione/Screenshot/Soluzione_VisualStudio_Client.jpg)

#### CLIENT – FUNZIONI
Il sotto-progetto Client costituisce la parte principale della parte CLIENT. Secondo la logica MVC sono stati sviluppati i seguenti controller:
-	SEDUTECONTROLLER: gestisce tutte funzionalità per gestire le sedute interfacciandosi con SEDUTECONTROLLER del modulo API.
-	ATTICONTROLLER: gestisce tutte le funzionalità per gestire gli atti (gestione articoli/commi/lettere, relatori, fascicolazione, ecc.) interfacciandosi con ATTICONTROLLER del modulo API.
-	EMENDAMENTICONTROLLER: gestisce tutte le funzionalità per gestire gli emendamenti (gestione firme/inviti/depositi, preview, modifica metadati, gestione stati, ordinamenti/ fascicolazione, ecc.) interfacciandosi con EMENDAMENTICONTROLLER del modulo API.
-	DASICONTROLLER: gestisce tutte le funzionalità per gestire gli atti di sindacato ispettivo interfacciandosi con DASICONTROLLER dell'API.
-	PERSONECONTROLLER: gestisce tutte le funzionalità inerenti gli utenti del sistema e la gestione dei relativi ruoli e gruppi di appartenenza (swap ruoli/gruppi, visualizzazione utenti e ruoli del sistema, cambio pin, ecc.) interfacciandosi con PERSONECONTROLLER del modulo API.
-	NOTIFICHECONTROLLER: gestisce tutte le funzionalità sulle notifiche interfacciandosi con NOTIFICHECONTROLLER del modulo API.
-	AUTENTICAZIONECONTROLLER: gestisce tutte le operazioni di autenticazione al portale interfacciandosi con AUTENTICAZIONECONTROLLER del modulo API.
-	STAMPECONTROLLER: gestisce le stampe interfacciandosi con STAMPECONTROLLER del modulo API.
-	VIDEOTUTORIAL: Contiene i tutorial per l’utilizzo del PEM.

#### CLIENT – LIBRERIE
La parte di interfaccia utente di PEM è stata realizzata utilizzando le tecnologie attualmente più evolute che consentono la visualizzazione responsive dell’applicazione. In particolare, sono stati utilizzati:
-	Materialize (stile) - [Documentation - Materialize (materializecss.com)](https://materializecss.com/)
-	jQuery (javascript) - [jQuery](https://jquery.com/)
-	Trumbowyg (editor testo) - [https://alex-d.github.io/Trumbowyg/](https://alex-d.github.io/Trumbowyg/))

#### TEMPLATE
Per rendere il portale PEM adattabile ad esigenze di layout differenti e personalizzabili, la visualizzazione degli emendamenti, la stampa dei fascicoli e dei singoli emendamenti è stata sviluppata utilizzando dei templates html. Attraverso questi templates, contenuti nella cartella Template nel progetto API, è possibile personalizzare il layout degli emendamenti e dei fascicoli sia nella versione html (per visualizzazione a video e per invio tramite email) sia nella versione pdf.

#### GESTIONE DELLE STAMPE
Come detto precedentemente, per una questione di performance, le stampe in pdf vengono generalmente effettuate in modalità asincrona.
Per tala gestione è stato sviluppato un servizio windows (GAMScheduler), un programma di interfaccia (Scheduer) che permette la configurazione di tale servizio e un job di stampa che si occupa di generare e inviare via email, le stampe pdf.

#### SERVIZIO (GAMScheduler service)
Servizio sviluppato in C# utilizzando la libreria Quartz.NET ([Quartz.NET (quartz-scheduler.net)](https://www.quartz-scheduler.net/))
Il servizio legge le configurazioni dallo schedulatore, carica tramite reflection la dll del job e la schedula passandogli i parametri letti sempre da configurazione nello scheduler.
Tutta la documentazione relativa alla libreria Quarts.NET è reperibile nel sito ufficiale [Quartz.NET Quick Start Guide | Quartz.NET (quartz-scheduler.net)](https://www.quartz-scheduler.net/documentation/quartz-3.x/quick-start.html#download-and-install)

#### SCHEDULER
Programma windows form sviluppato in C# che gestisce il servizio di windows GAMScheduler, la schedulazione e la configurazione dei job.
La schedulazione del job viene gestita tramite cronExpression (triggers_config.json).
La configurazione viene gestita tramite file jobs_config.json.

#### JOB - STAMPA
E’ un estensione dell’interfaccia IJob ereditata da Quartz ed esegue queste lavorazioni:
-	Autenticazione con utente di servizio all’api (ruolo SERVIZIO_JOB)
-	Elabora fino a 20 stampe
-	Ogni stampa ha un tot di emendamenti da stampare
-	Scarica dall’api i template precompilati
-	Crea N task per creare i PDF degli emendamenti
-	Manda la mail con il link al fascicolo
-	In caso di deposito differito, viene anche generato il pdf dell’emendamento appena depositato e inviato via mail

La soluzione del progetto in Visual Studio risulta quella in figura:

![Soluzione_VisualStudio_JobStampa](/Documentazione/Screenshot/Soluzione_VisualStudio_JobStampa.jpg)

# Installazione

## Note sulla release

Il codice sorgente pubblicato è relativo alla piattaforma PEM nella release 2.0, che sostituirà la versione attualmente in uso presso il Consiglio regionale della Lombardia.
Questa nuova versione, oltre ad utilizzare esclusivamente librerie e componenti opensource, separa la parte client dell’applicazione da quella server attraverso lo sviluppo di API dedicate e introduce miglioramenti nelle performance e nella gestione delle stampe pdf. L’introduzione delle API per la gestione dei dati e delle elaborazioni principali facilita lo sviluppo di un’app per dispositivi mobili (Apple e Android).
NOTA: Questa release è attualmente in fase di test e viene rilasciata in versione beta.

## Requisiti del sistema

Specifiche tecniche server:

- Sistema Operativo: Windows 2008 Server R2 + Active Directory
- Web e Application server: IIS 7.5 + Entity Framework 6.0
- Database: Microsoft SQL server 2012 o superiore

Specifiche tecniche client:
- Sistema Operativo: Microsoft windows (7 – 8 - 10), Mac OsX
- Browser: Internet Explorer (ver. 9.0-10.0-11.0), FireFox (ultima versione disponibile), Chrome (ultima versione disponibile), Safari (ultima versione disponibile)
- Dispositivi mobile (tablet/cellulari): iOS, Android e WindowsPhone, il portale è completamente responsive

## Procedura di installazione

L'installazione prevede la creazione del database tramite lo script fornito, la compilazione dei sorgenti Client, la creazione di due application su IIS, una per le Api e una per il Client compilato, con la configurazione del file web.config dell'application Client con la connessione al database. Al termine, la schedulazione del modulo di stampa asincrona.

Per la procedura completa di installazione fare riferimento alla documentazione specifica:

- [Documentazione](/Documentazione/Installazione.md)
 

# Licenza

## Autore / Copyright

Portale PEM - Presentazione Emendamenti
2020 (c) Regione Lombardia

Concesso in licenza [GNU Affero General Public Licence version 3](https://www.gnu.org/licenses/agpl-3.0.html) (SPDX: AGPL-3.0)

## Licenze dei componenti di terze parti

All'interno del codice del Portale PEM sono stati utilizzati i seguenti componenti di terze parti, nell'ambito delle relative licenze qui indicate:
  
- Log4net
 https://github.com/apache/logging-log4net
 con licenza [Apache-2.0 License](https://github.com/apache/logging-log4net/blob/master/LICENSE)
 
 
- ITextSharp
 https://github.com/itext/itextsharp
 con licenza [GNU Affero General Public License version 3](https://github.com/itext/itextsharp/blob/develop/LICENSE.md)
 
 
- Trumbowyg
 https://github.com/Alex-D/Trumbowyg
 con licenza [MIT License](https://github.com/Alex-D/Trumbowyg/blob/develop/LICENSE)
 
 
- Materialize 
 https://github.com/Dogfalo/materialize/tree/master
 con licenza [MIT License](https://github.com/Dogfalo/materialize/blob/v1-dev/LICENSE)
 
 
- Quartz.NET (quartz-scheduler.net)
 https://www.quartz-scheduler.net/
 con licenza [Apache 2.0 License](https://github.com/quartznet/quartznet/blob/master/license.txt)
 
 
- ExpressionBuilder (dbelmont/ExpressionBuilder)
 https://github.com/dbelmont/ExpressionBuilder
 con licenza [Apache 2.0 License](https://github.com/dbelmont/ExpressionBuilder/blob/master/LICENSE)
 
 
- Newtonsoft.json - MIT License
 https://www.nuget.org/packages/Newtonsoft.Json
 con licenza [MIT License](https://licenses.nuget.org/MIT)
 
 
- AutoMapper
 https://www.nuget.org/packages/AutoMapper/
 con licenza [MIT License](https://licenses.nuget.org/MIT)
 
 
- NPOI
 https://www.nuget.org/packages/NPOI/
 con licenza [Apache 2.0 License](https://www.nuget.org/packages/NPOI/2.5.2/License)



## Dettagli della licenza

La licenza per questo repository è [GNU Affero General Public Licence version 3](https://www.gnu.org/licenses/agpl-3.0.html) (SPDX: AGPL-3.0).
Non è possibile utilizzare l'opera salvo nel rispetto della Licenza.

È possibile ottenere una copia della Licenza al seguente indirizzo: https://opensource.org/licenses/AGPL-3.0

Salvo diversamente indicato dalla legge applicabile o concordato per iscritto, il software distribuito secondo i termini della Licenza è distribuito "TAL QUALE", SENZA GARANZIE O CONDIZIONI DI ALCUN TIPO, esplicite o implicite.
 
Si veda la Licenza per la lingua specifica che disciplina le autorizzazioni e le limitazioni secondo i termini della Licenza.
 
Si veda il file [LICENSE.md](LICENSE.md) all'interno del repository per i riferimenti completi.
 
Il logo della Regione Lombardia è di proprietà esclusiva di Regione Lombardia e per tanto non è rilasciato sotto licenza aperta.
 
