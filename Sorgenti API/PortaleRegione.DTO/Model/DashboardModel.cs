using System.Collections.Generic;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Response;

namespace PortaleRegione.DTO.Model
{
    public class DashboardModel
    {
        public DashboardModel()
        {
            DASI = new List<RiepilogoDASIModel>();
            PEM = new List<BaseResponse<AttiDto>>();
        }
        public BaseResponse<SeduteDto> Sedute { get; set; }
        public List<RiepilogoDASIModel> DASI { get; set; }
        public List<BaseResponse<AttiDto>> PEM { get; set; }
        public SeduteFormUpdateDto Seduta { get; set; }
        public PersonaDto CurrentUser { get; set; }
    }
}