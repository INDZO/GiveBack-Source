using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeactivateUserCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdWithRefreshTokensAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        if (user.Id == _currentUser.RequireUserId())
        {
            throw new ConflictException("Administrators cannot deactivate their own account.");
        }

        user.IsActive = false;
        foreach (var refreshToken in user.RefreshTokens.Where(t => t.IsActive))
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<UserDto>(user);
    }
}
