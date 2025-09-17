using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record VendorPaymentListDto(Guid Id, Guid VendorId, string? VendorName, DateTime PaymentDate, decimal Amount, VendorPaymentMethod Method);
public sealed record VendorPaymentReadDto(Guid Id, Guid VendorId, string? VendorName, Guid BillId, DateTime PaymentDate, decimal Amount, VendorPaymentMethod Method);
public sealed record VendorPaymentCreateDto(Guid VendorId, Guid BillId, DateTime PaymentDate, decimal Amount, Guid AccountId, VendorPaymentMethod Method);
public sealed record VendorPaymentUpdateDto(DateTime PaymentDate, decimal Amount, VendorPaymentStatus Status, Guid AccountId, VendorPaymentMethod Method);