using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Certificates.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Certificates.GetMyCertificates;

public sealed class GetMyCertificatesQueryHandler : IRequestHandler<GetMyCertificatesQuery, IReadOnlyList<CertificateDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyCertificatesQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CertificateDto>> Handle(GetMyCertificatesQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        return await _unitOfWork.Repository<Certificate>().Query()
            .Where(c => c.VolunteerId == volunteer.Id)
            .OrderByDescending(c => c.GeneratedAt)
            .ProjectTo<CertificateDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
