using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Lookups.GetSkills;

public sealed class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, IReadOnlyList<SkillDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSkillsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SkillDto>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Skill>().Query()
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ProjectTo<SkillDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
