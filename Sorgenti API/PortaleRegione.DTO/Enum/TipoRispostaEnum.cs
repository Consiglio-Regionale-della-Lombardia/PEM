using System;

namespace PortaleRegione.DTO.Enum
{
    public enum TipoRispostaEnum
    {
        ORALE = 1,
        SCRITTO = 2,
        COMMISSIONE = 3
    }

    public static class TipoRispostaHelper
    {
        public static string GetDescrizioneRisposta(TipoRispostaEnum tipoRisposta)
        {
            switch (tipoRisposta)
            {
                case TipoRispostaEnum.ORALE:
                    return "Orale";
                case TipoRispostaEnum.SCRITTO:
                    return "Scritto";
                case TipoRispostaEnum.COMMISSIONE:
                    return "In Commissione";
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipoRisposta), tipoRisposta, null);
            }
        }
    }
}