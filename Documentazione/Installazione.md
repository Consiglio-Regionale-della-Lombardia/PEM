# Portale PEM/DASI - Presentazione EMendamenti e Dematerializzazione Atti Sindacato ispettivo e d'Indirizzo
# Procedura di installazione

Qui di seguito elenchiamo la procedura completa di installazione del Portale PEM/DASI. 

## Contenuti

- [Creazione del database](#creazione-del-database)
- [Verifica configurazione IIS](#verifica-configurazione-IIS)
- [Compilazione della soluzione client e API](#compilazione-soluzione)
- [Creazione Application su IIS](#creazione-application-su-IIS)
- [Schedulazione del modulo di stampa](#schedulazione-del-modulo-di-stampa)

## Creazione del database

Per la creazione del database, lanciare in esecuzione sul proprio Sql server prima lo script **dbEmendamenti_completo.sql** e subito dopo lo script contenuto **dbEmendamenti_dati.sql**, entrambe contenuti all'interno della cartella Database della release.
Al termine, il database dbEmendamenti sarà creato correttamente. Creare per lo stesso database, un utente locale "emendamenti".

![Installazione_creazione_database](/Documentazione/Screenshot/Installazione_creazione_database.jpg)

Nella stessa cartella Database della release, sono contenuti gli script singoli di ogni elemento del database, in caso di modifiche puntuali da apportare allo schema:
- 01_tables (tabelle)
- 02_views (viste)
- 03_functions (funzioni)
- 04_storeprocedures (stored procedure)
- 05_data_populate (popolamento tabelle)

Dopo aver eseguito lo script completo di generazione del db, che viene aggiornato periodicamente ma non ad ogni singola modifica/correzione effettuata, è necessario verificare la coerenza delle tabelle/store/viste/function con gli script singoli che invece sono sempre aggiornati, in "tempo reale", al verificarsi di ogni cambiamento.

## Verifica configurazione IIS

Prima di procedere all'installazione dell'interfaccia del Portale PEM, occorre verificare che l'IIS installato sul web server comprenda tutte le funzionalità richieste.
Accedere a **Program and features** del web server, poi alla voce **Turn Windows features on or off** e infine alla finestra **Add Roles and Features wizard**. 
Proseguire lungo i tab confermando quanto già presente con il pulsante **Next**, fino alla scheda **Server Roles**.

Le funzionalità aggiuntive rispetto al default riguardano alcune particolarità dell'Application Development, in particolare i moduli .NET Extensibility 3.5 e 4.6 e ASP.NET 3.5 e 4.6.
Spostarsi poi sulla scheda **Features** e anche qui verificare che siano presenti i moduli del .NET Framework 4.6.

![Installazione_verifica_IIS_1](/Documentazione/Screenshot/Installazione_verifica_IIS_1.jpg)
 
 
![Installazione_verifica_IIS_2](/Documentazione/Screenshot/Installazione_verifica_IIS_2.jpg)
 
 
![Installazione_verifica_IIS_3](/Documentazione/Screenshot/Installazione_verifica_IIS_3.jpg)
 
 
![Installazione_verifica_IIS_4](/Documentazione/Screenshot/Installazione_verifica_IIS_4.jpg)
 
 
![Installazione_verifica_IIS_5](/Documentazione/Screenshot/Installazione_verifica_IIS_5.jpg)



## Compilazione Soluzione
Dopo aver scaricato i sorgenti, aprire la soluzione Client e la soluzione API e compilarle. Se la compilazione non restituisce errori, copiare i compilati nelle rispettive cartelle sul server IIS (tipicamente `c:\inetpub\wwwroot\PEM\client` e `c:\inetpub\wwwroot\PEM\API`).

### Configurazione dei file Web.config / App.config

I `Web.config` di API, API Pubblica, Client e gli `App.config` dello Scheduler sono **versionati** e contengono solo i parametri non riservati: strutture XML, URL pubblici, percorsi di log, flag funzionalità. I parametri sensibili (chiavi JWT, master key, licenze, credenziali servizi) sono caricati a runtime da un file `Secrets.config` non versionato e ignorato dal `.gitignore`, mentre la connessione al database è caricata da `ConnectionStrings.config`.

Procedura al primo clone:

1. Attivare il pre-commit hook locale che impedisce dimenticanze sui template:

    ```
    git config core.hooksPath .githooks
    ```

2. In ognuno dei progetti che hanno un `Secrets.config.example`, duplicare il file rimuovendo il suffisso `.example` e compilare i valori reali del proprio ambiente. I progetti interessati sono:

    - `Sorgenti API\PortaleRegione.API\PortaleRegione.API\Secrets.config`
    - `Sorgenti API Pubblica\PortaleRegione.Api.Public\Secrets.config`
    - `Sorgenti Scheduler Quartz\Scheduler Quartz\Secrets.config`

3. Modificare i file `ConnectionStrings.config` di:

    - `Sorgenti API\PortaleRegione.API\PortaleRegione.API\ConnectionStrings.config`
    - `Sorgenti API Pubblica\PortaleRegione.Api.Public\ConnectionStrings.config`

   sostituendo la stringa di connessione di default (LocalDB) con quella del proprio SQL Server. Per evitare che le modifiche locali compaiano in `git status` o vengano committate per errore, marcare il file come skip-worktree:

    ```
    git update-index --skip-worktree "Sorgenti API/PortaleRegione.API/PortaleRegione.API/ConnectionStrings.config"
    git update-index --skip-worktree "Sorgenti API Pubblica/PortaleRegione.Api.Public/ConnectionStrings.config"
    ```

4. Aprire le soluzioni in Visual Studio e compilare. A questo punto F5 avvia le applicazioni in IIS Express con i parametri del proprio ambiente.

Se durante lo sviluppo si aggiunge una nuova chiave riservata nel `Secrets.config`, il pre-commit hook ne segnala l'assenza nel `Secrets.config.example` e blocca il commit finché il template non viene aggiornato (placeholder vuoto). Questo evita che chi forka si trovi un nome di chiave non documentato.

## Creazione Application su IIS

Dall'IIS Manager del web server, creare due nuove Application Pool, **API** e **CLIENT**, entrambe con configurazioni standard.
Dopo aver copiato il compilato API e il compilato Client sotto la cartella C:\inetpub\wwwroot del web server, sempre dall'IIS Manager convertirle in Application, impostando per ognuna la relativa Application Pool dedicata appena creata.

![Installazione_API_Client_1](/Documentazione/Screenshot/Installazione_API_Client_1.jpg)
 
 
![Installazione_API_Client_2](/Documentazione/Screenshot/Installazione_API_Client_2.jpg)



## Schedulazione del modulo di stampa

#### INSTALLAZIONE DEL SERVIZIO WINDOWS
Per installare il servizio seguire le istruzioni [Creating Windows Service In .NET with Topshelf (c-sharpcorner.com)](https://www.c-sharpcorner.com/article/creating-windows-service-in-net-with-topshelf/)

![Installazione_scheduler_1](/Documentazione/Screenshot/Installazione_scheduler_1.jpg)

#### PROGRAMMAZIONE SCHEDULER

Nella schermata principale, il tasto “play” avvio servizio, mentre il tasto “stop” (visibile quando il servizio è attivo) ferma il servizio.

![Installazione_scheduler_2](/Documentazione/Screenshot/Installazione_scheduler_2.jpg)

Con il tasto “+” si accede alla schermata di inserimento e configurazione dei parametri necessari al funzionamento del job (anche tramite cron expression).
Per modificare una riga esistente, fare doppio click sul record.

![Installazione_scheduler_3](/Documentazione/Screenshot/Installazione_scheduler_3.jpg)

Nella finestra di gestione dell'evento di schedulazione di inserisce un nome per la programmazione, si seleziona il lavoro (in questo caso "Genera Stampe"), la data di partenza della programmazione e la frequenza di esecuzione del job.

![Installazione_scheduler_4](/Documentazione/Screenshot/Installazione_scheduler_4.jpg)
















