using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.Client.Models
{
    public class DASIViewModel
    {
        public BaseResponse<AttiDto> Data { get; set; }
        public int ITR { get; set; }
        public int IQT { get; set; }
        public int ITL { get; set; }
        public int MOZ { get; set; }
        public int MOZ_U { get; set; }
        public int MOZ_A { get; set; }
        public int MOZ_C { get; set; }
        public int MOZ_S { get; set; }
        public int ODG { get; set; }
    }
}