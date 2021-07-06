using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;

namespace PortaleRegione.BAL
{
    public class EMPublicLogic : BaseLogic
    {
        private readonly EmendamentiLogic _logicEm;
        private readonly IUnitOfWork _unitOfWork;

        public EMPublicLogic(IUnitOfWork unitOfWork, EmendamentiLogic logicEm)
        {
            _unitOfWork = unitOfWork;
            _logicEm = logicEm;
        }

        public async Task<string> GetBody(EM em, IEnumerable<FIRME> firme)
        {
            try
            {
                var persona = await _unitOfWork.Persone.Get(em.UIDPersonaProponente.Value);
                var personaDto = Mapper.Map<View_UTENTI, PersonaDto>(persona);
                var emendamentoDto = await _logicEm.GetEM_DTO(em, personaDto);
                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);
                var attoDto = Mapper.Map<ATTI, AttiDto>(atto);
                var firmeDto = firme.Select(Mapper.Map<FIRME, FirmeDto>);

                try
                {
                    var body = GetTemplate(TemplateTypeEnum.PDF);
                    GetBody(emendamentoDto, attoDto, firmeDto, personaDto, false, ref body);
                    return body;
                }
                catch (Exception e)
                {
                    Log.Error("GetBodyEM", e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                Log.Error("Logic - GetBodyEM", e);
                throw e;
            }
        }
    }
}