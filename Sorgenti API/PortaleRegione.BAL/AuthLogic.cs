﻿using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PortaleRegione.BAL.proxyAd;
using PortaleRegione.Contracts;
using PortaleRegione.Domain;
using PortaleRegione.DTO.Autenticazione;
using PortaleRegione.DTO.Domain;
using PortaleRegione.DTO.Enum;
using PortaleRegione.DTO.Model;
using PortaleRegione.DTO.Response;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PortaleRegione.BAL
{
    public class AuthLogic : BaseLogic
    {
        public AuthLogic(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponse> Login(LoginRequest loginModel)
        {
            try
            {
                if (string.IsNullOrEmpty(loginModel.Username))
                {
                    throw new Exception("Username non può essere nullo");
                }

                if (string.IsNullOrEmpty(loginModel.Password))
                {
                    throw new Exception("Password non può essere nulla");
                }

                if (loginModel.Username == AppSettingsConfiguration.Service_Username &&
                    loginModel.Password == AppSettingsConfiguration.Service_Password)
                {
                    var tokenService = GetToken(new PersonaDto
                    {
                        CurrentRole = RuoliIntEnum.SERVIZIO_JOB,
                        cognome = "SERVIZIO",
                        nome = "JOB"
                    });

                    return new LoginResponse
                    {
                        jwt = tokenService
                    };
                }

                var intranetAdService = new proxyAD();

#if DEBUG == true
                if (loginModel.Username.Contains("***"))
                {
                    //HACK LOGIN
                    loginModel.Username = loginModel.Username.Replace("***", "");
                }
                else
                {
#endif

                    if (AppSettingsConfiguration.AutenticazioneAD == 1)
                    {
                        var authResult = intranetAdService.Authenticate(
                            loginModel.Username,
                            loginModel.Password,
                            "CONSIGLIO",
                            AppSettingsConfiguration.TOKEN_R);
                        //var authResult = true;
                        if (!authResult)
                        {
                            throw new Exception(
                                "Nome Utente o Password non validi!");
                        }
                    }
                    else
                    {
                        var authResult_NoAD = await _unitOfWork
                            .Persone
                            .Autentica(
                                loginModel.Username,
                                BALHelper.EncryptString(loginModel.Password, AppSettingsConfiguration.JWT_MASTER)
                            );
                        if (!authResult_NoAD)
                        {
                            throw new Exception(
                                "Nome Utente o Password non validi!");
                        }
                    }
#if DEBUG == true
                }
#endif
                var personaInDb = await _unitOfWork.Persone.Get(@"CONSIGLIO\" + loginModel.Username);
                var persona = Mapper.Map<View_UTENTI, PersonaDto>(personaInDb);

                if (persona == null)
                {
                    throw new Exception(
                        "Autenticazione corretta, ma l'utente non risulta presente nel sistema. Contattare l'amministratore di sistema.");
                }

                // #730 - Utente non attivo non deve poter entrare nella piattaforma
                if (!persona.attivo.HasValue)
                {
                    throw new Exception("Utente non attivato.");
                }
                if (persona.attivo == false)
                {
                    throw new Exception("Utente non attivo.");
                }

                var Gruppi_Utente = new List<string>();

                if (AppSettingsConfiguration.AutenticazioneAD == 1)
                {
                    Gruppi_Utente.AddRange(intranetAdService.GetGroups(
                        loginModel.Username.Replace(@"CONSIGLIO\", ""), "PEM_", AppSettingsConfiguration.TOKEN_R));
                }
                else
                {
                    Gruppi_Utente.AddRange(personaInDb.gruppi_autorizzazione.Split(';'));
                }

                if (Gruppi_Utente.Count == 0)
                {
                    throw new Exception("Utente non configurato correttamente.");
                }

                var lRuoli = Gruppi_Utente.Select(@group => $"CONSIGLIO\\{@group}").ToList();

                var token = await GetToken(persona, lRuoli);
                await _unitOfWork.Sessioni.NuovaSessione(new Sessioni
                {
                    DataIngresso = DateTime.Now,
                    uidSessione = Guid.NewGuid(),
                    uidUtente = persona.UID_persona
                });
                return new LoginResponse
                {
                    jwt = token,
                    id = persona.UID_persona,
                    persona = persona
                };
            }
            catch (Exception e)
            {
                throw e;    
            }
        }

        public async Task<LoginResponse> CambioRuolo(RuoliIntEnum ruolo, SessionManager session)
        {
            try
            {
                var ruoloInDb = await _unitOfWork.Ruoli.Get((int)ruolo);
                if (ruoloInDb == null)
                {
                    throw new Exception("Ruolo non trovato");
                }

                var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(session._currentUId));
                var intranetAdService = new proxyAD();
                var Gruppi_Utente = new List<string>(intranetAdService.GetGroups(
                    persona.userAD.Replace(@"CONSIGLIO\", ""), "PEM_",
                    AppSettingsConfiguration.TOKEN_R));

                var lRuoli = Gruppi_Utente.Select(group => $"CONSIGLIO\\{group}").ToList();

                var ruoli_utente = await _unitOfWork.Ruoli.RuoliUtente(lRuoli);

                var ruoloAccessibile = ruoli_utente.SingleOrDefault(r => r.IDruolo == (int)ruolo);
                if (ruoloAccessibile == null)
                {
                    throw new Exception("Ruolo non accessibile");
                }

                persona.CurrentRole = ruolo;
                persona.Gruppo = await _unitOfWork.Gruppi.GetGruppoPersona(lRuoli, persona.IsGiunta);
                persona.Carica = await _unitOfWork.Persone.GetCarica(persona.UID_persona);
                persona.Ruoli = ruoli_utente.Select(Mapper.Map<RUOLI, RuoliDto>);

                var token = GetToken(persona);

                return new LoginResponse
                {
                    jwt = token,
                    id = persona.UID_persona,
                    persona = persona
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<LoginResponse> CambioGruppo(int gruppo, SessionManager session)
        {
            try
            {
                var gruppiDto = Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(await _unitOfWork.Gruppi.Get(gruppo));
                var persona = Mapper.Map<View_UTENTI, PersonaDto>(await _unitOfWork.Persone.Get(session._currentUId));
                var intranetAdService = new proxyAD();
                var Gruppi_Utente = new List<string>(intranetAdService.GetGroups(
                    persona.userAD.Replace(@"CONSIGLIO\", ""), "PEM_",
                    AppSettingsConfiguration.TOKEN_R));

                var lRuoli = Gruppi_Utente.Select(group => $"CONSIGLIO\\{group}").ToList();

                var ruoli_utente = await _unitOfWork.Ruoli.RuoliUtente(lRuoli);

                persona.Gruppo = gruppiDto ?? throw new Exception("ListaGruppo non trovato");
                persona.CurrentRole = RuoliIntEnum.Responsabile_Segreteria_Politica;
                persona.Carica = await _unitOfWork.Persone.GetCarica(persona.UID_persona);
                persona.Ruoli = ruoli_utente.Select(Mapper.Map<RUOLI, RuoliDto>);
                var token = GetToken(persona);

                return new LoginResponse
                {
                    jwt = token,
                    id = persona.UID_persona,
                    persona = persona
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Metodo per calcolare il JWT Token
        /// </summary>
        /// <param name="personaDto"></param>
        /// <param name="lRuoli_Gruppi"></param>
        /// <returns></returns>
        private async Task<string> GetToken(PersonaDto personaDto, List<string> lRuoli_Gruppi)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(
                    AppSettingsConfiguration.JWT_MASTER);

                var ruoli_utente = await _unitOfWork.Ruoli.RuoliUtente(lRuoli_Gruppi);
                personaDto.Ruoli = ruoli_utente.Select(Mapper.Map<RUOLI, RuoliDto>);
                personaDto.CurrentRole = (RuoliIntEnum)ruoli_utente.First().IDruolo;
                personaDto.Gruppo = Mapper.Map<View_gruppi_politici_con_giunta, GruppiDto>(
                    await _unitOfWork.Gruppi.GetGruppoAttuale(lRuoli_Gruppi, personaDto.CurrentRole));
                personaDto.Carica = await _unitOfWork.Persone.GetCarica(personaDto.UID_persona);

                if (personaDto.Gruppo != null)
                {
                    var capogruppo = await _unitOfWork.Gruppi.GetCapoGruppo(personaDto.Gruppo.id_gruppo);
                    if (capogruppo != null)
                        if (capogruppo.id_persona == personaDto.id_persona)
                        {
                            personaDto.IsCapoGruppo = true;
                        }
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, ruoli_utente.First().IDruolo.ToString()),
                    new Claim(ClaimTypes.Name, personaDto.UID_persona.ToString())
                };

                if (personaDto.Gruppo != null)
                {
                    claims.Add(new Claim("gruppo", personaDto.Gruppo.id_gruppo.ToString()));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Audience = AppSettingsConfiguration.URL_API,
                    Issuer = AppSettingsConfiguration.URL_API,
                    Subject = new ClaimsIdentity(claims),
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddHours(AppSettingsConfiguration.JWT_EXPIRATION),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Metodo per calcolare il JWT Token
        /// </summary>
        /// <param name="personaDto"></param>
        /// <returns></returns>
        private string GetToken(PersonaDto personaDto)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(
                    AppSettingsConfiguration.JWT_MASTER);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, Convert.ToInt32(personaDto.CurrentRole).ToString()),
                    new Claim(ClaimTypes.Name, personaDto.UID_persona.ToString())
                };

                if (personaDto.Gruppo != null)
                {
                    claims.Add(new Claim("gruppo", personaDto.Gruppo.id_gruppo.ToString()));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Audience = AppSettingsConfiguration.URL_API,
                    Issuer = AppSettingsConfiguration.URL_API,
                    Subject = new ClaimsIdentity(claims),
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddHours(AppSettingsConfiguration.JWT_EXPIRATION),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task Logout(Guid currentUid)
        {
            await _unitOfWork.Sessioni.ChiudiSessione(currentUid);
        }
    }
}