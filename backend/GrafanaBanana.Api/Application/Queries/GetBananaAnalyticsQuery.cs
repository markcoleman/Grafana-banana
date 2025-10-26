using GrafanaBanana.Api.Databricks;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to get banana analytics data
/// Following CQRS pattern - read operation
/// </summary>
public record GetBananaAnalyticsQuery : IRequest<BananaAnalytics>;
