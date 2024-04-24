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

using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Unity;

namespace PortaleRegione.Api.Public
{
    /// <summary>
    ///     Implementa un resolver di dipendenze personalizzato che integra il contenitore di Unity con Web API,
    ///     consentendo la risoluzione delle dipendenze dei controller e dei loro servizi.
    /// </summary>
    public class UnityResolver : IDependencyResolver
    {
        /// <summary>
        ///     Il contenitore di Unity utilizzato per risolvere le dipendenze.
        /// </summary>
        protected IUnityContainer container;

        /// <summary>
        ///     Inizializza una nuova istanza della classe UnityResolver con un contenitore di Unity specificato.
        /// </summary>
        /// <param name="container">Il contenitore di Unity per risolvere le dipendenze.</param>
        /// <exception cref="ArgumentNullException">Lancia un'eccezione se il contenitore è null.</exception>
        public UnityResolver(IUnityContainer container)
        {
            this.container = container ??
                             throw new ArgumentNullException("container", "Il contenitore non può essere null.");
        }

        /// <summary>
        ///     Risolve singolarmente il servizio del tipo specificato.
        /// </summary>
        /// <param name="serviceType">Il tipo di servizio da risolvere.</param>
        /// <returns>L'istanza del servizio risolto o null se la risoluzione fallisce.</returns>
        public object GetService(Type serviceType)
        {
            try
            {
                return container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return
                    null; // Ritorna null in caso di fallimento per permettere la risoluzione dei tipi non registrati.
            }
        }

        /// <summary>
        ///     Risolve tutti i servizi del tipo specificato.
        /// </summary>
        /// <param name="serviceType">Il tipo di servizio da risolvere.</param>
        /// <returns>Una collezione di istanze del servizio o una lista vuota se la risoluzione fallisce.</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>(); // Ritorna una lista vuota per i tipi non registrati.
            }
        }

        /// <summary>
        ///     Avvia un nuovo scope di risoluzione delle dipendenze, creando un sotto-contenitore.
        /// </summary>
        /// <returns>Un nuovo IDependencyScope che rappresenta il nuovo scope.</returns>
        public IDependencyScope BeginScope()
        {
            var child = container.CreateChildContainer();
            return new UnityResolver(child); // Crea un nuovo resolver con il sotto-contenitore.
        }

        /// <summary>
        ///     Esegue le attività di pulizia del contenitore di Unity.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Previeni la chiamata del finalizzatore se Dispose è stato già chiamato.
        }

        /// <summary>
        ///     Dispose il contenitore di Unity. Può essere sovrascritto in una classe derivata.
        /// </summary>
        /// <param name="disposing">True se il metodo è stato chiamato direttamente o indirettamente da un codice utente.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                container.Dispose();
            }
        }
    }
}