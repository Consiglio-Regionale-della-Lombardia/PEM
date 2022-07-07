using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.BAL
{
    public static class DASIHelper
    {
        public static string GetDescrizioneRisposta(TipoRispostaEnum tipoRisposta, List<CommissioneDto> commissioni)
        {
            var result = string.Empty;

            switch (tipoRisposta)
            {
                case TipoRispostaEnum.ORALE:
                    result = "Orale";
                    break;
                case TipoRispostaEnum.SCRITTO:
                    result = "Scritto";
                    break;
                case TipoRispostaEnum.COMMISSIONE:
                    result = "In Commissione";
                    break;
                case TipoRispostaEnum.IMMEDIATA:
                    result = "Immediata";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoRisposta), tipoRisposta, null);
            }

            if (commissioni.Any())
            {
                result += $" ({commissioni.First().nome_organo})";
            }

            return result;
        }
    }
}