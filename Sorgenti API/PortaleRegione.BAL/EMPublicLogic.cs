using AutoMapper;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using PortaleRegione.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<string> GetBody(EM em, IEnumerable<FirmeDto> firme)
        {
            try
            {
                var atto = await _unitOfWork.Atti.Get(em.UIDAtto);
                var personeInDb = await _unitOfWork.Persone.GetAll();
                var personeInDbLight = personeInDb.Select(Mapper.Map<View_UTENTI, PersonaLightDto>).ToList();

                var persona = personeInDb.First(p => p.UID_persona == em.UIDPersonaProponente);
                var personaDto = Mapper.Map<View_UTENTI, PersonaDto>(persona);
                var emendamentoDto = await _logicEm.GetEM_DTO(em, atto, personaDto, personeInDbLight);
                var attoDto = Mapper.Map<ATTI, AttiDto>(atto);

                try
                {
                    var body = GetTemplate(TemplateTypeEnum.PDF);
                    GetBody(emendamentoDto, attoDto, firme, personaDto, false, ref body);
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