# Portale PEM - Presentazione Emendamenti
# Procedura di installazione

Qui di seguito elenchiamo la procedura completa di installazione del Portale PEM. 

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
Dopo aver scaricato sorgenti della soluzione client e API, è necessario aprire la soluzione Client e la soluzione Api ed effettuare la compilazione delle due soluzioni. Se la compilazione non restituisce errori è possibile copiare le due soluzioni compilate nelle rispettive cartelle predisposte sul server IIS (tipicamente c:\inetpub\wwwroot\PEM\client e c:\inetpub\wwwroot\PEM\API) e i file di configurazione:

- c:\inetpub\wwwroot\PEM\client\web.config (https://github.com/Consiglio-Regionale-della-Lombardia/PEM/blob/master/Sorgenti%20Client/PortaleRegione.Client/PortaleRegione.Client/Web.config.txt)
- c:\inetpub\wwwroot\PEM\API\web.config (https://github.com/Consiglio-Regionale-della-Lombardia/PEM/blob/v2.2/Sorgenti%20API/PortaleRegione.API/PortaleRegione.API/Web.config.txt)

I due file di configurazione devono essere aggiornati inserendo correttamente i parametri di configurazione con valori relativi al proprio ambiente (connessione al database server, attivazione funzionalità, ecc). Nella versione pubblicata su questo repository i due web.config sono in versione testuale (.txt) e quindi vanno rinominati togliendo l'estensione .txt affinchè possano essere correttamente interpretati dal framework.net. Nelle versioni testuali dei due file di configurazione è stata inserita una breve descrizione esplicativa su ogni parametro per facilitare le impostazioni.

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
















