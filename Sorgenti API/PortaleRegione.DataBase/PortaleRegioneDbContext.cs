/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Data.Entity;
using System.Threading.Tasks;
using PortaleRegione.Domain;

namespace PortaleRegione.DataBase
{
    /// <summary>
    ///     Configurazione del contesto di accesso al database per il Portale Regione.
    ///     Questa classe facilita l'interazione con il database, permettendo operazioni CRUD sui set di dati.
    /// </summary>
    public class PortaleRegioneDbContext : DbContext
    {
        /// <summary>
        ///     Inizializza una nuova istanza del contesto del database con la stringa di connessione specificata.
        ///     Il lazy loading è disabilitato per migliorare le performance e prevenire il caricamento involontario di entità
        ///     correlate.
        /// </summary>
        public PortaleRegioneDbContext()
            : base("name=PortaleRegioneDbContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        // Di seguito, vengono definiti i set di dati per ogni tabella del database.
        // Ogni DbSet rappresenta una collezione di entità che mappa una specifica tabella nel database.

        public virtual DbSet<ARTICOLI> ARTICOLI { get; set; }
        public virtual DbSet<ATTI> ATTI { get; set; }
        public virtual DbSet<ATTI_RELATORI> ATTI_RELATORI { get; set; }
        public virtual DbSet<cariche> cariche { get; set; }
        public virtual DbSet<COMMI> COMMI { get; set; }
        public virtual DbSet<EM> EM { get; set; }
        public virtual DbSet<gruppi_politici> gruppi_politici { get; set; }
        public virtual DbSet<join_gruppi_politici_legislature> join_gruppi_politici_legislature { get; set; }
        public virtual DbSet<JOIN_GRUPPO_AD> JOIN_GRUPPO_AD { get; set; }
        public virtual DbSet<join_persona_AD> join_persona_AD { get; set; }
        public virtual DbSet<join_persona_assessorati> join_persona_assessorati { get; set; }
        public virtual DbSet<join_persona_gruppi_politici> join_persona_gruppi_politici { get; set; }
        public virtual DbSet<join_persona_organo_carica> join_persona_organo_carica { get; set; }
        public virtual DbSet<join_persona_recapiti> join_persona_recapiti { get; set; }
        public virtual DbSet<legislature> legislature { get; set; }
        public virtual DbSet<LETTERE> LETTERE { get; set; }
        public virtual DbSet<MISSIONI> MISSIONI { get; set; }
        public virtual DbSet<NOTIFICHE> NOTIFICHE { get; set; }
        public virtual DbSet<NOTIFICHE_DESTINATARI> NOTIFICHE_DESTINATARI { get; set; }
        public virtual DbSet<organi> organi { get; set; }
        public virtual DbSet<PARTI_TESTO> PARTI_TESTO { get; set; }
        public virtual DbSet<persona> persona { get; set; }
        public virtual DbSet<PINS> PINS { get; set; }
        public virtual DbSet<PINS_NoCons> PINS_NoCons { get; set; }
        public virtual DbSet<RUOLI> RUOLI { get; set; }
        public virtual DbSet<RUOLI_UTENTE> RUOLI_UTENTE { get; set; }
        public virtual DbSet<SEDUTE> SEDUTE { get; set; }
        public virtual DbSet<STAMPE> STAMPE { get; set; }
        public virtual DbSet<STAMPE_INFO> STAMPE_INFO { get; set; }
        public virtual DbSet<STATI_EM> STATI_EM { get; set; }
        public virtual DbSet<tbl_recapiti> tbl_recapiti { get; set; }
        public virtual DbSet<TIPI_ATTO> TIPI_ATTO { get; set; }
        public virtual DbSet<TIPI_EM> TIPI_EM { get; set; }
        public virtual DbSet<TIPI_NOTIFICA> TIPI_NOTIFICA { get; set; }
        public virtual DbSet<tipo_organo> tipo_organo { get; set; }
        public virtual DbSet<TITOLI_MISSIONI> TITOLI_MISSIONI { get; set; }
        public virtual DbSet<UTENTI_NoCons> UTENTI_NoCons { get; set; }
        public virtual DbSet<FIRME> FIRME { get; set; }
        public virtual DbSet<View_Composizione_GiuntaRegionale> View_Composizione_GiuntaRegionale { get; set; }
        public virtual DbSet<View_CONSIGLIERE_GRUPPO> View_CONSIGLIERE_GRUPPO { get; set; }
        public virtual DbSet<View_CONSIGLIERI_PEM> View_CONSIGLIERI_PEM { get; set; }
        public virtual DbSet<View_gruppi_politici_con_giunta> View_gruppi_politici_con_giunta { get; set; }
        public virtual DbSet<View_gruppi_politici_ws> View_gruppi_politici_ws { get; set; }
        public virtual DbSet<View_PINS> View_PINS { get; set; }
        public virtual DbSet<View_UTENTI> View_UTENTI { get; set; }
        public virtual DbSet<View_CAPIGRUPPO> View_CAPIGRUPPO { get; set; }
        public virtual DbSet<View_Conteggi_EM_Gruppi_Politici> View_Conteggi_EM_Gruppi_Politici { get; set; }
        public virtual DbSet<View_Conteggi_EM_Area_Politica> View_Conteggi_EM_Area_Politica { get; set; }
        public virtual DbSet<View_consiglieri_in_carica> View_consiglieri_in_carica { get; set; }
        public virtual DbSet<View_consiglieri_per_legislatura> View_consiglieri_per_legislatura { get; set; }
        public virtual DbSet<View_consiglieri> View_consiglieri { get; set; }
        public virtual DbSet<View_assessori_in_carica> View_assessori_in_carica { get; set; }
        public virtual DbSet<ATTI_DASI> DASI { get; set; } // DASI - Atti
        public virtual DbSet<ATTI_FIRME> ATTI_FIRME { get; set; } // DASI - Firme

        public virtual DbSet<ATTI_DASI_CONTATORI>
            DASI_CONTATORI { get; set; } // DASI - Contatori per tipo atto e tipo risposta

        public virtual DbSet<View_cariche_assessori_in_carica>
            View_cariche_assessori_in_carica { get; set; } // DASI - Soggetti interrogati

        public virtual DbSet<View_cariche_assessori_per_legislatura> View_cariche_assessori_per_legislatura
        {
            get;
            set;
        } // DASI - Soggetti interrogati per legislatura

        public virtual DbSet<ATTI_SOGGETTI_INTERROGATI>
            ATTI_SOGGETTI_INTERROGATI { get; set; } // DASI - Soggetti interrogati

        public virtual DbSet<View_Commissioni_attive> View_Commissioni_attive { get; set; } // DASI - Commissioni
        public virtual DbSet<View_Commissioni> View_Commissioni { get; set; } // DASI - Commissioni

        public virtual DbSet<View_Commissioni_per_legislatura>
            View_Commissioni_per_legislatura { get; set; } // DASI - Commissioni per legislatura

        public virtual DbSet<ATTI_COMMISSIONI> ATTI_COMMISSIONI { get; set; } // DASI - Commissioni risposta
        public virtual DbSet<ATTI_RISPOSTE> ATTI_RISPOSTE { get; set; } // DASI - Risposte
        public virtual DbSet<ATTI_MONITORAGGIO> ATTI_MONITORAGGIO { get; set; } // DASI - Monitoraggi
        public virtual DbSet<ATTI_DOCUMENTI> ATTI_DOCUMENTI { get; set; } // DASI - Documenti
        public virtual DbSet<ATTI_NOTE> ATTI_NOTE { get; set; } // DASI - Note
        public virtual DbSet<ATTI_PROPONENTI> ATTI_PROPONENTI { get; set; } // DASI - Commissioni proponenti
        public virtual DbSet<ATTI_ABBINAMENTI> ATTI_ABBINAMENTI { get; set; } // DASI - Abbinamenti
        public virtual DbSet<View_Atti> VIEW_ATTI { get; set; } // ATTI + ATTI_DASI
        public virtual DbSet<TAGS> TAGS { get; set; } // Elenco tags per emendamenti
        public virtual DbSet<FILTRI> FILTRI { get; set; } // Filtri personalizzati
        public virtual DbSet<REPORTS> REPORTS { get; set; } // Reports personalizzati
        public virtual DbSet<TEMPLATES> TEMPLATES { get; set; } // Templates

        /// <summary>
        ///     Override del metodo OnModelCreating per configurare i modelli di entità quando il modello per questo contesto viene
        ///     creato.
        ///     Qui possono essere configurate le mappature delle tabelle, le relazioni e le convenzioni.
        /// </summary>
        /// <param name="modelBuilder">Il costruttore del modello per il contesto di database.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ARTICOLI>()
                .Property(e => e.Articolo)
                .IsUnicode(false);

            modelBuilder.Entity<ARTICOLI>()
                .Property(e => e.TestoArticolo)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .Property(e => e.NAtto)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .Property(e => e.Oggetto)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .Property(e => e.Path_Testo_Atto)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .Property(e => e.LinkFascicoloPresentazione)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .Property(e => e.LinkFascicoloVotazione)
                .IsUnicode(false);

            modelBuilder.Entity<ATTI>()
                .HasMany(e => e.ARTICOLI)
                .WithRequired(e => e.ATTI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ATTI>()
                .HasMany(e => e.ATTI_RELATORI)
                .WithRequired(e => e.ATTI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ATTI>()
                .HasMany(e => e.COMMI)
                .WithRequired(e => e.ATTI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ATTI>()
                .HasMany(e => e.EM)
                .WithRequired(e => e.ATTI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ATTI>()
                .HasMany(e => e.NOTIFICHE)
                .WithRequired(e => e.ATTI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ATTI>()
                .HasMany(e => e.STAMPE)
                .WithRequired(e => e.ATTI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<cariche>()
                .Property(e => e.nome_carica)
                .IsUnicode(false);

            modelBuilder.Entity<cariche>()
                .Property(e => e.tipologia)
                .IsUnicode(false);

            modelBuilder.Entity<cariche>()
                .HasMany(e => e.join_persona_organo_carica)
                .WithRequired(e => e.cariche)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<COMMI>()
                .Property(e => e.Comma)
                .IsUnicode(false);

            modelBuilder.Entity<COMMI>()
                .Property(e => e.TestoComma)
                .IsUnicode(false);

            modelBuilder.Entity<LETTERE>()
                .Property(e => e.Lettera)
                .IsUnicode(false);

            modelBuilder.Entity<LETTERE>()
                .Property(e => e.TestoLettera)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.N_EM)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.N_SUBEM)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.DataDeposito)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.Hash)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.NTitolo)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.NCapo)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.NLettera)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.NNumero)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.TestoEM_originale)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.EM_Certificato)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.TestoREL_originale)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.PATH_AllegatoGenerico)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.PATH_AllegatoTecnico)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.NOTE_EM)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.NOTE_Griglia)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.TestoEM_Modificabile)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.chkf)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .Property(e => e.Colore)
                .IsUnicode(false);

            modelBuilder.Entity<EM>()
                .HasMany(e => e.EM1)
                .WithOptional(e => e.EM2)
                .HasForeignKey(e => e.Rif_UIDEM);

            modelBuilder.Entity<EM>()
                .HasMany(e => e.FIRME)
                .WithRequired(e => e.EM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EM>()
                .HasMany(e => e.NOTIFICHE)
                .WithRequired(e => e.EM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<gruppi_politici>()
                .Property(e => e.codice_gruppo)
                .IsUnicode(false);

            modelBuilder.Entity<gruppi_politici>()
                .Property(e => e.nome_gruppo)
                .IsUnicode(false);

            modelBuilder.Entity<gruppi_politici>()
                .HasMany(e => e.EM)
                .WithRequired(e => e.gruppi_politici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<gruppi_politici>()
                .HasMany(e => e.join_gruppi_politici_legislature)
                .WithRequired(e => e.gruppi_politici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<gruppi_politici>()
                .HasMany(e => e.JOIN_GRUPPO_AD)
                .WithRequired(e => e.gruppi_politici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<gruppi_politici>()
                .HasMany(e => e.join_persona_gruppi_politici)
                .WithRequired(e => e.gruppi_politici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<gruppi_politici>()
                .HasMany(e => e.NOTIFICHE_DESTINATARI)
                .WithRequired(e => e.gruppi_politici)
                .HasForeignKey(e => e.IdGruppo);

            modelBuilder.Entity<gruppi_politici>()
                .HasMany(e => e.NOTIFICHE)
                .WithRequired(e => e.gruppi_politici)
                .HasForeignKey(e => e.IdGruppo);

            modelBuilder.Entity<join_persona_AD>()
                .Property(e => e.pass_locale_crypt)
                .IsUnicode(false);

            modelBuilder.Entity<join_persona_assessorati>()
                .Property(e => e.nome_assessorato)
                .IsUnicode(false);

            modelBuilder.Entity<join_persona_recapiti>()
                .Property(e => e.recapito)
                .IsUnicode(false);

            modelBuilder.Entity<join_persona_recapiti>()
                .Property(e => e.tipo_recapito)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<legislature>()
                .Property(e => e.num_legislatura)
                .IsUnicode(false);

            modelBuilder.Entity<legislature>()
                .HasMany(e => e.join_gruppi_politici_legislature)
                .WithRequired(e => e.legislature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<legislature>()
                .HasMany(e => e.join_persona_assessorati)
                .WithRequired(e => e.legislature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<legislature>()
                .HasMany(e => e.join_persona_gruppi_politici)
                .WithRequired(e => e.legislature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<legislature>()
                .HasMany(e => e.join_persona_organo_carica)
                .WithRequired(e => e.legislature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<legislature>()
                .HasMany(e => e.organi)
                .WithRequired(e => e.legislature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LETTERE>()
                .Property(e => e.Lettera)
                .IsUnicode(false);

            modelBuilder.Entity<LETTERE>()
                .Property(e => e.TestoLettera)
                .IsUnicode(false);

            modelBuilder.Entity<MISSIONI>()
                .Property(e => e.Desccrizione)
                .IsUnicode(false);

            modelBuilder.Entity<NOTIFICHE>()
                .Property(e => e.Messaggio)
                .IsUnicode(false);

            //modelBuilder.Entity<NOTIFICHE>()
            //    .HasMany(e => e.NOTIFICHE_DESTINATARI)
            //    .WithRequired(e => e.NOTIFICHE)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<organi>()
                .Property(e => e.nome_organo)
                .IsUnicode(false);

            modelBuilder.Entity<organi>()
                .Property(e => e.nome_organo_breve)
                .IsUnicode(false);

            modelBuilder.Entity<organi>()
                .HasMany(e => e.join_persona_organo_carica)
                .WithRequired(e => e.organi)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<organi>()
                .HasMany(e => e.SEDUTE)
                .WithOptional(e => e.organi)
                .HasForeignKey(e => e.IDOrgano);

            modelBuilder.Entity<PARTI_TESTO>()
                .HasMany(e => e.EM)
                .WithRequired(e => e.PARTI_TESTO)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<persona>()
                .Property(e => e.cognome)
                .IsUnicode(false);

            modelBuilder.Entity<persona>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<persona>()
                .Property(e => e.foto)
                .IsUnicode(false);

            modelBuilder.Entity<persona>()
                .HasMany(e => e.join_persona_AD)
                .WithRequired(e => e.persona)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<persona>()
                .HasMany(e => e.join_persona_assessorati)
                .WithRequired(e => e.persona)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<persona>()
                .HasMany(e => e.join_persona_recapiti)
                .WithRequired(e => e.persona)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PINS>()
                .Property(e => e.PIN)
                .IsUnicode(false);

            modelBuilder.Entity<RUOLI>()
                .Property(e => e.Ruolo)
                .IsUnicode(false);

            modelBuilder.Entity<RUOLI>()
                .Property(e => e.ADGroup)
                .IsUnicode(false);

            modelBuilder.Entity<RUOLI>()
                .HasMany(e => e.NOTIFICHE)
                .WithOptional(e => e.RUOLI)
                .HasForeignKey(e => e.RuoloMittente);

            modelBuilder.Entity<RUOLI>()
                .HasMany(e => e.RUOLI_UTENTE)
                .WithRequired(e => e.RUOLI)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SEDUTE>()
                .Property(e => e.Intervalli)
                .IsUnicode(false);

            modelBuilder.Entity<STAMPE>()
                .Property(e => e.MessaggioErrore)
                .IsUnicode(false);

            modelBuilder.Entity<STAMPE>()
                .Property(e => e.PathFile)
                .IsUnicode(false);

            modelBuilder.Entity<STAMPE>()
                .Property(e => e.Query)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_recapiti>()
                .Property(e => e.id_recapito)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<tbl_recapiti>()
                .Property(e => e.nome_recapito)
                .IsUnicode(false);

            modelBuilder.Entity<TIPI_ATTO>()
                .HasMany(e => e.ATTI)
                .WithRequired(e => e.TIPI_ATTO)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TIPI_EM>()
                .HasMany(e => e.EM)
                .WithRequired(e => e.TIPI_EM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TIPI_NOTIFICA>()
                .Property(e => e.Tipo)
                .IsUnicode(false);

            modelBuilder.Entity<TIPI_NOTIFICA>()
                .HasMany(e => e.NOTIFICHE)
                .WithRequired(e => e.TIPI_NOTIFICA)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tipo_organo>()
                .Property(e => e.descrizione)
                .IsUnicode(false);

            modelBuilder.Entity<TITOLI_MISSIONI>()
                .Property(e => e.Descrizione)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .Property(e => e.cognome)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .Property(e => e.foto)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .Property(e => e.UserAD)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .Property(e => e.pass_locale_crypt)
                .IsUnicode(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.NOTIFICHE)
                .WithRequired(e => e.UTENTI_NoCons)
                .HasForeignKey(e => e.Mittente)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.NOTIFICHE_DESTINATARI)
                .WithRequired(e => e.Destinatario)
                .HasForeignKey(e => e.UIDPersona)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.PINS)
                .WithRequired(e => e.UTENTI_NoCons)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.PINS_NoCons)
                .WithRequired(e => e.UTENTI_NoCons)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.RUOLI_UTENTE)
                .WithRequired(e => e.UTENTI_NoCons)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.SEDUTE)
                .WithOptional(e => e.UTENTI_NoCons)
                .HasForeignKey(e => e.UIDPersonaCreazione);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.STAMPE)
                .WithRequired(e => e.UTENTI_NoCons)
                .HasForeignKey(e => e.UIDUtenteRichiesta)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UTENTI_NoCons>()
                .HasMany(e => e.FIRME)
                .WithRequired(e => e.UTENTI_NoCons)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FIRME>()
                .Property(e => e.FirmaCert)
                .IsUnicode(false);

            modelBuilder.Entity<FIRME>()
                .Property(e => e.Data_firma)
                .IsUnicode(false);

            modelBuilder.Entity<FIRME>()
                .Property(e => e.Data_ritirofirma)
                .IsUnicode(false);

            modelBuilder.Entity<View_Composizione_GiuntaRegionale>()
                .Property(e => e.cognome)
                .IsUnicode(false);

            modelBuilder.Entity<View_Composizione_GiuntaRegionale>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<View_Composizione_GiuntaRegionale>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERE_GRUPPO>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERE_GRUPPO>()
                .Property(e => e.cognome)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERE_GRUPPO>()
                .Property(e => e.codice_gruppo)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERE_GRUPPO>()
                .Property(e => e.nome_gruppo)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERE_GRUPPO>()
                .Property(e => e.num_legislatura)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERI_PEM>()
                .Property(e => e.Cognome)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERI_PEM>()
                .Property(e => e.Nome)
                .IsUnicode(false);

            modelBuilder.Entity<View_CONSIGLIERI_PEM>()
                .Property(e => e.foto)
                .IsUnicode(false);

            modelBuilder.Entity<View_gruppi_politici_con_giunta>()
                .Property(e => e.nome_gruppo)
                .IsUnicode(false);

            modelBuilder.Entity<View_gruppi_politici_con_giunta>()
                .Property(e => e.codice_gruppo)
                .IsUnicode(false);

            modelBuilder.Entity<View_UTENTI>()
                .Property(e => e.cognome)
                .IsUnicode(false);

            modelBuilder.Entity<View_UTENTI>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<View_UTENTI>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<View_UTENTI>()
                .Property(e => e.foto)
                .IsUnicode(false);

            modelBuilder.Entity<View_UTENTI>()
                .Property(e => e.legislature)
                .IsUnicode(false);

            modelBuilder.Entity<View_UTENTI>()
                .Property(e => e.GruppoPolitico_attuale)
                .IsUnicode(false);
        }
    }
}