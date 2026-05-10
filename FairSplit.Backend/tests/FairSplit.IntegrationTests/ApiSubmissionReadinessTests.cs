using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FairSplit.IntegrationTests;

public sealed class ApiSubmissionReadinessTests
{
    [Fact]
    public async Task Protected_Endpoint_Without_BearerToken_Returns401()
    {
        using var factory = new IntegrationTestFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync($"/api/groups/{Guid.NewGuid()}/expenses");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Protected_Endpoint_With_Invalid_BearerToken_Returns401()
    {
        using var factory = new IntegrationTestFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token-value");

        var response = await client.GetAsync($"/api/groups/{Guid.NewGuid()}/balances");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Group_Create_Get_Join_And_Members_List_Flow_Works()
    {
        using var factory = new IntegrationTestFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        await AuthorizeClientAsync(client, Guid.NewGuid(), "Alice");

        var createGroupResponse = await client.PostAsJsonAsync("/api/groups", new { name = "Trip Team" });
        createGroupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var group = await ParseJsonObjectAsync(createGroupResponse);
        var groupId = group.GetProperty("id").GetGuid();

        var getGroupResponse = await client.GetAsync($"/api/groups/{groupId}");
        getGroupResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var joinResponse = await client.PostAsJsonAsync($"/api/groups/{groupId}/join", new
        {
            displayName = "Bob"
        });

        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var membersResponse = await client.GetAsync($"/api/groups/{groupId}/members");
        membersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var members = await ParseJsonArrayAsync(membersResponse);
        members.Should().HaveCount(1);
        members[0].GetProperty("displayName").GetString().Should().Be("Bob");
    }

    [Fact]
    public async Task Expense_Create_List_Details_And_Balances_Flow_Works()
    {
        using var factory = new IntegrationTestFactory();
        var (groupId, firstMemberId, secondMemberId) = await SeedGroupWithTwoMembersAsync(factory);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        await AuthorizeClientAsync(client, firstMemberId, "Alice");

        var createExpenseResponse = await client.PostAsJsonAsync($"/api/groups/{groupId}/expenses", new
        {
            payerMemberId = firstMemberId,
            amount = 100.00m,
            splitType = "equal",
            participants = new[]
            {
                new { memberId = firstMemberId },
                new { memberId = secondMemberId }
            }
        });

        createExpenseResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdExpense = await ParseJsonObjectAsync(createExpenseResponse);
        var expenseId = createdExpense.GetProperty("id").GetGuid();

        var listExpensesResponse = await client.GetAsync($"/api/groups/{groupId}/expenses");
        listExpensesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var expenses = await ParseJsonArrayAsync(listExpensesResponse);
        expenses.Should().HaveCount(1);

        var getExpenseResponse = await client.GetAsync($"/api/groups/{groupId}/expenses/{expenseId}");
        getExpenseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var expenseDetails = await ParseJsonObjectAsync(getExpenseResponse);
        expenseDetails.GetProperty("participants").GetArrayLength().Should().Be(2);

        var balancesResponse = await client.GetAsync($"/api/groups/{groupId}/balances");
        balancesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var balances = await ParseJsonArrayAsync(balancesResponse);
        balances.Should().HaveCount(2);

        balances.Single(item => item.GetProperty("memberId").GetGuid() == firstMemberId)
            .GetProperty("netAmount").GetDecimal().Should().Be(50.00m);

        balances.Single(item => item.GetProperty("memberId").GetGuid() == secondMemberId)
            .GetProperty("netAmount").GetDecimal().Should().Be(-50.00m);

        var invalidMemberExpenseResponse = await client.PostAsJsonAsync($"/api/groups/{groupId}/expenses", new
        {
            payerMemberId = Guid.NewGuid(),
            amount = 10.00m,
            splitType = "equal",
            participants = new[]
            {
                new { memberId = firstMemberId },
                new { memberId = secondMemberId }
            }
        });

        invalidMemberExpenseResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Settlement_Create_List_Details_And_Balance_Effect_Works()
    {
        using var factory = new IntegrationTestFactory();
        var (groupId, firstMemberId, secondMemberId) = await SeedGroupWithTwoMembersAsync(factory);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        await AuthorizeClientAsync(client, firstMemberId, "Alice");

        var createExpenseResponse = await client.PostAsJsonAsync($"/api/groups/{groupId}/expenses", new
        {
            payerMemberId = firstMemberId,
            amount = 120.00m,
            splitType = "equal",
            participants = new[]
            {
                new { memberId = firstMemberId },
                new { memberId = secondMemberId }
            }
        });

        createExpenseResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createSettlementResponse = await client.PostAsJsonAsync($"/api/groups/{groupId}/settlements", new
        {
            fromMemberId = secondMemberId,
            toMemberId = firstMemberId,
            amount = 20.00m
        });

        createSettlementResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var settlement = await ParseJsonObjectAsync(createSettlementResponse);
        var settlementId = settlement.GetProperty("id").GetGuid();

        var listSettlementsResponse = await client.GetAsync($"/api/groups/{groupId}/settlements");
        listSettlementsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var settlements = await ParseJsonArrayAsync(listSettlementsResponse);
        settlements.Should().HaveCount(1);

        var getSettlementResponse = await client.GetAsync($"/api/groups/{groupId}/settlements/{settlementId}");
        getSettlementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getMemberBalanceResponse = await client.GetAsync($"/api/groups/{groupId}/balances/{firstMemberId}");
        getMemberBalanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var memberBalance = await ParseJsonObjectAsync(getMemberBalanceResponse);
        memberBalance.GetProperty("netAmount").GetDecimal().Should().Be(40.00m);

        var outsiderSettlementResponse = await client.PostAsJsonAsync($"/api/groups/{groupId}/settlements", new
        {
            fromMemberId = Guid.NewGuid(),
            toMemberId = firstMemberId,
            amount = 10.00m
        });

        outsiderSettlementResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task AuthorizeClientAsync(HttpClient client, Guid memberId, string displayName)
    {
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/dev-token", new
        {
            memberId,
            displayName
        });

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokenPayload = await ParseJsonObjectAsync(tokenResponse);
        var accessToken = tokenPayload.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static async Task<(Guid GroupId, Guid FirstMemberId, Guid SecondMemberId)> SeedGroupWithTwoMembersAsync(
        IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FairSplitDbContext>();

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Seeded Group"
        };

        var firstMember = new Member
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            DisplayName = "Alice"
        };

        var secondMember = new Member
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            DisplayName = "Bob"
        };

        dbContext.Groups.Add(group);
        dbContext.Members.AddRange(firstMember, secondMember);
        await dbContext.SaveChangesAsync();

        return (group.Id, firstMember.Id, secondMember.Id);
    }

    private static async Task<JsonElement> ParseJsonObjectAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }

    private static async Task<JsonElement[]> ParseJsonArrayAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        return document.RootElement
            .EnumerateArray()
            .Select(item => item.Clone())
            .ToArray();
    }
}
