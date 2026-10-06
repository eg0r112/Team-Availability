using Asp.Versioning.Builder;
using MediatR;
using TeamAvailability.Application.Features.Absences.CreateAbsence;
using TeamAvailability.Application.Features.Auth.Login;
using TeamAvailability.Application.Features.Auth.Register;
using TeamAvailability.Application.Features.Availability.GetTeamAvailability;
using TeamAvailability.Application.Features.Integrations.Jira;
using TeamAvailability.Application.Features.Integrations.Telegram;
using TeamAvailability.Application.Features.OnCall.CreateShift;
using TeamAvailability.Application.Features.OnCall.GetShifts;
using TeamAvailability.Application.Features.Teams.AddMember;
using TeamAvailability.Application.Features.Teams.CreateTeam;
using TeamAvailability.Application.Features.Teams.GetTeam;
using TeamAvailability.Application.Features.Teams.ListTeams;
using TeamAvailability.Application.Features.Users.LinkTelegram;
using TeamAvailability.Contracts.Users;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Absences;
using TeamAvailability.Contracts.Auth;
using TeamAvailability.Contracts.Integrations;
using TeamAvailability.Contracts.OnCall;
using TeamAvailability.Contracts.Teams;

namespace TeamAvailability.Extensions;

public static class EndpointExtensions
{
    public static RouteGroupBuilder MapApiEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var api = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1.0)
            .WithTags("Team Availability API");

        api.MapPost("/auth/register", async (RegisterRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RegisterCommand(request.Email, request.Password, request.DisplayName), ct);
            return Results.Ok(result);
        }).AllowAnonymous().WithName("Register");

        api.MapPost("/auth/login", async (LoginRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
            return Results.Ok(result);
        }).AllowAnonymous().WithName("Login");

        api.MapPut("/me/telegram", async (LinkTelegramRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new LinkTelegramCommand(request.ChatId), ct);
            return Results.NoContent();
        }).RequireAuthorization();

        api.MapGet("/teams", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListTeamsQuery(), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        api.MapPost("/teams", async (CreateTeamRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateTeamCommand(request.Name, request.Description), ct);
            return Results.Created($"/api/v1/teams/{result.Id}", result);
        }).RequireAuthorization("TeamLeadOrAdmin");

        api.MapGet("/teams/{teamId:guid}", async (Guid teamId, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTeamQuery(teamId), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        api.MapPost("/teams/{teamId:guid}/members", async (Guid teamId, AddTeamMemberRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new AddTeamMemberCommand(teamId, request.UserId, request.TimeZoneId, request.Role), ct);
            return Results.Ok(result);
        }).RequireAuthorization("TeamLeadOrAdmin");

        api.MapPost("/teams/{teamId:guid}/absences", async (Guid teamId, CreateAbsenceRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateAbsenceCommand(
                teamId, request.TeamMemberId, request.Type, request.StartDate, request.EndDate, request.ReplacementMemberId, request.Notes), ct);
            return Results.Created($"/api/v1/teams/{teamId}/absences/{result.Id}", result);
        }).RequireAuthorization("TeamLeadOrAdmin");

        api.MapGet("/teams/{teamId:guid}/availability", async (Guid teamId, DateOnly? date, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTeamAvailabilityQuery(teamId, date), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        api.MapPost("/teams/{teamId:guid}/on-call", async (Guid teamId, CreateOnCallShiftRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new CreateOnCallShiftCommand(teamId, request.TeamMemberId, request.StartUtc, request.EndUtc), ct);
            return Results.Created($"/api/v1/teams/{teamId}/on-call/{result.Id}", result);
        }).RequireAuthorization("TeamLeadOrAdmin");

        api.MapGet("/teams/{teamId:guid}/on-call", async (Guid teamId, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetOnCallShiftsQuery(teamId), ct);
            return Results.Ok(result);
        }).RequireAuthorization();

        api.MapGet("/integrations/jira/teams/{teamId:guid}/assignable", async (
            Guid teamId,
            DateOnly? date,
            HttpContext httpContext,
            IMediator mediator,
            CancellationToken ct) =>
        {
            if (httpContext.Items["ApiKey"] is null && !(httpContext.User.Identity?.IsAuthenticated ?? false))
                return Results.Unauthorized();

            var result = await mediator.Send(new GetJiraAssignableQuery(teamId, date), ct);
            return Results.Ok(result);
        }).AllowAnonymous();

        api.MapPost("/integrations/telegram/webhook", async (
            TelegramWebhookRequest request,
            HttpContext httpContext,
            IMediator mediator,
            IIdempotencyStore idempotencyStore,
            CancellationToken ct) =>
        {
            var update = request.Update;
            if (update.Message is null)
                return Results.Ok();

            var idempotencyKey = $"tg:{update.UpdateId}";
            var requestHash = update.UpdateId.ToString();
            var cached = await idempotencyStore.GetAsync(idempotencyKey, ct);
            if (cached is not null)
            {
                httpContext.Response.StatusCode = cached.StatusCode;
                return Results.Content(cached.ResponseBody, "application/json");
            }

            var reply = await mediator.Send(new ProcessTelegramUpdateCommand(
                update.Message.Chat.Id,
                update.Message.From.Id,
                update.Message.Text), ct);

            var body = $"{{\"ok\":true,\"reply\":\"{reply.Replace("\"", "\\\"")}\"}}";
            await idempotencyStore.SaveAsync(idempotencyKey, requestHash, StatusCodes.Status200OK, body, ct);
            return Results.Text(body, "application/json");
        }).AllowAnonymous();

        return api;
    }
}
