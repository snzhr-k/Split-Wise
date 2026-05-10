using FairSplit.Api.Infrastructure.Http;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Services.Business;
using FairSplit.Api.Services.Implementations;
using FairSplit.Api.Services.Interfaces;
using FairSplit.Api.Shared.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Global model validation filter converts ModelState errors to ValidationException
    options.Filters.Add<FairSplit.Api.Presentation.Filters.ValidateModelAttribute>();
})
.ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IExpenseParticipantService, ExpenseParticipantService>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();

builder.Services.AddSingleton<IExpenseSplitCalculator, ExpenseSplitCalculator>();
builder.Services.AddSingleton<IExpenseParticipantValidator, ExpenseParticipantValidator>();
builder.Services.AddSingleton<IBalanceDeltaCalculator, BalanceDeltaCalculator>();

builder.Services.AddScoped<IClock, SystemClock>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
