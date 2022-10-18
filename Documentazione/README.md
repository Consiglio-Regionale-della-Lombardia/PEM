# Portale digitalizzazione atti - Manuale per gli utenti

## Introduzione 
Il portale PEM-DASI è stato realizzato per digitalizzare e gestire a distanza la fase di presentazioni degli Atti di iniziativa dei consiglieri regionali ed automatizzare alcune fasi del loro iter. il portale è stato inizialmente studiato per digitalizzare la presentazione degli emendamenti/subemendamenti ai progetti di legge (modulo PEM) e, a seguito del riscontro positivo da parte degli utilizzatori, è stato esteso alla presentazione degli atti di indirizzo e sindacato ispettivo, realizzando il modulo DASI.

Il modulo DASI e il modulo PEM sono ospitati nella medesima piattaforma, ci sarà quindi un doppio canale di accesso: 
1) un canale per la presentazione degli Emendamenti ai progetti di legge, nell’ambiente quindi legato a una specifica seduta e attivato in concomitanza della stessa;
2) un canale per la presentazione degli ATTI di indirizzo e sindacato ispettivo, non necessariamente legato a una specifica seduta del Consiglio.

Va puntualizzato che, per i due canali di accesso, la categoria di utenti non è coincidente: gli assessori, in quanto non sono consiglieri o sono sospesi dalla carica, non accederanno al canale DASI.

![00_login](/Documentazione/Screenshot/00_login.jpg)

## Procedura di autenticazione, di firma e immodificabilità

Il portale prevede per entrambi i moduli due livelli di autenticazione: un primo livello per l’accesso al sistema e la gestione delle bozze degli atti; un secondo livello per effettuare le operazioni con rilevanza pubblica che garantisca l’identificabilità del soggetto che effettua l’operazione.

### Accesso al sistema (autenticazione di primo livello)

Per accedere al sistema vengono fornite, ad ogni utilizzatore, uno username e una password di dominio personali. Attraverso queste credenziali avviene l’identificazione dell’utente e sono concessi i diversi livelli di visibilità e autorizzazione.
Gli utenti appartenenti alla Giunta Regionale (Presidente, Assessori, Sottosegretari e relativo personale di segreteria) normalmente non fanno parte del dominio del Consiglio Regionale. L’applicativo fornisce quindi un’interfaccia web che permette ad ogni titolare la modifica in autonomia della propria password.

### PIN dispositivo (autenticazione di secondo livello)

Per effettuare le operazioni di firma e di deposito viene scelto, da ogni soggetto titolato, un PIN dispositivo di 4 caratteri alfanumerici case-sensitive (o di 4 cifre).
Come previsto dall’attuale normativa in materia di trattamento dati personali, il PIN deve essere scelto dall’intestatario stesso.
I PIN dispositivi sono forniti ai seguenti soggetti:
- Consiglieri regionali: uno per ogni consigliere (che ne sarà custode). Può essere utilizzato per le operazioni di firma e di deposito di atti già firmati.
- Dirigenti delle segreterie del gruppo: uno per ogni dirigente (che ne sarà custode). Può essere utilizzato esclusivamente per le operazioni di deposito di atti già firmati.
- Assessori/Sottosegretari regionali: uno per ogni assessore/sottosegretario (che ne sarà custode). Può essere utilizzato per le operazioni di firma e di deposito di atti già firmati.
- Dirigenti delle segreterie degli assessori/sottosegretari: uno per ogni dirigente (che ne sarà custode). Può essere utilizzato esclusivamente per le operazioni di deposito di atti già firmati.
- Personale delle segreterie politiche, su indicazione del Capogruppo. Può essere utilizzato esclusivamente per le operazioni di deposito di atti già firmati.

Di particolare importanza, sotto il profilo della sicurezza del sistema, è la modalità con cui vengono forniti/generati e conservati i PIN dispositivi. Il portale comprende delle funzionalità in grado di rispettare quanto prescritto dall'attuale normativa riguardante l’utilizzo di sistemi informatici (soprattutto se trattano dati personali o riservati) e allo stesso tempo garantire che i PIN siano conservati in maniera sicura.
La gestione dei PIN dispositivi rispetta le seguenti caratteristiche:

- SEGRETEZZA: il PIN viene conosciuto esclusivamente dal titolare ed eventualmente dall’amministratore del sistema PEM (che vi può accedere solo per necessità tecniche o in caso di contestazioni);
- SICUREZZA: il PIN viene custodito in un archivio sicuro, accessibile solo al personale incaricato e protetto da sistemi crittografici che ne impediscono la visualizzazione “in chiaro” e la modifica manuale dello stesso. Sono inoltre previsti adeguati piani di Backup dell’archivio;
- IMMODIFICABILITA’: il PIN, una volta generato/creato, non può più essere né cancellato né modificato. L’amministratore del sistema o l’intestatario del PIN, attraverso apposita funzionalità messa a disposizione dalla piattaforma, può generare/creare un nuovo PIN che sostituisce il precedente (che viene marcato temporalmente e mantenuto in archivio per poter decrittografare vecchi ATTI firmati). 

E' inoltre prevista per l’Amministratore del Sistema una funzionalità che permette di generare un nuovo PIN dispositivo casuale e che va obbligatoriamente sostituito dall’intestatario al primo accesso.

### Modalità di utilizzo del PIN dispositivo

Per gestire il requisito di immodificabilità del numero e del testo degli ATTI firmati, della data e ora, nonché delle firme, si utilizza la crittografia. Attraverso la crittografia tutte le informazioni che non devono poter essere modificate sono rese “trattabili” solo attraverso il Sistema PEM-DASI.
L’utilizzo dei PIN dispositivi avverrà e sarà regolato come descritto successivamente.

1. FIRMA DI UN ATTO DA PARTE DEL TITOLARE DELL’INIZIATIVA
Al momento della firma da parte del “primo firmatario”: il software richiede l’immissione del PIN all’utente/consigliere/assessore/sottosegretario e accedendo all’archivio delle chiavi, ne verifica la validità e la coerenza (ovvero verifica se quel PIN, seppur corretto, appartiene al soggetto titolato a firmare l’emendamento). Utilizza il PIN immesso dal primo firmatario per crittografare l'ATTO (arricchito con i metadati relativi ad articolo, comma, ecc…) rendendolo immodificabile. Utilizzando una chiave conosciuta solo allo sviluppatore del sw, che chiameremo chiave-embedded, crittografa la firma ovvero l’identificativo del firmatario e la data e l’orario in cui è avvenuta l’operazione.

2. FIRME SUCCESSIVE ALLA PRIMA
Al momento della firma: il software richiede all’utente l’immissione del PIN e accedendo all’archivio delle chiavi, ne verifica la validità e la coerenza (ovvero verifica se quel PIN, seppur corretto, appartiene a un soggetto titolato a firmare l’ATTO). Utilizzando la chiave embedded, crittografa la firma ovvero l’identificativo del firmatario e l’orario in cui è avvenuta l’operazione.

3. INVITO PER LA SOTTOSCRIZIONE DI UN ATTO
Al fine di identificare in maniera certa il primo firmatario di un atto, unico soggetto titolato ad invitare per la sottoscrizione un consigliere appartenente ad un altro gruppo, al momento dell’invito il software richiede il PIN dispositivo e ne verifica l’appartenenza al primo firmatario per procedere con l’operazione.

4. DEPOSITO DEGLI ATTI
Al momento del deposito: il software richiede l’immissione di un PIN e accedendo all’archivio delle chiavi, ne verifica la validità e la coerenza (ovvero verifica se quel PIN, seppur corretto, appartiene a un soggetto titolato ad effettuare il deposito). 
Se il PIN è valido effettua il deposito dell’ATTO, crittografando: il numero dell’ATTO appena generato dal sistema; la data e l’ora di deposito rilevata dal server; una stringa contente l’elenco (nominativo – data e ora) dei firmatari dell’ATTO; l’identificativo del soggetto che ha effettuato il deposito. La crittografia viene effettuata utilizzando la chiave-embedded. Dopo il deposito, l’ATTO diventa immodificabile.

