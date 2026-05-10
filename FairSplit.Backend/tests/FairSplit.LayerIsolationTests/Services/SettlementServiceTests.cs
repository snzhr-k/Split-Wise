using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Errors;
using FairSplit.Api.Services.Implementations;
using FairSplit.Api.Shared.Utilities;
using FluentAssertions;
using Moq;

namespace FairSplit.LayerIsolationTests.Services;

public sealed class SettlementServiceTests
{
    [Fact]
    public async Task GetByGroupIdAsync_WhenGroupDoesNotExist_ThrowsNotFound()
    {
        var settlementRepository = new Mock<ISettlementRepository>();
        var groupRepository = new Mock<IGroupRepository>();
        var memberRepository = new Mock<IMemberRepository>();
        var balanceRepository = new Mock<IBalanceRepository>();
        var transactionManager = new Mock<ITransactionManager>();
        var clock = new Mock<IClock>();

        groupRepository
            .Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new SettlementService(
            settlementRepository.Object,
            groupRepository.Object,
            memberRepository.Object,
            balanceRepository.Object,
            transactionManager.Object,
            clock.Object);

        await service
            .Invoking(x => x.GetByGroupIdAsync(Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();

        settlementRepository.Verify(
            x => x.GetByGroupIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
