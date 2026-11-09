using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, VolunteerProfileDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateMyProfileCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VolunteerProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Volunteers.GetByUserIdWithDetailsAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        profile.FirstName = request.FirstName.Trim();
        profile.LastName = request.LastName.Trim();
        profile.Bio = request.Bio;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<VolunteerProfileDto>(profile);
    }
}
