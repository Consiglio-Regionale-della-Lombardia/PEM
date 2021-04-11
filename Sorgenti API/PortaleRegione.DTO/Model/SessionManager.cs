using System;
using PortaleRegione.DTO.Enum;

namespace PortaleRegione.DTO.Model
{
    /// <summary>
    ///     Classe di mappaggio sessione utente
    /// </summary>
    public class SessionManager
    {
        public Guid _currentUId { get; set; }
        public RuoliIntEnum _currentRole { get; set; }
        public int _currentGroup { get; set; }
    }
}