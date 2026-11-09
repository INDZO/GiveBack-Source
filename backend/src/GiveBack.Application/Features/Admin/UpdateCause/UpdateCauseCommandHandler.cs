using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.UpdateCause;

public sealed class UpdateCauseCommandHandler : IRequestHandler<UpdateCauseCommand, CauseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCauseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CauseDto> Handle(UpdateCauseCommand request, CancellationToken cancellationToken)
    {
        var cause = await _unitOfWork.Repository<Cause>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Cause was not found.");
        if (await _unitOfWork.Repository<Cause>().Query().AnyAsync(c => c.Id != request.Id && c.Name == request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("Cause name must be unique.");
        }
        cause.Name = request.Name.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CauseDto>(cause);
    }
}
