using CafeSphere.Application.DTOs;
using CafeSphere.Application.Features.Catalog;
using CafeSphere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeSphere.API.Controllers;

[Authorize]
[Route("api/v1/reservations")]
public class ReservationsController : BaseApiController
{
    /// <summary>
    /// Retrieve all active table reservations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations()
    {
        var result = await Mediator.Send(new Application.Features.Reservations.GetReservationsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Book a table reservation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var command = new Application.Features.Reservations.CreateReservationCommand(
            request.CustomerName,
            request.CustomerPhone,
            request.CustomerEmail,
            request.TableId,
            request.TableId.StartsWith("T-") ? request.TableId : $"T-{request.TableId}",
            request.PartySize,
            request.ReservationTime,
            request.SpecialNotes
        );

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Update reservation status.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateReservationStatusRequest request)
    {
        var command = new Application.Features.Reservations.UpdateReservationStatusCommand(id, request.Status);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record UpdateReservationStatusRequest(Domain.Enums.ReservationStatus Status);

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager},{Roles.Cashier}")]
[Route("api/v1/customers")]
public class CustomersController : BaseApiController
{
    /// <summary>
    /// Retrieve customer directory.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers()
    {
        var result = await Mediator.Send(new Application.Features.Catalog.GetCustomersQuery());
        return HandleResult(result);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/employees")]
public class EmployeesController : BaseApiController
{
    /// <summary>
    /// List employee records.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees()
    {
        var result = await Mediator.Send(new Application.Features.Catalog.GetEmployeesQuery());
        return HandleResult(result);
    }
}

[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Manager}")]
[Route("api/v1/finance")]
public class FinanceController : BaseApiController
{
    /// <summary>
    /// Retrieve company expenses list.
    /// </summary>
    [HttpGet("expenses")]
    [ProducesResponseType(typeof(List<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenses()
    {
        var result = await Mediator.Send(new Application.Features.Catalog.GetExpensesQuery());
        return HandleResult(result);
    }
}

[Authorize]
[Route("api/v1/ai")]
public class AIController : BaseApiController
{
    /// <summary>
    /// Query CafeSphere AI Assistant for real-time sales recommendations & stock predictions using MongoDB data context.
    /// </summary>
    [HttpPost("recommendations")]
    [ProducesResponseType(typeof(AIRecommendationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAIRecommendations([FromBody] AIRecommendationRequest request)
    {
        var result = await Mediator.Send(new GetAIRecommendationsQuery());
        return HandleResult(result);
    }
}
