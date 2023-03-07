using AutoMapper;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Domain.Essentials;
using PortaleRegione.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class EMPublicLogic : BaseLogic
    {
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

                var persona = Users.First(p => p.UID_persona == em.UIDPersonaProponente);
                var personaDto = Mapper.Map<PersonaLightDto, PersonaDto>(persona);
                var emendamentoDto = await _logicEm.GetEM_DTO(em.UIDEM, atto, personaDto);
                var attoDto = Mapper.Map<ATTI, AttiDto>(atto);

                try
                {
                    var body = GetTemplate(TemplateTypeEnum.PDF);
                    GetBody(emendamentoDto, attoDto, firme.ToList(), personaDto, false, ref body);
                    return body;
                }
                catch (Exception e)
                {
                    //Log.Error("GetBodyEM", e);
                    throw e;
                }
            }
            catch (Exception e)
            {
                //Log.Error("Logic - GetBodyEM", e);
                throw e;
            }
        }
    }
}