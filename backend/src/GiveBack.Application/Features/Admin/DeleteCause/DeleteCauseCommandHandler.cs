using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.DeleteCause;

public sealed class DeleteCauseCommandHandler : IRequestHandler<DeleteCauseCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCauseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCauseCommand request, CancellationToken cancellationToken)
    {
        var cause = await _unitOfWork.Repository<Cause>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Cause was not found.");
        var isUsed = await _unitOfWork.Opportunities.Query().AnyAsync(o => o.CauseId == request.Id, cancellationToken) ||
            await _unitOfWork.Repository<VolunteerInterest>().Query().AnyAsync(i => i.CauseId == request.Id, cancellationToken);
        if (isUsed)
        {
            throw new ConflictException("Cause is used by volunteers or opportunities and cannot be deleted.");
        }

        _unitOfWork.Repository<Cause>().Remove(cause);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
