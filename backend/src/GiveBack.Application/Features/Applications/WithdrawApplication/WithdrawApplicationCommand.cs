using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.WithdrawApplication;

public sealed record WithdrawApplicationCommand(int Id) : IRequest<ApplicationDto>;
