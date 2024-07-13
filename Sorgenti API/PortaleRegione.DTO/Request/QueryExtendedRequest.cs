using System;
using System.Collections.Generic;

namespace PortaleRegione.DTO.Request;

public class QueryExtendedRequest
{
    public List<int> Soggetti { get; set; } = new List<int>();
    public List<int> Stati { get; set; } = new List<int>();
    public List<int> Tipi { get; set; } = new List<int>();
    public List<int> TipiChiusura { get; set; } = new List<int>();
    public List<int> TipiVotazione { get; set; } = new List<int>();
    public List<int> TipiDocumento { get; set; } = new List<int>();
    public bool DocumentiMancanti { get; set; } = false;
    public List<Guid> Proponenti { get; set; } = new List<Guid>();
    public List<Guid> Provvedimenti { get; set; } = new List<Guid>();
    public List<Guid> AttiDaFirmare { get; set; } = new List<Guid>();
}