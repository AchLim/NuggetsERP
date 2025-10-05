using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record CustomerPaymentListDto(Guid Id, Guid CustomerInvoiceId, string CustomerInvoiceNumber, Guid CustomerId, string? CustomerName, DateTime PaymentDate, decimal Amount, string Status, CustomerPaymentMethod Method, string PaymentNumber);
public sealed record CustomerPaymentReadDto(Guid Id, Guid CustomerInvoiceId, string CustomerInvoiceNumber, Guid CustomerId, string? CustomerName, DateTime PaymentDate, decimal Amount, string Status, CustomerPaymentMethod Method, string PaymentNumber);
public sealed record CustomerPaymentCreateDto(Guid CustomerInvoiceId, DateTime PaymentDate, decimal Amount, CustomerPaymentMethod Method);
public sealed record CustomerPaymentUpdateDto(DateTime PaymentDate, CustomerPaymentStatus Status, decimal Amount, CustomerPaymentMethod Method);