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
using System.Linq;

namespace PortaleRegione.Client.Helpers
{
    public static class SanitizeHtmlHelper
    {
        /// <summary>
        /// Sanitizza l'HTML correggendo tag li orfani e altri problemi strutturali
        /// </summary>
        /// <param name="html">HTML da sanitizzare</param>
        /// <returns>HTML corretto e sicuro</returns>
        public static string SanitizeHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();

                // Configura il parser per essere tollerante
                doc.OptionFixNestedTags = true;
                doc.OptionAutoCloseOnEnd = true;
                doc.OptionCheckSyntax = false;

                // Carica l'HTML
                doc.LoadHtml(html);

                // Trova tutti i tag <li> nel documento
                var orphanedLiNodes = new List<HtmlAgilityPack.HtmlNode>();
                FindOrphanedLiNodes(doc.DocumentNode, orphanedLiNodes);

                // Correggi i tag <li> orfani raggruppandoli in <ul>
                if (orphanedLiNodes.Count > 0)
                {
                    FixOrphanedLiNodes(orphanedLiNodes);
                }

                // Restituisce l'HTML corretto
                return doc.DocumentNode.OuterHtml;
            }
            catch (Exception ex)
            {
                // In caso di errore, logga e restituisce l'HTML originale
                System.Diagnostics.Debug.WriteLine($"Errore durante la sanitizzazione HTML: {ex.Message}");
                return html;
            }
        }

        /// <summary>
        /// Trova tutti i tag li che non hanno un genitore ul o ol
        /// </summary>
        private static void FindOrphanedLiNodes(HtmlAgilityPack.HtmlNode node, List<HtmlAgilityPack.HtmlNode> orphanedLiNodes)
        {
            if (node == null) return;

            // Controlla se il nodo corrente è un <li> orfano
            if (node.Name.ToLower() == "li")
            {
                var parent = node.ParentNode;
                if (parent != null &&
                    parent.Name.ToLower() != "ul" &&
                    parent.Name.ToLower() != "ol")
                {
                    orphanedLiNodes.Add(node);
                }
            }

            // Ricerca ricorsiva nei nodi figli
            if (node.HasChildNodes)
            {
                foreach (var childNode in node.ChildNodes.ToList())
                {
                    FindOrphanedLiNodes(childNode, orphanedLiNodes);
                }
            }
        }

        /// <summary>
        /// Corregge i tag li orfani raggruppandoli in ul
        /// </summary>
        private static void FixOrphanedLiNodes(List<HtmlAgilityPack.HtmlNode> orphanedLiNodes)
        {
            // Raggruppa i tag li consecutivi per genitore
            var groupedByParent = new Dictionary<HtmlAgilityPack.HtmlNode, List<HtmlAgilityPack.HtmlNode>>();

            foreach (var liNode in orphanedLiNodes)
            {
                var parent = liNode.ParentNode;
                if (parent == null) continue;

                if (!groupedByParent.ContainsKey(parent))
                {
                    groupedByParent[parent] = new List<HtmlAgilityPack.HtmlNode>();
                }

                groupedByParent[parent].Add(liNode);
            }

            // Per ogni gruppo di li orfani, crea un ul e sposta i li al suo interno
            foreach (var group in groupedByParent)
            {
                var parent = group.Key;
                var liNodes = group.Value;

                if (liNodes.Count == 0) continue;

                // Trova i li consecutivi e raggruppali
                var consecutiveGroups = new List<List<HtmlAgilityPack.HtmlNode>>();
                var currentGroup = new List<HtmlAgilityPack.HtmlNode>();

                HtmlAgilityPack.HtmlNode previousNode = null;
                foreach (var liNode in liNodes.OrderBy(n => n.StreamPosition))
                {
                    if (previousNode != null)
                    {
                        // Verifica se i nodi sono consecutivi controllando i nodi tra loro
                        bool areConsecutive = AreNodesConsecutive(previousNode, liNode);

                        if (!areConsecutive)
                        {
                            // Inizia un nuovo gruppo
                            if (currentGroup.Count > 0)
                            {
                                consecutiveGroups.Add(new List<HtmlAgilityPack.HtmlNode>(currentGroup));
                                currentGroup.Clear();
                            }
                        }
                    }

                    currentGroup.Add(liNode);
                    previousNode = liNode;
                }

                // Aggiungi l'ultimo gruppo
                if (currentGroup.Count > 0)
                {
                    consecutiveGroups.Add(currentGroup);
                }

                // Crea un ul per ogni gruppo di li consecutivi
                foreach (var consecutiveGroup in consecutiveGroups)
                {
                    if (consecutiveGroup.Count == 0) continue;

                    var firstLi = consecutiveGroup[0];

                    // Crea il nuovo ul
                    var ulNode = firstLi.OwnerDocument.CreateElement("ul");

                    // Inserisce il ul prima del primo li
                    parent.InsertBefore(ulNode, firstLi);

                    // Sposta tutti i li dentro il ul
                    foreach (var liNode in consecutiveGroup)
                    {
                        parent.RemoveChild(liNode);
                        ulNode.AppendChild(liNode);
                    }
                }
            }
        }

        /// <summary>
        /// Verifica se due nodi sono consecutivi controllando che non ci siano altri li tra loro
        /// </summary>
        private static bool AreNodesConsecutive(HtmlAgilityPack.HtmlNode node1, HtmlAgilityPack.HtmlNode node2)
        {
            var parent = node1.ParentNode;
            if (parent == null || parent != node2.ParentNode)
            {
                return false;
            }

            var childNodes = parent.ChildNodes;
            int index1 = childNodes.IndexOf(node1);
            int index2 = childNodes.IndexOf(node2);

            if (index1 == -1 || index2 == -1 || index2 <= index1)
            {
                return false;
            }

            // Controlla se tra i due nodi ci sono solo spazi bianchi o commenti
            for (int i = index1 + 1; i < index2; i++)
            {
                var node = childNodes[i];

                // Ignora nodi di testo vuoti e commenti
                if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        return false;
                    }
                }
                else if (node.NodeType != HtmlAgilityPack.HtmlNodeType.Comment)
                {
                    // Se c'è un elemento HTML tra i due li, non sono consecutivi
                    return false;
                }
            }

            return true;
        }
    }
}