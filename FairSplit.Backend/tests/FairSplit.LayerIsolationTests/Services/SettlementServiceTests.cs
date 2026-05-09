using FairSplit.Api.Repositories.Interfaces;
using FairSplit.Api.Services.Implementations;
using FluentAssertions;
using Moq;

namespace FairSplit.LayerIsolationTests.Services;

public sealed class SettlementServiceTests
{
    [Fact]
    public async Task HandlePlaceholderAsync_CompletesWithoutCallingRepository()
    {
        var repository = new Mock<ISettlementRepository>();
        var service = new SettlementService(repository.Object);

        await service.Invoking(x => x.HandlePlaceholderAsync(CancellationToken.None))
            .Should().NotThrowAsync();

        repository.VerifyNoOtherCalls();
    }
}
