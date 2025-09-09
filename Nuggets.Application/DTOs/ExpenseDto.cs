using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public record ExpenseCreateDto(
    string Description,
    ExpenseCategory Category,
    decimal Amount,
    DateTime ExpenseDate
);

public record ExpenseUpdateDto(
    string Description,
    ExpenseCategory Category,
    decimal Amount,
    DateTime ExpenseDate
);