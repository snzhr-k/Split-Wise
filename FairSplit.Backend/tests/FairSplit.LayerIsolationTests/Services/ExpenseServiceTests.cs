using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Implementations;
using FairSplit.Api.Services.Models;
using FairSplit.Api.Shared.Utilities;
using Moq;

namespace FairSplit.LayerIsolationTests.Services;

public sealed class ExpenseServiceTests
{
    [Fact]
    public async Task Create_WhenGroupDoesNotExist_ThrowsNotFound_WithoutCallingWriteRepositories()
    {
        var expenseRepository = new Mock<IExpenseRepository>();
        var groupRepository = new Mock<IGroupRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var expenseParticipantRepository = new Mock<IExpenseParticipantRepository>();
        var balanceRepository = new Mock<IBalanceRepository>();
        var transactionManager = new Mock<ITransactionManager>();
        var clock = new Mock<IClock>();

        transactionManager
            .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((operation, token) => operation(token));

        groupRepository
            .Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new ExpenseService(
            expenseRepository.Object,
            groupRepository.Object,
            memberRepository.Object,
            expenseParticipantRepository.Object,
            balanceRepository.Object,
            transactionManager.Object,
            clock.Object);

        var command = new CreateExpenseCommand
        {
            GroupId = Guid.NewGuid(),
            PayerMemberId = Guid.NewGuid(),
            Amount = 50m,
            SplitType = ExpenseSplitType.Equal,
            Participants =
            [
                new CreateExpenseParticipantCommand { MemberId = Guid.NewGuid() }
            ]
        };

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(command, CancellationToken.None));

        expenseRepository.Verify(x => x.AddAsync(It.IsAny<FairSplit.Api.Domain.Entities.Expense>(), It.IsAny<CancellationToken>()), Times.Never);
        expenseParticipantRepository.Verify(x => x.AddRangeAsync(It.IsAny<IReadOnlyCollection<FairSplit.Api.Domain.Entities.ExpenseParticipant>>(), It.IsAny<CancellationToken>()), Times.Never);
        balanceRepository.Verify(x => x.ApplyDeltasAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyDictionary<Guid, decimal>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