5. RITIRO DI UNA FIRMA
Per poter ritirare una firma ad un ATTO il software richiede l’immissione del proprio PIN e accedendo all’archivio delle chiavi, ne verifica la validità e la coerenza (ovvero verifica se quel PIN appartiene al soggetto a cui appartiene anche la firma che si sta tentando di ritirare).
Utilizzando la chiave embedded crittografa e memorizza i dati relativi al ritiro della firma ovvero l’orario in cui è avvenuta l’operazione. (Nel caso un ATTO non sia ancora stato depositato, la firma viene eliminata dal sistema)

## Ruoli degli utenti
Di seguito sono riportati i ruouli ricoperti dagli utenti all'interno dei moduli PEM e DASI, fatto salvo per gli utenti della Giunta regionale i quali possono operare solo all'interno del modulo PEM:

- Servizio Segreteria Dell’assemblea (PEM-DASI)
- Segreteria Dei Diversi Gruppi Consiliari (PEM-DASI)
- Responsabile Di Segreteria Dei Diversi Gruppi Consiliari (PEM-DASI)
- Consiglieri Regionali (PEM-DASI)
- Presidente Della Giunta Regionale (PEM)
- Assessori E Sottosegretari Regionali (PEM)
- Segreterie Di Presidente/Assessori/Sottosegretari (PEM)
- Responsabili Di Segreteria Di Presidente/Assessori/Sottosegretari (PEM)
- Amministratori Di Giunta (PEM)
- Amministratori del sistema (PEM-DASI)
- Segreterie particolari dei componenti dell’Ufficio di presidenza (PEM-DASI)

## Integrazione con altri applicativi presenti nel Consiglio regionale della Lombardia
Sotto il profilo degli applicativi, nell’iter di ciascun ATTO intervengono principalmente la banca dati GEASI, (banca dati per la Gestione degli atti di sindacato e di indirizzo), dinamicamente correlata alle pagine web del sito istituzionale, e la piattaforma EDMA (sistema documentale e di protocollazione) per la gestione delle comunicazioni elettroniche relative a ciascun atto. Queste integrazioni possono essere disabilitate tramite opportuni parametri di configurazione.

Altro applicativo con cui i moduli PEM-DASI dialogano è Ge.CO. disponibile su gitHub all’url https://github.com/Consiglio-Regionale-della-Lombardia/GeCo. L’applicazione GeCo (GEstione COnsiglieri) permette la gestione e consultazione delle informazioni istituzionali relative ai Consiglieri e agli Assessori regionali. Attraverso questa integrazione la piattaforma PEM-DASI “recupera” tutte le informazioni relative a:
- Anagrafica dei consiglieri regionali
-	Anagrafica degli assessori e sottosegretari
-	Anagrafica e composizione dei gruppi politici
-	Cariche dei consiglieri e degli assessori
-	Anagrafica delle commissioni consiliari
Per tale motivo il funzionamento della piattaforma è strettamente legato a questa integrazione. In assenza di tale integrazione, per poter utilizzare la piattaforma, è necessario sviluppare opportuni moduli per la gestione/integrazione delle suddette anagrafiche.

A seguire, verranno illustrate le differenti funzionalità del Portale suddivise per i due moduli PEM e DASI.


# Modulo PEM - Presentazione Emendamenti
Gli emendamenti sono proposte di modifica riferite ad uno specifico punto di un progetto di legge (titolo, articolo, comma, allegato, ecc.) prima che questo venga votato dall’assemblea legislativa regionale. I subemendamenti sono invece proposte di modifica riferite ad un emendamento precedentemente presentato. Il Portale PEM aiuta a de-materializzare e informatizzare le procedure del Consiglio Regionale per la presentazione di emendamenti/subemendamenti (EM/SUBEM).

