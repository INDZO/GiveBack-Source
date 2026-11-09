using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.CreateCause;

public sealed class CreateCauseCommandHandler : IRequestHandler<CreateCauseCommand, CauseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCauseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CauseDto> Handle(CreateCauseCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Repository<Cause>().Query().AnyAsync(c => c.Name == request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("Cause name must be unique.");
        }
        var cause = new Cause { Name = request.Name.Trim() };
        await _unitOfWork.Repository<Cause>().AddAsync(cause, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CauseDto>(cause);
    }
}
