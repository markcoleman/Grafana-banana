using GrafanaBanana.Api.Databricks;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to get banana production data by year
/// Following CQRS pattern - read operation
/// </summary>
public record GetBananaProductionQuery(int Year) : IRequest<List<BananaProduction>>;
