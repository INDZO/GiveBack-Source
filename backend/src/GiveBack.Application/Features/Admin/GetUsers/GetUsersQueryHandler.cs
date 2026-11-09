using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Users.Query()
            .OrderByDescending(u => u.CreatedAt)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
