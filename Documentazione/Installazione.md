# Portale PEM/DASI - Presentazione EMendamenti e Dematerializzazione Atti Sindacato ispettivo e d'Indirizzo
# Procedura di installazione

Qui di seguito elenchiamo la procedura completa di installazione del Portale PEM/DASI.

## Contenuti

- [Creazione del database](#creazione-del-database)
- [Verifica configurazione IIS](#verifica-configurazione-iis)
- [Compilazione delle soluzioni](#compilazione-delle-soluzioni)
  - [File di configurazione](#file-di-configurazione)
- [Creazione Application su IIS](#creazione-application-su-iis)
- [Schedulazione dei job](#schedulazione-dei-job)

## Creazione del database

Creare sul proprio SQL Server (2019 o successivo) un database vuoto con collation `Latin1_General_CI_AI`, la stessa che gli script dichiarano sulle colonne di testo delle tabelle di audit, e un utente dedicato all'applicazione (per esempio "emendamenti") da usare nelle stringhe di connessione.

Lo schema si crea con gli script della cartella Database della release, eseguiti sul database appena creato in quest'ordine e, dentro ogni cartella, in ordine alfabetico:

1. `01_tables` (tabelle)
2. `03_functions` (funzioni)
3. `02_views` (viste), cominciando da `dbo.View_UTENTI.View.sql`
4. `04_storeprocedures` (stored procedure)
5. `05_data_populate` (popolamento delle tabelle di supporto: tipi, ruoli, parti del testo, organi, legislature, aree politiche)
6. `07_Audit` (tabelle di audit)
7. `06_TRIGGERS` (trigger che scrivono le tabelle di audit)
8. `06_updates` (script di aggiornamento, in ordine di data)

Le funzioni vanno create prima delle viste, che in parte le usano (per esempio `View_UTENTI` usa `get_legislature_from_persona`), e `View_UTENTI` prima delle altre viste, diverse delle quali la leggono. Le tabelle di audit vanno create prima dei trigger che le scrivono e dello script di aggiornamento di EM, che modifica `EM_Audit`.

Molti script cominciano con un'istruzione `USE` che punta al database su cui sono stati generati: prima di eseguirli va tolta, oppure sostituita con il nome del proprio database.

Gli script di `06_updates` sono idempotenti e su un database creato con le cartelle precedenti non aggiungono nulla, perché le colonne ci sono già; quello di EM ricrea il trigger `TrigEM`, identico a quello di `06_TRIGGERS`. Su un database esistente si eseguono, in ordine di data, quelli non ancora applicati, prima di aggiornare le applicazioni (vedi [Struttura del database](/Database/DATABASE.md)).

Gli script `dbEmendamenti_completo.sql` e `dbEmendamenti_dati.sql` sono storici, fermi al 2022: non creano fra le altre le tabelle FILTRI, REPORTS, TEMPLATES, ATTI_RISPOSTE e ATTI_DOCUMENTI né le tabelle di audit, e non vanno usati per una nuova installazione. La schermata seguente si riferisce a quella procedura.

![Installazione_creazione_database](/Documentazione/Screenshot/Installazione_creazione_database.jpg)

## Verifica configurazione IIS

Prima di procedere all'installazione dell'interfaccia del Portale PEM, occorre verificare che l'IIS installato sul web server comprenda tutte le funzionalità richieste.
Accedere a **Program and features** del web server, poi alla voce **Turn Windows features on or off** e infine alla finestra **Add Roles and Features wizard**.
Proseguire lungo i tab confermando quanto già presente con il pulsante **Next**, fino alla scheda **Server Roles**.

Le funzionalità aggiuntive rispetto al default riguardano alcune particolarità dell'Application Development, in particolare i moduli .NET Extensibility 4.8 e ASP.NET 4.8.
Spostarsi poi sulla scheda **Features** e anche qui verificare che siano presenti i moduli del .NET Framework 4.8.

Le schermate seguenti vengono da un'installazione precedente e mostrano i moduli nelle versioni 3.5 e 4.6.

![Installazione_verifica_IIS_1](/Documentazione/Screenshot/Installazione_verifica_IIS_1.jpg)
 
 
![Installazione_verifica_IIS_2](/Documentazione/Screenshot/Installazione_verifica_IIS_2.jpg)
 
 
![Installazione_verifica_IIS_3](/Documentazione/Screenshot/Installazione_verifica_IIS_3.jpg)
 
 
![Installazione_verifica_IIS_4](/Documentazione/Screenshot/Installazione_verifica_IIS_4.jpg)
 
 
![Installazione_verifica_IIS_5](/Documentazione/Screenshot/Installazione_verifica_IIS_5.jpg)



## Compilazione delle soluzioni
Dopo aver scaricato i sorgenti, aprire e compilare le soluzioni delle tre applicazioni web:

- `Sorgenti Client/PortaleRegione.Client/PortaleRegione.Client.sln`
- `Sorgenti API/PortaleRegione.API/PortaleRegione.API.sln`
- `Sorgenti API Pubblica/PortaleRegione.Api.Public/PortaleRegione.Api.Public.sln`

Se la compilazione non restituisce errori, copiare i compilati sul server IIS in una cartella per ciascuna application, per esempio `C:\inetpub\wwwroot\GeDASI\Client`, `C:\inetpub\wwwroot\GeDASI\API` e `C:\inetpub\wwwroot\GeDASI\ApiPubblica`.

L'API e il job di stampa generano i pdf con Chromium tramite Playwright: alla prima stampa di ogni processo il codice lancia l'installazione del browser, che lo scarica se non è già presente. Il server deve quindi poterlo scaricare, oppure averlo già installato.

### File di configurazione

I `Web.config` di API, API Pubblica e Client e gli `App.config` dello Scheduler sono **versionati** e contengono solo i parametri non riservati: strutture XML, URL pubblici, percorsi di log, flag delle funzionalità. I parametri riservati e quelli che cambiano da un ambiente all'altro stanno in file esterni non versionati, da creare accanto al `Web.config` o all'`App.config` partendo dal template `.example`, che ne elenca le chiavi con i valori vuoti. Sul server i file esterni vanno creati allo stesso modo, accanto al `Web.config` di ciascuna application.

- API (`Sorgenti API\PortaleRegione.API\PortaleRegione.API`):
    - `Secrets.config`, dal template `Secrets.config.example`, agganciato con `<appSettings file="Secrets.config">`: chiavi JWT e dei servizi esterni, master key e master PIN, credenziali dell'utente di servizio dei job e di GEA, parametri della posta
    - `ConnectionStrings.config`, agganciato con `<connectionStrings configSource="ConnectionStrings.config" />`: è versionato con una stringa di connessione LocalDB da sostituire con quella del proprio SQL Server
    - `Edma.config`, dal template `Edma.config.example`, agganciato con il `configSource` della sezione `edmaSettings`: configurazione della protocollazione su EDMA
- API Pubblica (`Sorgenti API Pubblica\PortaleRegione.Api.Public`):
    - `Secrets.config`, dal template `Secrets.config.example`: master key, percorsi dei documenti e indirizzi delle pagine pubbliche degli atti. La `masterKey` deve essere identica a quella dell'API, altrimenti i dati certificati degli atti non si decifrano
    - `ConnectionStrings.config`, come per l'API
- Scheduler (`Sorgenti Scheduler Quartz\Scheduler Quartz`):
    - `Secrets.config`, dal template `Secrets.config.example`: utente e password di servizio con cui il programma legge dall'API i log delle stampe
- Importazione Dati Alfresco (`Sorgenti Importazione Dati Alfresco\PortaleRegione.C102.ImportazioneDatiAlfresco`):
    - `Secrets.config`, dal template `Secrets.config.example`: chiave di cifratura e stringa di connessione

API e API Pubblica partono anche se un file esterno manca, con conseguenze diverse. Senza `Secrets.config` le chiavi riservate risultano vuote. Senza `ConnectionStrings.config` la lettura della stringa di connessione, e quindi ogni accesso al database, fallisce con `ConfigurationErrorsException`. Senza `Edma.config` la sezione `edmaSettings` resta vuota e la protocollazione su EDMA non è configurata.

La chiave `EDMA_AbilitaEditManualeProtocollo` va tenuta uguale nell'`Edma.config` dell'API e nel `Web.config` del Client: l'API la usa per consentire la modifica manuale del campo Protocollo, il Client per mostrarne il comando.

Nel `Web.config` dell'API il commento su `JWT_EXPIRATION` dice "Valore in minuti", ma il codice la usa come ore (`AddHours` in `Sorgenti API/PortaleRegione.BAL/AuthLogic.cs`); anche `COOKIE_EXPIRE_IN` del `Web.config` del Client è in ore.

Procedura al primo clone:

1. Attivare il pre-commit hook locale che impedisce dimenticanze sui template, lanciando da Windows PowerShell:

    ```
    .\tools\setup-hooks.ps1
    ```

   Lo script imposta `core.hooksPath` su `.githooks`, come farebbe `git config core.hooksPath .githooks`. L'hook `.githooks/pre-commit` esegue `tools/check-secrets-template.ps1` con `powershell.exe`, quindi serve Windows PowerShell.

2. In ognuno dei progetti elencati sopra, duplicare i template rimuovendo il suffisso `.example` e compilare i valori reali del proprio ambiente.

3. Modificare i file `ConnectionStrings.config` di:

    - `Sorgenti API\PortaleRegione.API\PortaleRegione.API\ConnectionStrings.config`
    - `Sorgenti API Pubblica\PortaleRegione.Api.Public\ConnectionStrings.config`

   sostituendo la stringa di connessione di default (LocalDB) con quella del proprio SQL Server. Per evitare che le modifiche locali compaiano in `git status` o vengano committate per errore, marcare il file come skip-worktree:

    ```
    git update-index --skip-worktree "Sorgenti API/PortaleRegione.API/PortaleRegione.API/ConnectionStrings.config"
    git update-index --skip-worktree "Sorgenti API Pubblica/PortaleRegione.Api.Public/ConnectionStrings.config"
    ```

4. Aprire le soluzioni in Visual Studio e compilare. A questo punto F5 avvia le applicazioni in IIS Express con i parametri del proprio ambiente.

Il controllo dell'hook confronta, per ogni `Secrets.config` e `Edma.config` locale, i nomi delle chiavi con quelli del template; i valori non vengono controllati e i file che non esistono in locale vengono saltati. Se il file locale contiene una chiave assente dal template, il commit viene bloccato finché il template non viene aggiornato (placeholder vuoto); le chiavi presenti solo nel template vengono segnalate come avviso. Questo evita che chi forka si trovi un nome di chiave non documentato.

## Creazione Application su IIS

Dall'IIS Manager del web server creare tre Application Pool, **CLIENT**, **API** e **APIPUBBLICA**, tutte con configurazione standard.
Dopo aver copiato i compilati nelle rispettive cartelle sotto `C:\inetpub\wwwroot\GeDASI`, sempre dall'IIS Manager convertire ciascuna cartella in Application, impostando per ognuna la relativa Application Pool dedicata appena creata.

![Installazione_API_Client_1](/Documentazione/Screenshot/Installazione_API_Client_1.jpg)
 
 
![Installazione_API_Client_2](/Documentazione/Screenshot/Installazione_API_Client_2.jpg)



## Schedulazione dei job

I job pianificati sono tre e girano tutti nel servizio Windows dello Scheduler:

- GeneraStampe (`Sorgenti modulo di stampa asincrona/Jobs/GeneraStampeJob`): genera le stampe pdf richieste in modalità asincrona e le invia via email
- CalcoloRitardoAttoJob (`Sorgenti modulo calcolo ritardo/Jobs/CalcoloRitardoAttoJob`): aggiorna i giorni di ritardo nella risposta agli atti (`ATTI_DASI.Ritardo`)
- CleanLogRetention (`Sorgenti modulo retention/Jobs/CleanLogRetention`): cancella le righe vecchie delle tabelle di audit

#### INSTALLAZIONE DEL SERVIZIO WINDOWS
Il servizio è il progetto `Sorgenti Scheduler Quartz/Scheduler Service` (`SchedulerService.exe`), basato su Topshelf e configurato per girare con l'account LocalSystem. Per installarlo seguire le istruzioni [Creating Windows Service In .NET with Topshelf (c-sharpcorner.com)](https://www.c-sharpcorner.com/article/creating-windows-service-in-net-with-topshelf/).
Il nome del servizio non è impostato nel codice e si sceglie all'installazione: deve coincidere con la chiave `ServiceName` dell'`App.config` del programma Scheduler (`Sorgenti Scheduler Quartz/Scheduler Quartz`), che nel repository vale `ScheduleService`.

Nell'`App.config` del servizio si indicano il percorso di `jobs_config.json` (`PathJobsConfig`), quello di `triggers_config.json` (`PathTriggerConfig`) e la cartella dei job (`PathCustomJobs`), a cui il servizio accoda il percorso della dll registrato in `jobs_config.json`. Gli stessi due file vanno indicati nell'`App.config` del programma Scheduler, insieme all'indirizzo dell'API (`UrlApi`). I due file devono esistere: la cartella del programma Scheduler ne contiene una copia vuota.

Il servizio legge la configurazione e carica le dll dei job solo all'avvio. Per sostituire la dll di un job si ferma il servizio, si copia la nuova dll e si riavvia il servizio.

![Installazione_scheduler_1](/Documentazione/Screenshot/Installazione_scheduler_1.jpg)

#### PROGRAMMAZIONE SCHEDULER

Nella schermata principale il tasto "play" avvia il servizio, mentre il tasto "stop" (visibile quando il servizio è attivo) lo ferma. Con il servizio attivo i pulsanti dei lavori, delle programmazioni e di inserimento sono disattivati: per cambiare la configurazione si ferma il servizio.

![Installazione_scheduler_2](/Documentazione/Screenshot/Installazione_scheduler_2.jpg)

Con il tasto "+" si inserisce un lavoro o una programmazione, a seconda della vista attiva. Per modificare una riga esistente, fare doppio click sul record.
Un lavoro si inserisce scegliendo un file .zip con la dll del job e le sue dipendenze; nello zip deve esserci una sola dll con "job" nel nome. Il programma estrae lo zip nella cartella `CustomJobs\<nome del lavoro>` e registra il lavoro in `jobs_config.json` con i parametri ricavati dalle proprietà pubbliche della classe del job e i valori vuoti, da compilare poi con un doppio click sul lavoro.

![Installazione_scheduler_3](/Documentazione/Screenshot/Installazione_scheduler_3.jpg)

Nella finestra di gestione dell'evento di schedulazione si inserisce un nome per la programmazione, si seleziona il lavoro (in questo caso "Genera Stampe"), la data di partenza della programmazione e la frequenza di esecuzione del job, oppure un'espressione cron.

![Installazione_scheduler_4](/Documentazione/Screenshot/Installazione_scheduler_4.jpg)

#### PARAMETRI DEI JOB

I nomi dei parametri sono quelli che il programma Scheduler propone all'inserimento del lavoro e distinguono maiuscole e minuscole.

GeneraStampe (`Sorgenti modulo di stampa asincrona/Jobs/GeneraStampeJob/GeneraStampeJobFramework/Genera.cs`):

- `Username` e `Password`: utente di servizio con cui il job si autentica all'API, uguale a `Service_Username` e `Service_Password` del `Secrets.config` dell'API
- `UrlApi_Internal`: indirizzo dell'API usato per tutte le chiamate del job
- `UrlApi`: letto ma non usato dal job
- `UrlClient`: indirizzo del Client, per i link di download inviati via email
- `ConnectionString`: stringa di connessione al database
- `StoreProcedure`: stored procedure che preleva e blocca le stampe da lavorare; lo script `04_storeprocedures/dbo.PickAndLockStampe.StoredProcedure.sql` crea `PickAndLockStampe`
- `NumMaxTentativi`: numero massimo di tentativi per ogni stampa
- `CartellaLavoroTemporanea`: cartella di lavoro dei file intermedi
- `CartellaLavoroStampe`: cartella in cui vengono salvati i file prodotti
- `EmailFrom`: mittente delle email
- `RootRepository`: cartella in cui sono archiviati i pdf degli atti, riusati finché la stampa dell'atto è valida
- `PercorsoCompatibilitaDocumenti`: cartella degli allegati degli atti, accodati ai pdf
- `masterKey`: chiave di cifratura, la stessa dell'API

Attenzione alla grafia di `UrlApi_Internal`. Il valore arriva all'indirizzo usato dal job solo con questo nome, quello proposto dal programma Scheduler. Con la grafia `UrlAPI_Internal` il metodo `ConvertParameters` legge la chiave ma ne scrive il valore in `UrlApi` (righe 77-78 di `Genera.cs`): l'indirizzo interno resta vuoto e il job non raggiunge l'API.

CalcoloRitardoAttoJob (`Sorgenti modulo calcolo ritardo/Jobs/CalcoloRitardoAttoJob/MainJob.cs`):

- `connectionString`: stringa di connessione al database

CleanLogRetention (`Sorgenti modulo retention/Jobs/CleanLogRetention/MainJob.cs` e `Worker.cs`):

- `connectionString`: stringa di connessione al database
- `retention`: giorni di conservazione; il job cancella le righe con data più vecchia
- `tables`: elenco delle tabelle separate da virgola; ogni voce è `TABELLA` oppure `TABELLA:COLONNA`, dove la colonna è quella della data e vale `DatAudit` se non indicata
- `pathReport`: cartella in cui il job scrive il log `log_clean_retention_AAAAMMGG.txt`
