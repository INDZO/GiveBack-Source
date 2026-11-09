using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.ActivateUser;

public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ActivateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        user.IsActive = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<UserDto>(user);
    }
}
