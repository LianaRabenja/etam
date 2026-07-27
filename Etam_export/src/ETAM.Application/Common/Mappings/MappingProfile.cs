using AutoMapper;
using ETAM.Application.DTOs;
using ETAM.Domain.Entities;

namespace ETAM.Application.Common.Mappings;

/// <summary>Profil AutoMapper Domain &lt;-&gt; DTO.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Chantier, ChantierDto>();
        CreateMap<ChantierCreateDto, Chantier>();

        CreateMap<PrevisionJournaliere, PrevisionDto>()
            .ForMember(d => d.ChantierNom, o => o.MapFrom(s => s.Chantier != null ? s.Chantier.Nom : null))
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Total));
        CreateMap<PrevisionLigne, PrevisionLigneDto>()
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Total));
    }
}
