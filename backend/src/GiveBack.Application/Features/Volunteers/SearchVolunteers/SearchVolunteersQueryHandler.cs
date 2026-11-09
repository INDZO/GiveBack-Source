using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Volunteers.SearchVolunteers;

public sealed class SearchVolunteersQueryHandler : IRequestHandler<SearchVolunteersQuery, IReadOnlyList<VolunteerSearchResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SearchVolunteersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<VolunteerSearchResultDto>> Handle(SearchVolunteersQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != nameof(UserRole.Admin))
        {
            var organization = await _unitOfWork.Organizations.GetByUserIdAsync(
                _currentUser.RequireUserId(), cancellationToken);
            if (organization?.IsVerified != true)
            {
                throw new ForbiddenException("Only verified organizations can search for volunteers.");
            }
        }

        var query = _unitOfWork.Repository<VolunteerProfile>().Query()
            .AsNoTracking()
            .Where(v => v.User.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = request.Name.Trim();
            query = query.Where(v =>
                v.FirstName.Contains(name) ||
                v.LastName.Contains(name) ||
                (v.FirstName + " " + v.LastName).Contains(name));
        }

        if (request.SkillId.HasValue)
        {
            query = query.Where(v => v.VolunteerSkills.Any(s => s.SkillId == request.SkillId.Value));
        }

        if (request.CauseId.HasValue)
        {
            query = query.Where(v => v.VolunteerInterests.Any(i => i.CauseId == request.CauseId.Value));
        }

        return await query
            .OrderBy(v => v.LastName)
            .ThenBy(v => v.FirstName)
            .Take(50)
            .Select(v => new VolunteerSearchResultDto
            {
                Id = v.Id,
                FirstName = v.FirstName,
                LastName = v.LastName,
                TotalHours = v.TotalHours,
                SkillNames = v.VolunteerSkills.Select(s => s.Skill.Name).ToList(),
                CauseNames = v.VolunteerInterests.Select(i => i.Cause.Name).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
