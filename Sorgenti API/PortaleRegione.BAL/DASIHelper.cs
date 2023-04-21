using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using System.Collections.Generic;

namespace PortaleRegione.BAL
{
    public static class DASIHelper
    {
        public static string GetDescrizioneRisposta(TipoRispostaEnum tipoRisposta, List<CommissioneDto> commissioni)
        {
            string result;

            switch (tipoRisposta)
            {
                case TipoRispostaEnum.ORALE:
                    result = "Orale";
                    break;
                case TipoRispostaEnum.SCRITTA:
                    result = "Scritta"; //#725
                    break;
                case TipoRispostaEnum.COMMISSIONE:
                    result = "In Commissione";
                    break;
                case TipoRispostaEnum.IMMEDIATA:
                    result = "Immediata";
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }
    }
}