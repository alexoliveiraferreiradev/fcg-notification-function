using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Domain.Repositories;
using MediatR;

namespace Fcg.Notification.Function.Application.UseCase.WelcomeEmail
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand>
    {
        private readonly IUserSnapshotRepository _userSnapshotRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(IUserSnapshotRepository userSnapshotRepository, IUnitOfWork unitOfWork)
        {
            _userSnapshotRepository = userSnapshotRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var snapshot = await _userSnapshotRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if(snapshot is null )
            {
                _userSnapshotRepository.Add(new UserSnapshot(request.UserId, request.UserName,
                request.Email));
            }
            else
                snapshot.ApplyChanges(request.UserName, request.occurredAt);


            await _unitOfWork.CommitAsync();   
        }
    }
}
