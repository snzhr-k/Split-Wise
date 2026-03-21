using FairSplit.Api.Controllers;
using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Presentation.Models.Requests;
using FairSplit.Api.Presentation.Models.Responses;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FairSplit.LayerIsolationTests.Controllers;

public sealed class ExpensesControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreated_WithResponseFromService()
    {
        var groupId = Guid.NewGuid();
        var payerId = Guid.NewGuid();
        var participantId = Guid.NewGuid();

        var service = new Mock<IExpenseService>();
        service
            .Setup(x => x.CreateAsync(It.IsAny<CreateExpenseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Expense
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                PayerMemberId = payerId,
                Amount = 120m,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });

        var controller = new ExpensesController(service.Object);

        var request = new ExpenseRequest
        {
            PayerMemberId = payerId,
            Amount = 120m,
            SplitType = "equal",
            Participants =
            [
                new ExpenseParticipantShareRequest { MemberId = participantId }
            ]
        };

        var actionResult = await controller.Create(groupId, request, CancellationToken.None);
        var createdResult = Assert.IsType<CreatedResult>(actionResult.Result);
        var response = Assert.IsType<ExpenseResponse>(createdResult.Value);

        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(groupId, response.GroupId);
        Assert.Equal(120m, response.Amount);

        service.Verify(x => x.CreateAsync(It.IsAny<CreateExpenseCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
