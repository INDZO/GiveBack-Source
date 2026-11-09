using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Auth.RegisterVolunteer;

public sealed class RegisterVolunteerCommandHandler : IRequestHandler<RegisterVolunteerCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public RegisterVolunteerCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterVolunteerCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }

        var distinctSkillIds = request.SkillIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (distinctSkillIds.Length > 0)
        {
            var existingSkillCount = await _unitOfWork.Repository<Skill>().Query()
                .CountAsync(s => distinctSkillIds.Contains(s.Id), cancellationToken);
            if (existingSkillCount != distinctSkillIds.Length)
            {
                throw new NotFoundException("One or more skills were not found.");
            }
        }

        var distinctCauseIds = request.CauseIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (distinctCauseIds.Length > 0)
        {
            var existingCauseCount = await _unitOfWork.Repository<Cause>().Query()
                .CountAsync(c => distinctCauseIds.Contains(c.Id), cancellationToken);
            if (existingCauseCount != distinctCauseIds.Length)
            {
                throw new NotFoundException("One or more causes were not found.");
            }
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRole.Volunteer,
            IsActive = true,
            VolunteerProfile = new VolunteerProfile
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Bio = request.Bio
            }
        };

        foreach (var skillId in distinctSkillIds)
        {
            user.VolunteerProfile.VolunteerSkills.Add(new VolunteerSkill { SkillId = skillId });
        }

        foreach (var causeId in distinctCauseIds)
        {
            user.VolunteerProfile.VolunteerInterests.Add(new VolunteerInterest { CauseId = causeId });
        }

        foreach (var item in request.Availability ?? Array.Empty<AvailabilityRequestDto>())
        {
            if (item.StartTime >= item.EndTime)
            {
                throw new ConflictException("Availability start time must be before end time.");
            }

            user.VolunteerProfile.Availabilities.Add(new VolunteerAvailability
            {
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            });
        }

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshToken = _refreshTokenService.CreateRefreshToken(user.Id);
        await _unitOfWork.Repository<GiveBack.Domain.Entities.RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken.PlainToken,
            UserId = user.Id,
            Role = user.Role
        };
    }
}
