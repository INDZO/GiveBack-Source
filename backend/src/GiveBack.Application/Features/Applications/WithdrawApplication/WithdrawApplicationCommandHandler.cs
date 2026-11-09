using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Enums;
using MediatR;

namespace GiveBack.Application.Features.Applications.WithdrawApplication;

public sealed class WithdrawApplicationCommandHandler : IRequestHandler<WithdrawApplicationCommand, ApplicationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WithdrawApplicationCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApplicationDto> Handle(WithdrawApplicationCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var application = await _unitOfWork.Applications.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Application was not found.");

        var canWithdraw = application.ApplicationType switch
        {
            ApplicationType.Individual => application.VolunteerId == volunteer.Id,
            ApplicationType.Team => application.Team?.CaptainVolunteerId == volunteer.Id,
            _ => false
        };
        if (!canWithdraw)
        {
            throw new ForbiddenException("Only the applicant or team captain can withdraw this application.");
        }
        if (application.Status != ApplicationStatus.Submitted)
        {
            throw new ConflictException("Only submitted applications can be withdrawn.");
        }

        application.Status = ApplicationStatus.Withdrawn;
        application.ReviewedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ApplicationDto>(application);
    }
}
