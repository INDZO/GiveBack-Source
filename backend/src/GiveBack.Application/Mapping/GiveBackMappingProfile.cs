using AutoMapper;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Application.Features.Auth.Dtos;
using GiveBack.Application.Features.Badges.Dtos;
using GiveBack.Application.Features.Certificates.Dtos;
using GiveBack.Application.Features.HourLogs.Dtos;
using GiveBack.Application.Features.ImpactReports.Dtos;
using GiveBack.Application.Features.Notifications.Dtos;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Application.Features.Organizations.Dtos;
using GiveBack.Application.Features.Teams.Dtos;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Application.Mapping;

public sealed class GiveBackMappingProfile : Profile
{
    public GiveBackMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Skill, SkillDto>();
        CreateMap<Cause, CauseDto>();
        CreateMap<VolunteerAvailability, VolunteerAvailabilityDto>();

        CreateMap<VolunteerProfile, VolunteerProfileDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.SkillIds, o => o.MapFrom(s => s.VolunteerSkills.Select(vs => vs.SkillId)))
            .ForMember(d => d.Skills, o => o.MapFrom(s => s.VolunteerSkills.Select(vs => vs.Skill)))
            .ForMember(d => d.SkillNames, o => o.MapFrom(s => s.VolunteerSkills.Select(vs => vs.Skill.Name)))
            .ForMember(d => d.CauseIds, o => o.MapFrom(s => s.VolunteerInterests.Select(vi => vi.CauseId)))
            .ForMember(d => d.Causes, o => o.MapFrom(s => s.VolunteerInterests.Select(vi => vi.Cause)))
            .ForMember(d => d.CauseNames, o => o.MapFrom(s => s.VolunteerInterests.Select(vi => vi.Cause.Name)))
            .ForMember(d => d.Availability, o => o.MapFrom(s => s.Availabilities))
            .ForMember(d => d.Badges, o => o.MapFrom(s => s.VolunteerBadges));

        CreateMap<OrganizationProfile, OrganizationProfileDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email));

        CreateMap<Opportunity, OpportunityDto>()
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Organization.Name))
            .ForMember(d => d.CauseName, o => o.MapFrom(s => s.Cause.Name))
            .ForMember(d => d.SkillIds, o => o.MapFrom(s => s.OpportunitySkills.Select(os => os.SkillId)))
            .ForMember(d => d.Skills, o => o.MapFrom(s => s.OpportunitySkills.Select(os => os.Skill)))
            .ForMember(d => d.SkillNames, o => o.MapFrom(s => s.OpportunitySkills.Select(os => os.Skill.Name)));
        CreateMap<Opportunity, OpportunityDetailsDto>()
            .IncludeBase<Opportunity, OpportunityDto>();
        CreateMap<OpportunityInvitation, OpportunityInvitationDto>()
            .ForMember(d => d.OpportunityTitle, o => o.MapFrom(s => s.Opportunity.Title))
            .ForMember(d => d.OrganizationName, o => o.MapFrom(s => s.Opportunity.Organization.Name))
            .ForMember(d => d.StartAt, o => o.MapFrom(s => s.Opportunity.StartAt));

        CreateMap<VolunteerApplication, ApplicationDto>()
            .ForMember(d => d.OpportunityTitle, o => o.MapFrom(s => s.Opportunity.Title))
            .ForMember(d => d.VolunteerName, o => o.MapFrom(s => s.Volunteer == null ? null : s.Volunteer.FirstName + " " + s.Volunteer.LastName))
            .ForMember(d => d.TeamName, o => o.MapFrom(s => s.Team == null ? null : s.Team.Name))
            .ForMember(d => d.OpportunityStartAt, o => o.MapFrom(s => s.Opportunity.StartAt))
            .ForMember(d => d.OpportunityEndAt, o => o.MapFrom(s => s.Opportunity.EndAt))
            .ForMember(d => d.OpportunityDurationHours, o => o.MapFrom(s => s.Opportunity.DurationHours))
            .ForMember(d => d.OpportunityStatus, o => o.MapFrom(s => s.Opportunity.Status));

        CreateMap<VolunteerTeam, VolunteerTeamDto>();
        CreateMap<VolunteerTeamMember, TeamMemberDto>()
            .ForMember(d => d.VolunteerName, o => o.MapFrom(s => s.Volunteer.FirstName + " " + s.Volunteer.LastName));

        CreateMap<VolunteerHourLog, VolunteerHourLogDto>()
            .ForMember(d => d.VolunteerName, o => o.MapFrom(s => s.Volunteer.FirstName + " " + s.Volunteer.LastName))
            .ForMember(d => d.OpportunityTitle, o => o.MapFrom(s => s.Opportunity.Title));

        CreateMap<Certificate, CertificateDto>();
        CreateMap<ImpactReport, ImpactReportDto>()
            .ForMember(d => d.OpportunityTitle, o => o.MapFrom(s => s.Opportunity.Title));
        CreateMap<Badge, BadgeDto>();
        CreateMap<VolunteerBadge, VolunteerBadgeDto>()
            .ForMember(d => d.BadgeName, o => o.MapFrom(s => s.Badge.Name))
            .ForMember(d => d.BadgeDescription, o => o.MapFrom(s => s.Badge.Description));
        CreateMap<Notification, NotificationDto>();

    }
}
