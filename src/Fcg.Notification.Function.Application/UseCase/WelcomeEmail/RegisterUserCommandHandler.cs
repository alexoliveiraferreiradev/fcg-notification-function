using Fcg.Core.Abstractions.Interfaces;
using Fcg.Notification.Function.Application.Common.Interfaces;
using Fcg.Notification.Function.Domain.Entities;
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
            var userSnapshot = new UserSnapshot(request.UserName,
                request.Email);
            _userSnapshotRepository.Add(userSnapshot);
        }
    }
}