- [Introduzione](#introduzione)
- [Servizio Segreteria dell’Assemblea](#servizio-segreteria-dellassemblea)
- [Segreteria dei diversi gruppi consiliari](#segreteria-dei-diversi-gruppi-consiliari)
- [Responsabile di segreteria dei diversi gruppi consiliari](#responsabile-di-segreteria-dei-diversi-gruppi-consiliari)
- [Consiglieri regionali](#consiglieri-regionali)
- [Assessori e sottosegretari regionali](#assessori-e-sottosegretari-regionali)
- [Presidente della Giunta Regionale](#presidente-della-giunta-regionale)
- [Segreterie di presidente/assessori/sottosegretari](#segreterie-di-presidenteassessorisottosegretari)
- [Responsabili di segreteria di presidente/assessori/sottosegretari](#responsabili-di-segreteria-di-presidenteassessorisottosegretari)
- [Amministratori di Giunta](#amministratori-di-giunta)
- [Allegati](#allegati)

## Introduzione Modulo PEM

L'obiettivo del Portale PEM è di rendere più efficiente e funzionale l’intera procedura di gestione degli emendamenti ai progetti di legge, con un’applicazione multiutente con livelli di informatizzazione e automazione più o meno ampi, a seconda delle varie fasi del processo.

Il Portale PEM permette di:

- gestire da remoto la predisposizione dei testi degli EM/SUBEM, la firma e l’operazione di deposito;
- garantire autenticità del testo, intesa come integrità del testo, provenienza del documento dal suo autore e certezza temporale sulla sua presentazione;
- snellire le operazioni per la lavorazione degli EM/SUBEM (numerazione, marcatura, autenticazione, ecc…);
- ridurre i tempi necessari alla generazione di output finalizzati alla discussione in aula e alle fasi post-seduta, attraverso la loro generazione automatica
- agevolare i lavori d'aula: visualizzare, gestire e modificare in Aula il testo degli emendamenti;
- ridurre l’utilizzo della carta e dei supporti per la stampa.

Per illustrare le funzionalità del modulo PEM verranno di seguito descritte le principali attività e funzionalità dei diversi attori del sistema.

## Servizio Segreteria dell’Assemblea 
Gli utenti del Servizio Segreteria dell’Assemblea si possono collegare al sistema utilizzando le proprie credenziali di rete.
Tramite il Portale PEM, predispongono le informazioni relative alle sedute consiliari e inseriscono a sistema gli atti emendabili, in base all’OdG della seduta consiliare, indicando la data della seduta e il termine (data e orario) stabilito per la presentazione degli emendamenti. A decorrere da tale orario il sistema “marcherà” gli emendamenti come “pervenuti oltre il termine ordinario”.

![PEM_1](/Documentazione/Screenshot/PEM_1.jpg)

![PEM_2](/Documentazione/Screenshot/PEM_2.jpg)

Creata la seduta, è possibile inserire i Progetti di Legge, sui quali potranno essere proposti degli emendamenti, indicando le seguenti informazioni: TIPO DI ATTO, NUMERO DI ATTO, OGGETTO, INDICAZIONE DEGLI ARTICOLI CON RELATIVI COMMI e LETTERE, RELATORI e ASSESSORE DI RIFERIMENTO.

![PEM_3](/Documentazione/Screenshot/PEM_3.jpg)

All’atto di inserimento del PDL per ogni parte emendabile codificata a sistema (articolo, comma, lettera) si potrà aggiungere il relativo testo e l’eventuale rubrica. L’inserimento è facilitato da una griglia che compone in maniera “visuale” il testo del progetto di legge man mano che vengono inserite le informazioni. 

![PEM_4](/Documentazione/Screenshot/PEM_4.jpg)

Cliccando sull’ articolo/comma/lettera si aprirà una tendina editabile all’interno della quale sarà possibile inserire il testo. 

![PEM_5](/Documentazione/Screenshot/PEM_5.jpg)

Terminata la predisposizione della seduta potranno visualizzare in un’apposita GrigliadiLavoro gli EM/SUBEM man mano depositati, raggruppati per atto (datati e numerati automaticamente dal sistema) e ordinati a partire dagli ultimi EM/SUBEM depositati.

![PEM_6](/Documentazione/Screenshot/PEM_6.jpg)

Scaduto il termine di presentazione, rendono pubblico l’elenco degli EM/SUBEM in ordine cronologico.

Attraverso un’apposita funzione di riordino semi automatica, gli utenti del Servizio Segreteria dell’Assemblea riordinano gli emendamenti in base all’ordine di votazione in aula. Questa funzionalità può essere schematizzata nei seguenti passi:
 
1. l’ufficio visualizza una griglia, denominata GrigliadiLavoro, contenente gli estremi degli emendamenti;
 
2. tramite un apposito bottone, il sistema effettua un primo ordinamento automatico degli EM/SUBEM, utilizzando come informazioni discriminanti la parte (titolo, capo, articolo, comma, lettera,…) a cui l’EM/SUBEM si riferisce e il tipo di EM/SUBEM (seguendo questo ordine: 1° soppressivo, 2° interamente modificativo, 3° modificativo, 4° aggiuntivo) (art. 92 - R.G.). Affinché il riordino funzioni tali informazioni devono essere indicate obbligatoriamente in fase di presentazione dell’EM/SUBEM;
 
3. dopo aver generato una prima lista ordinata (per votazione) degli EM/SUBEM, l’operatore può raffinare l’ordinamento spostando le righe della griglia.
 
4. l’elenco degli emendamenti in ordine di votazione, durante la fase di lavorazione, è visibile solo alla Segreteria Assemblea. L’elenco degli EM/SUBEM in ordine di votazione viene pubblicato (disponibile per la visualizzazione a tutti gli utenti del sistema) solo a lavorazione conclusa, su disposizione del dirigente della Segreteria Assemblea.
 
NOTA: le informazioni relative alla parte oggetto di modifica (titolo, capo, articolo, comma, lettera,…) e il tipo di EM/SUBEM presenti in griglia possono essere modificate dal personale della Segreteria dell’Assemblea per poter gestire i casi di informazioni non corrette inserite dai presentatori degli emendamenti. Tale modifica opera solo sui metadati dell’EM ma non va a sovrascrivere in nessun caso le informazioni indicate dai presentatori e presenti al momento del deposito (che sono contenute e crittografate nel testo dell’EM/SUBEM – Vedi Allegato 1). La griglia on-line pubblicata, unica per tutti, può essere lavorata anche in contemporanea, dal personale autorizzato, in qualsiasi momento e gli effetti sono visibili a tutti in tempo reale. Possibile anche un’esportazione in excel/PDF dei metadati contenuti nella griglia per lavorazioni diverse.

La GrigliadiLavoro è completa di strumenti per filtrare gli emendamenti presenti ed inserire annotazioni. Permette l’individuazione, la selezione rapida e la catalogazione degli EM/SUBEM inammissibili, ritirati, decaduti, respinti, approvati e di quelli presentati oltre i termini regolamentari (12.30 del giorno antecedente la seduta). Alcune operazioni possono essere effettuate su blocchi di emendamenti (es. decadenza di 10 emendamenti con un’unica operazione).
Durante la discussione in aula, attraverso la GrigliadiLavoro, è inoltre possibile indicare attraverso una spunta l’EM/SUBEM attualmente in discussione/votazione.

Terminato l'ordinamento per votazione degli EM/SUBEM depositati, gli utenti del Servizio Segreteria dell’Assemblea generano, attraverso un’apposita funzionalità del sistema, il file contenente i testi degli EM/SUBEM in ordine di votazione.

Attraverso opportuni alert via email, ricevono inoltre notifiche relative all’inserimento di eventuali EM/SUBEM presentati dopo il termine ordinario.

Durante l’intero processo la Segreteria dell’Assemblea gestisce gli EM/SUBEM catalogando quelli inammissibili, uguali tra loro nel contenuto (che verranno visualizzati con colore diverso), con variazione scalare, ritirati, approvati, ecc…; può apporre note agli EM/SUBEM “editando” i metadati sulla GrigliadiLavoro. Le note costituiscono un metadato relativo all’EM/SUBEM. Alcune informazioni, presenti nella griglia (es. respinti, approvati, decaduti), vengono visualizzate in una zona non crittografata dell’EM/SUBEM attraverso note inserite a discrezione degli operatori.

Gli utenti del Servizio Segreteria dell’Assemblea inseriscono a sistema l’orario di inizio seduta. Durante la fase d’aula poi, presidiano in tempo reale la discussione e la votazione degli EM/SUBEM, modificando i metadati relativi allo “stato” (es. respinto, approvato, decaduto,…) e apponendo note che potranno essere riservate (visibili solo al Servizio) oppure visualizzate in un’apposita area non crittografata sul testo dell’EM/SUBEM.

Durante la fase d’aula possono anche effettuare, in tempo reale, modifiche apportate in fase di votazione sul testo dell’EM/SUBEM oggetto di discussione. Le modifiche, che in nessun modo devono alterare l’EM/SUBEM originale depositato, vengono contestualmente visualizzate sotto l’EM/SUBEM.

A votazione avvenuta, dispongono la “chiusura” dei PDL trattati per bloccare la possibilità di aggiungere emendamenti. Successivamente al termine della seduta:
- generano il file con i testi dei soli emendamenti approvati;
- stampano gli emendamenti per l’unione nel fascicolo cartaceo.

Dopo aver correttamente classificato gli emendamenti/subemendamenti in base all’esito della votazione in Aula (approvati, respinti, decaduti, ecc) possono visualizzare gli EM/SUBEM approvati posizionati direttamente sul “PDL strutturato in parti” (articoli, commi, lettere, ecc). Gli EM/SUBEM verranno posizionati in corrispondenza della parte che modificano, visualizzando un badge con foto del proponente, nominativo e numero di EM/SUBEM. Cliccando sul badge si potrà visualizzare il testo dell’EM.

![PEM_7](/Documentazione/Screenshot/PEM_7.jpg)

Gli utenti del Servizio Segreteria dell’Assemblea hanno anche a disposizione un set di report che permettono di visualizzare/esportare l’elenco (in vari formati) e gli emendamenti depositati in base al pdl di riferimento, alla seduta, allo stato (approvato/respinto/ritirato), al firmatario (primo/successivo), all’articolo, ecc…


## Segreteria dei diversi gruppi consiliari

Gli utenti della segreteria dei diversi gruppi consiliari si possono collegare al sistema utilizzando le proprie credenziali di rete.

Inseriscono gli EM/SUBEM in bozza e li visualizzano in un’apposita GrigliadiLavoro, adeguatamente separati e raggruppati per PDL di riferimento. Affinché un EM/SUBEM possa essere accettato dal sistema (in bozza) devono essere compilate le informazioni obbligatorie: 
- PDL di riferimento, parte (titolo, articolo, comma, lettera, ecc…) a cui si riferisce l’EM/SUBEM;
- tipo di EM/SUBEM (soppressivo, modificativo, aggiuntivo);
- testo dell’EM/SUBEM. Indicano poi, obbligatoriamente, in fase di predisposizione dell’EM/SUBEM, il consigliere titolare dell’iniziativa dell’atto. Tale consigliere verrà identificato dal sistema come “primo firmatario” dell’EM/SUBEM.

Grazie alla predisposizione del Progetto di Legge codificato in base alle singole parti con i relativi testi (Articolo, Comma, Lettera) la piattaforma permette di creare emendamenti posizionandosi con il mouse direttamente sulla parte del PDL visualizzato “in anteprima” a video. Questo processo visuale e assistito consente di ridurre al minimo gli errori sulle parti del PDL che si intende emendare nonché evitare l’utilizzo di versioni errate (modificate) del PDL e il possibile inserimento di emendamenti duplicati.
Cliccando sulla parte da emendare il sistema predisporrà in automatico il format per l’inserimento dell’emendamento.
Dopo aver inserito un emendamento/subemendamento in bozza si ha sempre la possibilità di gestirlo attraverso un’apposita GrigliadiLavoro dedicata al gruppo politico:

![PEM_22](/Documentazione/Screenshot/PEM_22.jpg)

Attraverso questa griglia, gli utenti della segreteria dei diversi gruppi consiliari possono visualizzare tutti gli EM/SUBEM depositati da parte del proprio gruppo politico e modificare gli EM/SUBEM ancora in “stato di bozza” e non firmati.
Hanno facoltà di “alertare”, tramite messaggio email preconfigurato e contenente il link diretto a uno specifico EM/SUBEM, un consigliere appartenente al proprio gruppo alla firma di un EM/SUBEM in bozza e che deve essere depositato.

Hanno inoltre a disposizione un set di report che permettono di visualizzare/esportare l’elenco (in vari formati) e gli emendamenti depositati in base al PDL di riferimento, alla seduta, allo stato (approvato/respinto/ritirato), al firmatario (primo/successivo), all’articolo, ecc…

Su richiesta formale da parte del Capogruppo, la segreteria del gruppo consiliare può essere autorizzata ad effettuare l’operazione di deposito (come dettagliato successivamente per il [Responsabile di segreteria dei diversi gruppi consiliari](#responsabile-di-segreteria-dei-diversi-gruppi-consiliari)) per gli EM/SUBEM formulati dai consiglieri del proprio gruppo. In questo caso verrà creato, attraverso una specifica funzionalità del sistema, un PIN dispositivo (valido per il deposito). L’operazione potrà essere effettuata dall’interessato in maniera autonoma e obbligatoriamente quando il PIN viene reimpostato dall’amministratore del sistema.


## Responsabile di segreteria dei diversi gruppi consiliari

Il responsabile di segreteria dei diversi gruppi consiliari si può collegare al sistema utilizzando le proprie credenziali di rete.

Inserisce gli EM/SUBEM in bozza e li visualizza adeguatamente separati e raggruppati per PDL di riferimento con modalità del tutto analoghe a quanto previsto per gli utenti della segreteria dei diversi gruppi consiliari.

Anche il responsabile di segreteria può “sollecitare”, tramite messaggio email preconfigurato e contenente il link diretto a un EM/SUBEM, un consigliere appartenente al proprio gruppo alla firma di un EM/SUBEM in bozza e che deve essere depositato.

Soprattutto può eseguire il deposito degli EM/SUBEM: attraverso una password dispositiva (codice PIN) può effettuare l’operazione di deposito degli EM/SUBEM, solo quelli firmati dal consigliere titolare dell’iniziativa (primo firmatario). Al momento del deposito, il documento EM/SUBEM viene numerato e marcato temporalmente dal sistema. Il numero e il testo dell’EM/SUBEM, la data, l’ora e le firme diverranno immodificabili: un algoritmo crittografico infatti genera un codice di controllo dipendente dalle suddette informazioni. Nessun controllo temporale verrà effettuato dal sistema che pertanto accetta in qualunque momento emendamenti sui PDL attivi.
Affinché un emendamento possa essere depositato devono essere compilate le TUTTE informazioni obbligatorie e deve essere presente almeno la firma del consigliere proponente utilizzando il proprio PIN.
NOTA: il deposito di un EM/SUBEM può essere effettuato direttamente dal consigliere, se primo firmatario, ovvero dal responsabile di segreteria del gruppo per gli EM/SUBEM per i quali il primo firmatario appartiene a quello specifico gruppo.

Il responsabile di segreteria può visualizzare gli EM/SUBEM depositati relativi al proprio gruppo ma anche quelli presentati con consiglieri di altro gruppo, previo invito.
Per questo può accedere all’AREA INVITI contenente l’elenco dei EM/SUBEM per i quali un consigliere del proprio gruppo politico ha ricevuto o effettuato un invito a firmare.

![PEM_25](/Documentazione/Screenshot/PEM_25.jpg)

Come la sua segreteria, il responsabile ha a disposizione un set di report che gli permettono di visualizzare/esportare l’elenco (in vari formati) e gli emendamenti depositati in base al PDL di riferimento, alla seduta, allo stato (approvato/respinto/ritirato), al firmatario (primo/successivo), all’articolo, ecc…

Attraverso una specifica funzionalità del sistema, può effettuare il cambio del PIN in maniera autonoma e obbligatoriamente quando il PIN viene reimpostato dall’amministratore del sistema.


## Consiglieri regionali

Il consigliere regionale si può collegare al sistema utilizzando le proprie credenziali di rete.

Inserisce gli EM/SUBEM in bozza e li visualizza nell'area condivisa con tutto il suo gruppo politico. Il sistema individua automaticamente il consigliere che inserisce l’EM/SUBEM e lo identifica come “primo firmatario” (titolare dell’iniziativa).

Il consigliere regionale firma, utilizzando il proprio “PIN dispositivo”, i propri EM/SUBEM e quelli presenti a sistema inseriti da un appartenente al proprio gruppo politico. Affinché un emendamento possa essere firmato devono essere compilate le seguenti informazioni obbligatorie: 
- PDL di riferimento, parte (titolo, articolo, comma, lettera, ecc…) a cui si riferisce l’EM/SUBEM;
- tipo di EM/SUBEM (soppressivo, interamente sostitutivo, modificativo, aggiuntivo);
- testo dell’EM/SUBEM;
- indicazione se l’EM/SUBEM ha effetti finanziari.

![PEM_9](/Documentazione/Screenshot/PEM_9.jpg)

Dopo la prima firma, il testo dell’EM/SUBEM viene crittografato e non può più essere modificato.

Il consigliere regionale può firmare, fino alla votazione, un EM/SUBEM afferente al proprio gruppo politico o per il quale ha ricevuto un “invito” da parte del primo firmatario dell’atto (quindi prima del deposito, previo invito, anche se l’EM/SUBEM è di un altro gruppo politico). La firma apposta successivamente al deposito è riconoscibile, visualizzata diversamente, rispetto alle firme effettuate prima del deposito.

![PEM_10](/Documentazione/Screenshot/PEM_10.jpg)

Il consigliere regionale può anche rimuovere la propria firma da un EM/SUBEM rispettando i seguenti vincoli:

A. Se l’EM/SUBEM è ancora in bozza (non depositato):
- se è il primo firmatario, l’EM/SUBEM viene eliminato permanentemente dal sistema;
- altrimenti viene eliminata la firma e non ne rimane traccia visibile sull’EM;

B. Se l’EM/SUBEM è già stato depositato:
- se è il primo firmatario la firma non viene “rimossa” ma risulta il ritiro: la firma è visualizzata barrata con sotto la data/ora del ritiro. Il secondo firmatario, in ordine cronologico, viene considerato come primo firmatario (ai soli fini amministrativi);
- altrimenti risulta il solo ritiro della firma: la firma è visualizzata barrata con sotto la data/ora del ritiro.

C. Se tutti i firmatari ritirano la propria firma da un EM/SUBEM depositato, questo risulta ritirato. L’EM/SUBEM ritirato rimane in elenco con la dicitura “RITIRATO”. L’EM/SUBEM RITIRATO può essere fatto proprio, ma senza modifiche, da altri consiglieri attraverso l’apposizione della propria firma.

Se il consigliere regionale è il primo firmatario, può invitare alla firma del proprio EM/SUBEM non ancora depositato un consigliere appartenente ad un altro gruppo politico. L’invito viene inviato selezionando come destinatari uno/più consiglieri oppure un intero gruppo politico. L’operazione di invito viene notificata/visualizzata, oltre che ai singoli consiglieri invitati, anche ai Capogruppo (presidente del gruppo politico) e Responsabili di Segreteria dei gruppi sia del consigliere invitante sia dei consiglieri invitati. Il sistema consente di effettuare inviti multipli, in momenti differenti e fino al deposito dell’EM/SUBEM.

Il consigliere regionale può accedere all’AREA INVITI contenente:
- l’elenco degli EM/SUBEM per i quali ha ricevuto un invito a firmare (inviti ricevuti);
- l’elenco degli EM/SUBEM per i quali è primo firmatario e ha mandato un invito (inviti effettuati).

Se il consigliere è anche Capogruppo (Presidente del gruppo politico), nell’AREA INVITI visualizza tutti EM/SUBEM per i quali, un consigliere appartenente al proprio gruppo, ha ricevuto o effettuato un invito. La presenza di EM/SUBEM nella propria AREA INVITI è segnalata direttamente nel menù di accesso all’area attraverso un contatore – es. “Hai 3 inviti a Firmare”.

Il consigliere regionale utilizzando il proprio “PIN dispositivo” può effettuare l’operazione di deposito degli EM/SUBEM di cui è primo firmatario. Al momento del deposito l’EM/SUBEM viene numerato e marcato temporalmente dal sistema in modo immodificabile. Nessun controllo temporale viene effettuato dal sistema che pertanto accetta in qualunque momento EM/SUBEM sui PDL attivi.

Dopo il termine ordinario e nel corso della seduta, il consigliere regionale può presentare EM/SUBEM collegandosi al sistema attraverso un pc o dispositivo mobile, inserendo/firmando/depositando i testi (art. 87 comma 3 – Regolamento Generale). La procedura utilizzata è quella ordinaria di inserimento, tenendo presente quanto segue:

- I Consiglieri/PresidenteGR/Assessori/Sottosegretari possono presentare solo SUBEMENDAMENTI ad EMENDAMENTI depositati da altri entro i termini stabiliti dall’art. 87 del Regolamento Generale.
- I relatori del PDL in discussione e i componenti della Giunta Regionale (Presidente G.R., Assessori, Sottosegretari) possono inserire ancora EMENDAMENTI fino alla votazione del provvedimento.
- Il sistema comunque non pone alcun blocco in relazione alle fattispecie sopra indicate, ma accetta sempre tutti gli EM/SUBEM presentati.
- L’inserimento di EM/SUBEM inseriti durante la discussione viene notificato, attraverso un messaggio email, agli operatori della Segreteria dell’Assemblea.

Il consigliere regionale può modificare, attraverso una specifica funzionalità del sistema, il PIN dispositivo (valido per firma e deposito) che sostituirà quello in uso. L’operazione può essere effettuata dall’interessato in maniera autonoma e obbligatoriamente quando il PIN viene reimpostato dall’amministratore del sistema.


## Assessori e sottosegretari regionali

Assessori e sottosegretari regionali si possono collegare al sistema utilizzando apposite credenziali.

Visualizzano la GrigliadiLavoro degli EM/SUBEM, relativi alla Giunta (la Giunta è assimilabile a un gruppo consiliare).
Inoltre un assessore/sottosegretario regionale che ricopre anche la carica di Consigliere regionale, può accedere anche all’area del Gruppo Politico di appartenenza e inserire, come consigliere, EM/SUBEM.

Assessori e sottosegretari regionali possono inserire gli EM/SUBEM in bozza e visualizzarli adeguatamente separati e raggruppati per PDL di riferimento. Affinché un EM/SUBEM possa essere accettato in bozza dal sistema devono essere compilate le informazioni obbligatorie.
Il sistema individua automaticamente il componente della Giunta che inserisce l’EM/SUBEM come “primo firmatario” (titolare dell’iniziativa).

Assessori e sottosegretari regionali firmano, utilizzando il proprio “PIN dispositivo”, i propri EM/SUBEM ed eventualmente gli EM/SUBEM inseriti da altro assessore. Dopo la prima firma, il testo dell’EM/SUBEM non può più essere modificato ed alla firma del titolare dell’iniziativa (primo firmatario) l’EM/SUBEM verrà crittografato.
Affinché un emendamento possa essere firmato devono essere compilate le seguenti informazioni obbligatorie: 
- PDL di riferimento, parte (titolo, articolo, comma, lettera, ecc…) a cui si riferisce l’EM/SUBEM;
- tipo di EM/SUBEM (soppressivo, interamente sostitutivo, modificativo, aggiuntivo);
- testo dell’EM/SUBEM;
- indicazione se l’EM/SUBEM ha effetti finanziari (nel caso di EM/SUBEM di Giunta occorre anche allegare la relazione tecnico-finanziaria).
Se l’Assessore regionale è anche un Consigliere, al momento della firma, può decidere se far risultare l’EM/SUBEM firmato come Consigliere o come Assessore (in questo caso, insieme alla firma, dovrà essere visualizzata anche la carica).

Assessori e sottosegretari regionali possono rimuovere la firma dai propri EM/SUBEM rispettando i seguenti vincoli:

A. Se l’EM/SUBEM è ancora in bozza (non depositato):
- se è il primo firmatario l’EM/SUBEM viene eliminato permanentemente dal sistema;
- altrimenti viene eliminata la firma e non ne rimane traccia visibile sull’EM.

B. Se l’EM/SUBEM è già stato depositato:
- se è il primo firmatario la firma non viene “rimossa” ma risulta il ritiro: la firma è visualizzata barrata con sotto la data/ora del ritiro. L’EM/SUBEM RITIRATO può essere fatto proprio, ma senza modifiche, da altri consiglieri attraverso l’apposizione della propria firma.
- altrimenti risulta il solo ritiro della firma: la firma è visualizzata barrata con sotto la data/ora del ritiro.

Utilizzando il proprio “PIN dispositivo”, gli Assessori e sottosegretari regionali possono effettuare l’operazione di deposito degli EM/SUBEM di cui sono primi firmatari. La firma e il deposito di EM/SUBEM di Giunta vengono notificate, via email, ad un numero configurabile di contatti.

Dopo il termine ordinario e nel corso della seduta, gli Assessori e sottosegretari regionali possono presentare EM/SUBEM collegandosi al sistema attraverso un pc o dispositivo mobile, inserendo/firmando/depositando i testi (art. 87 comma 3 – Regolamento Generale).
Possono modificare, attraverso una specifica funzionalità del sistema, il PIN dispositivo (valido per firma e deposito) che sostituirà quello in uso. L’operazione può essere effettuata dall’interessato in maniera autonoma e obbligatoriamente quando il PIN viene reimpostato dall’amministratore del sistema.
Possono inoltre modificare autonomamente la propria password di accesso (password di rete).


## Presidente della Giunta Regionale

Il Presidente della Giunta Regionale si può collegare al sistema utilizzando apposite username e password personali.
Il suo ruolo ha tutte le funzionalità previste per gli assessori/sottosegretari (si veda sezione precedente).
Inoltre può ritirare qualunque EM/SUBEM depositato di cui il primo firmatario è un assessore/sottosegretario.

## Segreterie di presidente/assessori/sottosegretari

Gli utenti delle segreterie di presidente/assessori/sottosegretari si possono collegare al sistema utilizzando apposite username e password personali.

Inseriscono gli EM/SUBEM in bozza e li visualizzano adeguatamente separati e raggruppati per PDL di riferimento. Affinché un EM/SUBEM possa essere accettato dal sistema in bozza dovranno essere compilate le informazioni, indicando, obbligatoriamente, in fase di predisposizione dell’EM/SUBEM, il titolare dell’iniziativa (“primo firmatario”).
Possono modificare gli EM/SUBEM ancora in “stato di bozza” e non firmati.

Inoltre hanno a disposizione un set di report che permettono di visualizzare/esportare l’elenco (in vari formati) degli emendamenti depositati in base al PDL di riferimento, alla seduta, allo stato (approvato/respinto/ritirato), al firmatario (primo/successivo), all’articolo, ecc…

Gli utenti delle segreterie di presidente/assessori/sottosegretari possono “allertare”, tramite messaggio email preconfigurato e contenente il link diretto a un EM/SUBEM, il Presidente o il proprio Assessore/Sottosegretario di riferimento alla firma di un EM/SUBEM in bozza e che deve essere depositato.
Possono modificare autonomamente, attraverso una specifica funzionalità del sistema, la propria password di accesso (password di rete).

## Responsabili di segreteria di presidente/assessori/sottosegretari

I responsabili di segreteria di presidente/assessori/sottosegretari si possono collegare al sistema utilizzando apposite username e password personali.

Possono inserire gli EM/SUBEM in bozza, compilando le informazioni obbligatorie e indicando, in fase di predisposizione dell’EM/SUBEM, il titolare dell’iniziativa (“primo firmatario”).

Inoltre hanno a disposizione un set di report che permettono di visualizzare/esportare l’elenco (in vari formati) degli emendamenti depositati in base al PDL di riferimento, alla seduta, allo stato (approvato/respinto/ritirato), al firmatario (primo/successivo), all’articolo, ecc…

I responsabili di segreteria di presidente/assessori/sottosegretari possono “allertare”, tramite messaggio email preconfigurato e contenente il link diretto a un EM/SUBEM, il Presidente o il proprio Assessore/Sottosegretario di riferimento alla firma di un EM/SUBEM in bozza e che deve essere depositato.

Attraverso una password dispositiva (codice PIN – uno per ogni segreteria) possono effettuare l’operazione di deposito degli EM/SUBEM con almeno un firmatario. Il deposito di EM/SUBEM di Giunta viene poi notificato, via email, ad un numero configurabile di contatti.
NOTA: il deposito di un EM/SUBEM può essere effettuato direttamente dal presidente/assessore/sottosegretario primo firmatario ovvero da un qualsiasi responsabile di segreteria per gli EM/SUBEM di Giunta.

I responsabili di segreteria di presidente/assessori/sottosegretari possono modificare, attraverso una specifica funzionalità del sistema, il proprio PIN dispositivo (valido per deposito) che sostituirà quello in uso. L’operazione può essere effettuata dall’interessato in maniera autonoma e obbligatoriamente quando il PIN viene reimpostato dall’amministratore del sistema.
Possono anche modificare autonomamente la propria password di accesso (password di rete).

Inoltre, hanno a disposizione un set di report che permettono di visualizzare/esportare l’elenco (in vari formati) degli emendamenti depositati in base al PDL di riferimento, alla seduta, allo stato (approvato/respinto/ritirato), al firmatario (primo/successivo), all’articolo, ecc… 

## Amministratori di Giunta

Gli Amministratori di Giunta gestiscono gli utenti “GIUNTA” del sistema e possono eseguire le operazioni di:
- ricerca di utenze;
- creazione/disabilitazione di utenze: al nuovo utente dovrà essere richiesto il cambio password al primo accesso;
- reset delle password operative gestite tramite Active directory.

## Allegati

### Allegato 1. Output di un emendamento generato dal sistema

![Allegato 1](/Documentazione/Screenshot/Allegato_1.jpg)


# Modulo DASI – Presentazione Atti di indirizzo e di sindacato ispettivo
Gli atti di indirizzo e di sindacato ispettivo sono atti, tipici della tradizione parlamentare, attraverso i quali il Consiglio esercita la funzione di controllo sull’esecutivo e concorre alla determinazione dell’indirizzo politico (articolo 14, comma 1, dello Statuto d’autonomia).

Gli strumenti di indirizzo politico, denominati ATTI DI INDIRIZZO, previsti dal Regolamento generale sono mozioni, ordini del giorno e risoluzioni.
La funzione di controllo invece, come definita a livello parlamentare, si estrinseca (anche) nell’attività di sindacato ispettivo (ATTI DI SINDACATO ISPETTIVO), e viene tradizionalmente esercitata attraverso gli strumenti tipici dell’interpellanza (ITL), dell’interrogazione (ITR) e dell’interrogazione a risposta immediata (IQT).

- [Introduzione Modulo DASI](#Introduzione-Modulo-DASI)
- [Modalità di digitalizzazione degli Atti](#Modalità-di-digitalizzazione-degli-Atti)
- [Caratteristiche peculiari delle diverse tipologie degli atti](#Caratteristiche-peculiari-delle-diverse-tipologie-degli-atti)
  - [Atti di sindacato ispettivo](#Atti-di-sindacato-ispettivo):
    - [Interrogazione (ITR)](#Interrogazione-(ITR))
    - [Interrogazione a Risposta Immediata (IQT)](#Interrogazione-a-Risposta-Immediata)
    - [Interpellanza (ITL)](#Interpellanza-(ITL))
  - [Atti di indirizzo](#Atti-di-indirizzo):
    - [Mozione (MOZ)](#Mozione-(MOZ))
    - [Ordine del Giorno (OdG)](#Ordine-del-Giorno-(OdG))
    
## Introduzione Modulo DASI 
L'obiettivo del Portale DASI è mettere a disposizione strumenti e procedure atte a consentire ai Consiglieri la presentazione in forma dematerializzata degli atti di sindacato ispettivo e di indirizzo. 
Il modulo DASI è in grado di dialogare con le piattaforme e le banche dati che gestiscono, con modalità̀ diverse e con finalità̀ diverse, le varie operazioni relative al successivo iter di ciascun atto.

Il modulo DASI permette di: 
- gestire da remoto la predisposizione dei testi degli ATTI, la firma e l’operazione di deposito;
- garantire autenticità del testo, intesa come integrità del testo, provenienza del documento dal suo autore e certezza temporale sulla sua presentazione;
- snellire le operazioni per la lavorazione degli ATTI (numerazione, marcatura, autenticazione, ecc…);
- ridurre l’utilizzo della carta e dei supporti per la stampa.

## Modalità di digitalizzazione degli Atti
Gli ATTI DI INDIRIZZO E DI SINDACATO ISPETTIVO sono atti attraverso i quali il Consiglio esercita la funzione e controllo sull’esecutivo e concorre alla determinazione dell’indirizzo politico (articolo 14, comma 1, dello Statuto d’autonomia)

Gli strumenti di indirizzo politico, denominati ATTI DI INDIRIZZO, previsti dal Regolamento generale sono:

- Per il regolamento generale del consiglio regionale della Lombardia, la Mozione (MOZ), a sua volta si distingue in:
  - Mozione di sfiducia 
  - Mozione di censura 
  - Mozione abbinata 
  - Mozione urgente 
  
- L’ordine del Giorno (OdG) non è strumento autonomo, ma accessorio ad altro atto, in genere un progetto di legge; è anche lo strumento tipico con cui l’Aula conclude i dibattiti su un determinato argomento.

La funzione di controllo si estrinseca nell’attività̀ di sindacato ispettivo (ATTI DI SINDACATO ISPETTIVO), e viene esercitata attraverso:
- l’Interpellanza (ITL) 
- l’Interrogazione (ITR) 
- l’Interrogazione a risposta immediata (IQT)

Ognuno di questi atti è soggetto a un distinto iter rispetto agli emendamenti ai progetti di legge, ma hanno in comune le medesime regole quanto a modalità di inserimento e presentazione.

Ogni ATTO sarà redatto secondo una precisa struttura determinata dal tipo di atto, che conterrà alcuni dati identificativi. Una volta che l’ATTO è stato redatto verrà salvato a sistema come bozza (riconoscibile dalla dicitura TEMP seguito da un numero progressivo datole dal sistema). 

Il consigliere proponente potrà decidere di firmare, modificare, eliminare l’ATTO o lasciarlo in bozza e successivamente svolgere una delle operazioni previste per lo specifico atto (deposito, proposta di iscrizione in seduta, raccolta firme, abbinamento, ecc). 

Al fine di garantire la validità dell’ATTO, il Consigliere proponente lo dovrà firmare. La firma elettronica, gestita nella piattaforma attraverso l’utilizzo di un PIN scelto dal firmatario, ha lo scopo di garantire la certezza della provenienza del documento crittografando il testo attraverso il PIN in possesso esclusivo del singolo consigliere. Ogni consigliere utilizzerà per la firma degli ATTI il medesimo PIN usato per firmare gli emendamenti. 

Tramite il sistema degli inviti il consigliere potrà eventualmente invitare altri consiglieri – del proprio gruppo o di gruppi diversi – a firmare il proprio atto. Questa fase è particolarmente importante nei casi in cui il Regolamento stabilisce un numero minimo di firme per il tipo di ATTO, ai fini della sua presentazione.

Le modalità e il numero delle firme necessari per la presentazione delle diverse tipologie di ATTI sono attualmente impostate sulla base del regolamento generale del consiglio regionale della Lombardia. Tuttavia, il sistema permette di definire, attraverso opportuni parametri di configurazione, le regole da applicare a ciascuna tipologia di ATTO. 
Sarà sempre possibile il ritiro della propria firma secondo quando definito dal regolamento generale del consiglio regionale della Lombardia. 

Dopo la firma, il processo di presentazione in forma dematerializzata si completerà con il deposito dell’ATTO (INVIO), equivalente digitale della consegna a mano dell’originale al Servizio Segreteria dell’Assemblea consiliare, mediante il pulsante “deposita”.

Con il deposito, il testo dell’ATTO diventerà intangibile. Infatti, a seguito del deposito dell’ATTO, la piattaforma procederà̀ in modo automatico alle operazioni correlate alla:
- marcatura temporale
- numerazione progressiva per TIPO ATTO 
- avvisi informatici equivalenti al rilascio di ricevuta di presentazione
- intangibilità̀ dell’atto rispetto a determinati elementi: testo dell’ATTO, firma, marcatura temporale, numerazione.

## Caratteristiche peculiari delle diverse tipologie degli atti

### Atti di sindacato ispettivo 

Gli atti di sindacato ispettivo costituiscono uno strumento specifico di vigilanza sull’attività della Giunta regionale e si distinguono in Interrogazioni, Interrogazioni a risposta immediata e Interpellanze.
I consiglieri e i collaboratori dei gruppi accederanno alla loro area personale inserendo le credenziali già usate per il modulo PEM. 

Una volta fatto il login si verrà reindirizzati nella schermata principale del portale PEM- DASI. 
Per la redazione delle varie tipologie di ATTI è necessario entrare nel Modulo DASI. 
Apparirà la schermata riepilogativa degli ATTI. Mediante il pulsante + in basso a destra apparirà un menù a scomparsa che permetterà di selezionare il tipo di atto che si vuole redigere. 

![PEM_12](/Documentazione/Screenshot/PEM_12.jpg)

### Interrogazione (ITR)
I consiglieri regionali presentano le Interrogazioni (d’ora in avanti ITR) al Presidente del Consiglio regionale per il tramite del Servizio Segreteria dell’Assemblea. Non vi sono vincoli temporali per la loro presentazione.

L’interrogazione consiste nella domanda scritta, anche non motivata, rivolta da uno o più consiglieri regionali al Presidente della Regione o alla Giunta per avere informazioni circa la sussistenza o la verità di un fatto determinato o intorno a deliberazioni o atti adottati.

Mediante una GrigliadiLavoro, riservata a ogni gruppo politico, i consiglieri e il personale autorizzato delle segreterie del gruppo avranno a disposizione l’elenco dei propri atti e la possibilità di inserire nuove ITR compilando attraverso un apposito format i seguenti campi obbligatori:
- Tipo risposta richiesta 
- Oggetto dell’interrogazione 
- Premesse dell’ITR
- Soggetto interrogato: il format di inserimento prevede l’elenco dei soggetti a cui rivolgere l’interrogazione 
- Testo dell’ATTO (Richieste)
- Firma: il Consigliere proponente firma l’interrogazione con le proprie credenziali e PIN ed è identificato come “primo firmatario” dal sistema 
- Allegati: è possibile allegare all’interrogazione anche la documentazione che si ritiene opportuna 

![PEM_13](/Documentazione/Screenshot/PEM_13.jpg)

Al termine della compilazione di tutti i campi l’Interrogazione è salvata a sistema come bozza e rinominata come ITR TEMP e a seguire un numero progressivo datole dal sistema. 
Il consigliere può scegliere di firmare, modificare o eliminare la ITR in bozza. Dopo la firma può invitare altri consiglieri, del proprio gruppo o di altri gruppi, a sottoscrivere l’ITR e infine depositarla, sempre utilizzando il proprio PIN dispositivo.

Una volta che la ITR verrà depositata, la piattaforma restituirà al presentatore la conferma di avvenuto deposito, con indicazione del numero dell’atto: un progressivo che in Consiglio regionale della Lombardia segue una codifica specifica (configurabile nel sistema).

### Interrogazione a risposta immediata (IQT) 
L’ interrogazione a risposta immediata (d’ora in avanti IQT) consiste in un’unica domanda, formulata in modo chiaro e conciso, su un fatto connotato da particolare urgenza o attualità politica, rientrante nell’ambito delle competenze del Presidente della Regione o della Giunta regionale. 

In Consiglio regionale della Lombardia le IQT vengono trattate una volta al mese nella seduta dedicata alla trattazione degli atti di sindacato e di indirizzo. 
Le IQT vengono presentate compilando il format dedicato accedendo alla sezione dedicata alle interrogazioni a risposta immediata. La risposta a queste interrogazioni è orale in aula. 

Seguendo quanto sopra descritto per la presentazione delle ITR, il consigliere presentatore della IQT accederà alla sua area riservata e con modalità del tutto simili a quelle descritte per le ITR compilerà il format per l’inserimento delle IQT con i seguenti campi obbligatori:
- Tipo risposta: è un campo già compilato con “Immediata” 
- Oggetto dell’IQT
- Premesse dell’IQT 
- Soggetto interrogato: possono essere solo il Presidente di Regione, la Giunta, il singolo assessore o più assessori. Il presentatore della IQT può porre un flag su quale tra i soggetti sopra indicati è rivolta l’interrogazione. 
- Testo dell’atto (Richieste): la compilazione del testo prevede l’inserimento di una sola domanda e l’uso limitato di parole e/o caratteri.  
- Firma: Le modalità e il numero delle firme necessari per la presentazione delle diverse tipologie di ATTI sono attualmente impostate sulla base del regolamento generale del consiglio regionale della Lombardia. Tuttavia, il sistema permette di definire, attraverso opportuni parametri di configurazione, le regole da applicare a ciascuna tipologia di ATTO.
- Allegati: è possibile allegare all’interrogazione anche la documentazione che si ritiene opportuna

![PEM_14](/Documentazione/Screenshot/PEM_14.jpg)

Come le ITR, anche la IQT, una volta redatta, è salvata a sistema come bozza e rinominata come IQT TEMP e a seguire un numero progressivo datole dal sistema.

Le IQT vengono presentate unicamente per la trattazione in una seduta. La presentazione di una IQT collegata a una seduta, non comporta l’iscrizione automatica, in quanto l’ordine del giorno della seduta viene deciso dal Presidente del Consiglio sulla base della verifica di ammissibilità.

Le IQT, predisposte in bozza, possono essere depositate solo se è presente a sistema una seduta d’Aula in cui è prevista la trattazione degli ATTI (una spunta selezionabile dalla segreteria del Servizio Assemblea indicherà questa possibilità).
L’iscrizione dell’atto a una seduta prevede solo il collegamento dell’atto ad una seduta specifica ma non dovrà comportare il cambio di stato dell’atto che rimarrà “depositato”.

![PEM_15](/Documentazione/Screenshot/PEM_15.jpg)

Il consigliere proponente la IQT cliccherà sul pulsante a forma di ingranaggio e apparirà la dicitura “proponi iscrizione”. Selezionandola, apparirà una finestra dove verrà proposta dal sistema la seduta d’Aula codificata a sistema dal personale della Segreteria dell’Assemblea consiliare.

![PEM_16](/Documentazione/Screenshot/PEM_16.jpg)

Dopo aver selezionato la seduta in cui trattare l’atto, si potrà procedere al deposito. Al momento del deposito, il sistema effettuerà il conteggio delle firme secondo le regole previste dal Consiglio regionale della Lombardia e nel caso tali regole non siano state rispettate, avvisa il consigliere e non procede all’operazione di deposito.

Una volta che l’IQT verrà depositata, la piattaforma restituirà al presentatore la conferma di avvenuto deposito, con indicazione del numero dell’atto: un progressivo che in Consiglio regionale della Lombardia segue una codifica specifica (configurabile nel sistema).

Contestualmente al deposito, la piattaforma avviserà tramite messaggio e-mail il Servizio segreteria assemblea della IQT depositata.

L’aggiunta di firme per le IQT è consentita fino a quando lo stato dell’atto è “presentato”, cioè fino all’annunzio in aula.

### Interpellanza (ITL)
L’interpellanza (d’ora in avanti ITL) consiste nella domanda rivolta da uno o più consiglieri regionali al Presidente della Regione o alla Giunta per conoscere i motivi o gli intendimenti della loro condotta in particolari circostanze; è lo strumento specifico per interpellare la Giunta su temi collegati all’indirizzo politico e sulle ragioni delle politiche adottate.

Le interpellanze si distinguono in base alla tipologia di risposta richiesta: 
- scritta, 
- orale in commissione, 
- orale in Assemblea. 

Il tipo di risposta condiziona la procedura e la numerazione (in Consiglio regionale le ITL seguono una particolare codifica a seconda del tipo di risposta richiesto).

![PEM_17](/Documentazione/Screenshot/PEM_17.jpg)

Per la risposta in commissione il proponente deve indicare anche la commissione (permanente o speciale) a cui indirizzare il quesito. ll format di inserimento prevede l’elenco delle commissioni tra cui scegliere. 

![PEM_18](/Documentazione/Screenshot/PEM_18.jpg)

Il consigliere presentatore della ITL accederà alla sua area riservata, dove avrà a disposizione l’elenco delle ITL inserite a sistema e riferite al proprio gruppo, e con modalità del tutto simili a quelle descritte per gli altri Atti, compilerà il format per l’inserimento delle ITL con i seguenti campi obbligatori:
-	Tipo di risposta richiesta: L’indicazione del tipo di risposta è un campo obbligatorio 
-	Oggetto dell’interpellanza
-	Premesse dell’interpellanza
-	Soggetto interrogato: si vedano le medesime disposizioni indicate per l’ITR e l’IQT. Il consigliere presentatore viene inoltre guidato nel drafting dell’atto fleggando una delle seguenti opzioni: chiede – invita – impegna
-	Testo dell’interpellanza (Richieste)
-	Firma : si vedano le medesime disposizioni indicate per l’ITR
-	Allegati: è possibile allegare all’interrogazione anche la documentazione che si ritiene opportuna 

Il processo di presentazione delle ITL si concluderà con la firma dell’atto e il suo deposito (come descritto per le ITR e l’IQT). L’unica firma necessaria per procedere al deposito dell’interpellanza è quella del consigliere proponente dell’atto.

### Atti di indirizzo 
Gli strumenti di indirizzo politico previsti dal Regolamento generale sono quelli tipici delle assemblee legislative: Mozioni e Ordini del giorno. Sebbene questi atti abbiano tutti identica natura di atti di indirizzo politico, vanno rilevati alcuni punti di differenziazione per quanto attiene ai profili procedurali.

### Mozione (MOZ)
La Mozione (d’ora in avanti MOZ) consiste in un documento motivato e sottoscritto da uno o più consiglieri volto a promuovere una deliberazione consiliare.

I consiglieri regionali presentano le mozioni al Presidente del Consiglio regionale per il tramite del servizio Segreteria dell’Assemblea.

Il consigliere presentatore della MOZ accederà alla sua area riservata e potrà inserire nuove mozioni a sistema attraverso il format dedicato, contenente i seguenti campi obbligatori:
- Tipo mozione: L’indicazione della tipologia è un campo obbligatorio 
- Oggetto della mozione
- Premesse della mozione
- Soggetti Interessati: si vedano le medesime disposizioni indicate per gli altri ATTI. Il consigliere presentatore viene inoltre guidato nel drafting dell’atto fleggando una delle seguenti opzioni: chiede – invita – impegna
- Testo dell’atto (Richieste)
- Firma della mozione: il Consigliere proponente firma la Mozione con le proprie credenziali ed è identificato come “primo firmatario” dal sistema. Le modalità e il numero delle firme necessari per la presentazione delle diverse tipologie di mozioni sono attualmente impostate sulla base del Regolamento Generale del Consiglio Regionale della Lombardia. Tuttavia, il sistema permette di definire, attraverso opportuni parametri di configurazione, le regole da applicare a ciascuna tipologia di ATTO. Il sistema controlla il numero di sottoscrittori necessari per la presentazione (deposito) e nel caso le regole non siano state rispettate, avvisa il presentatore e non procede all’operazione di deposito. Come prescritto dal Regolamento Generale del consiglio regionale lombardo le mozioni, per poter essere depositate, richiedono un numero minimo di firme definito in base alla loro tipologia:
  - La mozione urgente, o richiesta di trattazione urgente per una mozione, può essere effettuata dal presidente del gruppo solo se una mozione è firmata da almeno 8 consiglieri (anche appartenenti a gruppi diversi). Ogni consigliere può comparire una sola volta come uno degli otto sottoscrittori di una mozione urgente, per la medesima seduta. Qualora una mozione sia stata sottoscritta da tutti i capigruppo presenti in Consiglio, non si applicano limiti al numero di mozioni che possono essere richieste per la trattazione urgente nella medesima seduta. 
  - La mozione di sfiducia al Presidente della Regione deve essere presentata da almeno un quinto dei componenti del Consiglio regionale. 
  - La mozione di censura nei confronti di un assessore deve essere presentata da almeno un quinto dei componenti del Consiglio regionale). 
  - Le mozioni abbinate (mozione abbinata ad altra mozione presentata) possono essere richieste dal presidente del gruppo con il limite massimo di una mozione abbinata per gruppo politico, nella medesima seduta.

![PEM_19](/Documentazione/Screenshot/PEM_19.jpg)

Il processo di presentazione delle MOZ si concluderà con la firma dell’atto e la sua presentazione (come descritto per i precedenti ATTI).

Al momento del deposito il sistema avvisa tramite messaggio e-mail il Servizio segreteria assemblea della tipologia di mozione depositata.

### Ordine del Giorno (OdG)
L’ordine del giorno (d’ora in avanti OdG) consiste in un documento motivato sottoscritto da uno o più consiglieri volto a promuovere una deliberazione del Consiglio regionale.
Gli ordini del giorno sono iscritti per la trattazione in aula collegati a un provvedimento o a un dibattito consiliare. 

Tramite la piattaforma accedendo alla tipologia OdG, il Consigliere proponente dell’OdG compila il format che prevede le seguenti caratteristiche:
- Argomento a cui abbinare l’ordine del giorno: può essere un Progetto di legge o un altro tipo di atto
- Oggetto dell’ordine del griono
- Premesse
- Soggetti interessati: si vedano le medesime disposizioni indicate per gli altri ATTI
- Testo dell’atto (Richieste)
- Tipo ordine del giorno: indicazione se l’ODG è di “non passaggio all’esame degli articoli”
- Firma dell’ordine del giorno: il consigliere proponente firma l’atto e viene identificato come primo firmatario. Altri consiglieri possono firmare come pure possono essere invitati consiglieri di gruppi differenti a firmare l’atto

![PEM_20](/Documentazione/Screenshot/PEM_20.jpg)

Il processo di presentazione dell’OdG si concluderà con la firma dell’atto e la sua presentazione (deposito).

Il sistema permetterà di iscrivere tutti gli OdG depositati, a prescindere dalla data di deposito, alla seduta d’Aula prescelta, selezionandola tra quelle disponibili e preventivamente codificate da parte della segreteria del Servizio Assemblea. Gli ordini del giorno possono essere presentati solo se è presente una seduta aperta (si veda la procedura descritta per le IQT).

Al momento del deposito il sistema avvisa tramite messaggio e-mail il Servizio segreteria assemblea dell’avvenuto depositato dell’OdG.

# Amministrazione del sistema

L'area amministrativa del sistema è comune ad entrambi moduli e consente di gestire gli utenti della piattaform e i relativi permessi, sfruttando un'integrazione con l'Active Directory di dominio.
Gli Amministratori del sistema accedono al portale con visibilità completa sugli atti inseriti e in qualsiasi stato si trovino.
Possono autenticarsi al sistema “impersonando” una qualsiasi tipologia d’utenza (Segreteria dell’Assemblea, Consigliere, Responsabile di Segreteria, Assessore, ecc…).
Possono gestire completamente gli utenti del sistema che possono accedere al modulo PEM e al modulo DASI.
L'area amministrativa mette a disposizione degli ads tutti gli strumenti per effettuare le operazioni di:
- ricerca di utenze;
- creazione/disabilitazione di utenze;
- modifica delle password operative;
- generazione, ma non visibilità, di nuovi PIN: il PIN generato viene notificato all’interessato, che dovrà modificarlo obbligatoriamente al primo accesso al sistema;
- assegnazione, utilizzando un’apposita funzionalità web d’interfacciamento con i gruppi Active directory, delle diverse visibilità sulle aree del sistema.

Gli amministratori del sistema PEM-DASI gestiscono inoltre tutti i parametri di configurazione del portale e sono gli unici a poter eseguire le operazioni per il cambio legislatura.

![PEM_11](/Documentazione/Screenshot/PEM_11.jpg)


## Dettagli della licenza
La documentazione di PEM-DASI è rilasciata con licenza Creative Commons Attribution-ShareAlike 4.0 International

Salvo diversamente indicato dalla legge applicabile o concordato per iscritto, la documentazione rilasciata secondo i termini della Licenza è distribuita "TAL QUALE", SENZA GARANZIE O CONDIZIONI DI ALCUN TIPO, esplicite o implicite.

Si veda la Licenza per la lingua specifica che disciplina le autorizzazioni e le limitazioni secondo i termini della Licenza.

Si veda il file LICENSE.md all'interno della cartella Documentazione del repository per i riferimenti completi.

Il logo della Regione Lombardia è di proprietà esclusiva di Regione Lombardia e per tanto non è rilasciato sotto licenza aperta.
