using GrafanaBanana.Api.Databricks;
using MediatR;

namespace GrafanaBanana.Api.Application.Queries;

/// <summary>
/// Query to get banana sales data by region
/// Following CQRS pattern - read operation
/// </summary>
public record GetBananaSalesQuery(string Region) : IRequest<List<BananaSalesData>>;
