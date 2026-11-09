using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Lookups.GetCauses;

public sealed class GetCausesQueryHandler : IRequestHandler<GetCausesQuery, IReadOnlyList<CauseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCausesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CauseDto>> Handle(GetCausesQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Cause>().Query()
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ProjectTo<CauseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
