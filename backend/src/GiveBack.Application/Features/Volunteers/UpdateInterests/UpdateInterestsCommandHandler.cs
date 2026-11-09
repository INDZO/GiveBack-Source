using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Volunteers.UpdateInterests;

public sealed class UpdateInterestsCommandHandler : IRequestHandler<UpdateInterestsCommand, IReadOnlyList<CauseDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateInterestsCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CauseDto>> Handle(UpdateInterestsCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Volunteers.GetByUserIdWithDetailsAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var distinctCauseIds = request.CauseIds.Distinct().ToArray();
        if (distinctCauseIds.Length > 0)
        {
            var existingCauseCount = await _unitOfWork.Repository<Cause>().Query()
                .CountAsync(c => distinctCauseIds.Contains(c.Id), cancellationToken);
            if (existingCauseCount != distinctCauseIds.Length)
            {
                throw new NotFoundException("One or more causes were not found.");
            }
        }
        var existingCauseIds = profile.VolunteerInterests.Select(x => x.CauseId).ToHashSet();
        var requestedCauseIds = distinctCauseIds.ToHashSet();

        foreach (var existing in profile.VolunteerInterests.Where(x => !requestedCauseIds.Contains(x.CauseId)).ToList())
        {
            _unitOfWork.Repository<VolunteerInterest>().Remove(existing);
        }

        foreach (var causeId in requestedCauseIds.Except(existingCauseIds))
        {
            profile.VolunteerInterests.Add(new VolunteerInterest { VolunteerId = profile.Id, CauseId = causeId });
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var causes = await _unitOfWork.Repository<Cause>().Query().Where(c => distinctCauseIds.Contains(c.Id)).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CauseDto>>(causes);
    }
}
