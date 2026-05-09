using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Business;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Implementations;
using FairSplit.Api.Services.Models;
using FairSplit.Api.Shared.Utilities;
using FluentAssertions;
using Moq;

namespace FairSplit.LayerIsolationTests.Services;

public sealed class ExpenseServiceTests
{
    public sealed class SuccessCases
    {
        [Fact]
        public async Task CreateAsync_WhenValidEqualSplit_CreatesExpenseAndUpdatesBalances()
        {
            var now = new DateTimeOffset(2026, 5, 9, 10, 30, 0, TimeSpan.Zero);
            var groupId = Guid.NewGuid();
            var payerId = Guid.NewGuid();
            var participantId = Guid.NewGuid();

            var command = new CreateExpenseCommand
            {
                GroupId = groupId,
                PayerMemberId = payerId,
                Amount = 120m,
                SplitType = ExpenseSplitType.Equal,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = payerId },
                    new CreateExpenseParticipantCommand { MemberId = participantId }
                ]
            };

            var members = new[]
            {
                new Member { Id = payerId, GroupId = groupId },
                new Member { Id = participantId, GroupId = groupId }
            };

            var (service, harness) = CreateService(now);
            harness.GroupRepository
                .Setup(x => x.ExistsAsync(groupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            harness.MemberRepository
                .Setup(x => x.GetByIdsInGroupAsync(groupId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(members);

            Expense? savedExpense = null;
            harness.ExpenseRepository
                .Setup(x => x.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
                .Callback<Expense, CancellationToken>((expense, _) => savedExpense = expense);

            IReadOnlyCollection<ExpenseParticipant>? savedParticipants = null;
            harness.ExpenseParticipantRepository
                .Setup(x => x.AddRangeAsync(It.IsAny<IReadOnlyCollection<ExpenseParticipant>>(), It.IsAny<CancellationToken>()))
                .Callback<IReadOnlyCollection<ExpenseParticipant>, CancellationToken>((participants, _) => savedParticipants = participants);

            IReadOnlyDictionary<Guid, decimal>? deltas = null;
            harness.BalanceRepository
                .Setup(x => x.ApplyDeltasAsync(groupId, It.IsAny<IReadOnlyDictionary<Guid, decimal>>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, IReadOnlyDictionary<Guid, decimal>, CancellationToken>((_, applied, _) => deltas = applied);

            var result = await service.CreateAsync(command, CancellationToken.None);

            result.GroupId.Should().Be(groupId);
            result.PayerMemberId.Should().Be(payerId);
            result.Amount.Should().Be(120m);
            result.CreatedAtUtc.Should().Be(now);

            savedExpense.Should().NotBeNull();
            savedExpense!.CreatedAtUtc.Should().Be(now);
            savedParticipants.Should().NotBeNull();
            savedParticipants!.Should().HaveCount(2);

            deltas.Should().NotBeNull();
            deltas![payerId].Should().Be(60m);
            deltas[participantId].Should().Be(-60m);

            harness.TransactionManager.Verify(
                x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenClockIsProvided_UsesClockForCreatedAtUtc()
        {
            var now = new DateTimeOffset(2026, 5, 9, 12, 45, 0, TimeSpan.Zero);
            var groupId = Guid.NewGuid();
            var payerId = Guid.NewGuid();

            var command = new CreateExpenseCommand
            {
                GroupId = groupId,
                PayerMemberId = payerId,
                Amount = 10m,
                SplitType = ExpenseSplitType.Equal,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = payerId }
                ]
            };

            var members = new[]
            {
                new Member { Id = payerId, GroupId = groupId }
            };

            var (service, harness) = CreateService(now);
            harness.GroupRepository
                .Setup(x => x.ExistsAsync(groupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            harness.MemberRepository
                .Setup(x => x.GetByIdsInGroupAsync(groupId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(members);

            var result = await service.CreateAsync(command, CancellationToken.None);

            result.CreatedAtUtc.Should().Be(now);
        }
    }

    public sealed class ValidationFailures
    {
        [Fact]
        public async Task CreateAsync_WhenAmountIsZero_ThrowsValidationException()
        {
            var command = new CreateExpenseCommand
            {
                GroupId = Guid.NewGuid(),
                PayerMemberId = Guid.NewGuid(),
                Amount = 0m,
                SplitType = ExpenseSplitType.Equal,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = Guid.NewGuid() }
                ]
            };

            var (service, harness) = CreateService(DateTimeOffset.UtcNow);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
            harness.TransactionManager.Verify(
                x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenParticipantsEmpty_ThrowsValidationException()
        {
            var command = new CreateExpenseCommand
            {
                GroupId = Guid.NewGuid(),
                PayerMemberId = Guid.NewGuid(),
                Amount = 50m,
                SplitType = ExpenseSplitType.Equal,
                Participants = []
            };

            var (service, harness) = CreateService(DateTimeOffset.UtcNow);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
            harness.TransactionManager.Verify(
                x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicateParticipants_ThrowsInvalidSplitException()
        {
            var memberId = Guid.NewGuid();
            var command = new CreateExpenseCommand
            {
                GroupId = Guid.NewGuid(),
                PayerMemberId = memberId,
                Amount = 50m,
                SplitType = ExpenseSplitType.Equal,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = memberId },
                    new CreateExpenseParticipantCommand { MemberId = memberId }
                ]
            };

            var (service, harness) = CreateService(DateTimeOffset.UtcNow);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidSplitException>();
            harness.TransactionManager.Verify(
                x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenCustomSplitDoesNotSum_ThrowsInvalidSplitException()
        {
            var groupId = Guid.NewGuid();
            var payerId = Guid.NewGuid();
            var participantId = Guid.NewGuid();

            var command = new CreateExpenseCommand
            {
                GroupId = groupId,
                PayerMemberId = payerId,
                Amount = 100m,
                SplitType = ExpenseSplitType.Custom,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = payerId, ShareAmount = 70m },
                    new CreateExpenseParticipantCommand { MemberId = participantId, ShareAmount = 20m }
                ]
            };

            var members = new[]
            {
                new Member { Id = payerId, GroupId = groupId },
                new Member { Id = participantId, GroupId = groupId }
            };

            var (service, harness) = CreateService(DateTimeOffset.UtcNow);
            harness.GroupRepository
                .Setup(x => x.ExistsAsync(groupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            harness.MemberRepository
                .Setup(x => x.GetByIdsInGroupAsync(groupId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(members);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidSplitException>();
        }
    }

    public sealed class BusinessRuleFailures
    {
        [Fact]
        public async Task CreateAsync_WhenGroupMissing_ThrowsNotFoundException()
        {
            var command = CreateDefaultCommand();
            var (service, harness) = CreateService(DateTimeOffset.UtcNow);

            harness.GroupRepository
                .Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>();
            harness.ExpenseRepository.Verify(x => x.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Never);
            harness.ExpenseParticipantRepository.Verify(
                x => x.AddRangeAsync(It.IsAny<IReadOnlyCollection<ExpenseParticipant>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            harness.BalanceRepository.Verify(
                x => x.ApplyDeltasAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyDictionary<Guid, decimal>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WhenPayerNotInGroup_ThrowsForbiddenOperationException()
        {
            var groupId = Guid.NewGuid();
            var payerId = Guid.NewGuid();
            var participantId = Guid.NewGuid();

            var command = new CreateExpenseCommand
            {
                GroupId = groupId,
                PayerMemberId = payerId,
                Amount = 80m,
                SplitType = ExpenseSplitType.Equal,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = payerId },
                    new CreateExpenseParticipantCommand { MemberId = participantId }
                ]
            };

            var members = new[]
            {
                new Member { Id = participantId, GroupId = groupId }
            };

            var (service, harness) = CreateService(DateTimeOffset.UtcNow);
            harness.GroupRepository
                .Setup(x => x.ExistsAsync(groupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            harness.MemberRepository
                .Setup(x => x.GetByIdsInGroupAsync(groupId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(members);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenOperationException>();
        }

        [Fact]
        public async Task CreateAsync_WhenParticipantNotInGroup_ThrowsForbiddenOperationException()
        {
            var groupId = Guid.NewGuid();
            var payerId = Guid.NewGuid();
            var missingParticipantId = Guid.NewGuid();

            var command = new CreateExpenseCommand
            {
                GroupId = groupId,
                PayerMemberId = payerId,
                Amount = 80m,
                SplitType = ExpenseSplitType.Equal,
                Participants =
                [
                    new CreateExpenseParticipantCommand { MemberId = payerId },
                    new CreateExpenseParticipantCommand { MemberId = missingParticipantId }
                ]
            };

            var members = new[]
            {
                new Member { Id = payerId, GroupId = groupId }
            };

            var (service, harness) = CreateService(DateTimeOffset.UtcNow);
            harness.GroupRepository
                .Setup(x => x.ExistsAsync(groupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            harness.MemberRepository
                .Setup(x => x.GetByIdsInGroupAsync(groupId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(members);

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenOperationException>();
        }
    }

    public sealed class OrchestrationBehavior
    {
        [Fact]
        public async Task CreateAsync_WhenTransactionThrows_PropagatesException()
        {
            var command = CreateDefaultCommand();
            var (service, harness) = CreateService(DateTimeOffset.UtcNow);

            harness.TransactionManager
                .Setup(x => x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidSplitException("boom"));

            var act = () => service.CreateAsync(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidSplitException>();
        }
    }

    private static CreateExpenseCommand CreateDefaultCommand()
    {
        return new CreateExpenseCommand
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
    }

    private static (ExpenseService service, ServiceHarness harness) CreateService(DateTimeOffset now)
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

        clock.SetupGet(x => x.UtcNow).Returns(now);

        var service = new ExpenseService(
            expenseRepository.Object,
            groupRepository.Object,
            memberRepository.Object,
            expenseParticipantRepository.Object,
            balanceRepository.Object,
            transactionManager.Object,
            clock.Object,
            new ExpenseSplitCalculator(),
            new ExpenseParticipantValidator(),
            new BalanceDeltaCalculator());

        return (service, new ServiceHarness(
            expenseRepository,
            groupRepository,
            memberRepository,
            expenseParticipantRepository,
            balanceRepository,
            transactionManager));
    }

    private sealed record ServiceHarness(
        Mock<IExpenseRepository> ExpenseRepository,
        Mock<IGroupRepository> GroupRepository,
        Mock<IMemberRepository> MemberRepository,
        Mock<IExpenseParticipantRepository> ExpenseParticipantRepository,
        Mock<IBalanceRepository> BalanceRepository,
        Mock<ITransactionManager> TransactionManager);
}
