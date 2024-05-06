using System.Collections.Generic;

namespace PortaleRegione.DTO.Domain
{
    public class ReportDto
    {
        public string ReportName { get; set; }
        public int CoverType { get; set; }
        public int DataViewType { get; set; }
        public List<int> Columns { get; set; } = new List<int>();
        public int ExportFormat { get; set; }
        public string Filters { get; set; }
    }
}