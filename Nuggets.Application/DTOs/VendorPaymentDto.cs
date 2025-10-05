using Nuggets.Domain.Entities;

namespace Nuggets.Application.DTOs;

public sealed record VendorPaymentListDto(Guid Id, Guid VendorId, string? VendorName, Guid VendorBillId, string BillNumber, string? PaymentNumber, DateTime PaymentDate, decimal Amount, VendorPaymentMethod Method, string Status);
public sealed record VendorPaymentReadDto(Guid Id, Guid VendorId, string? VendorName, Guid VendorBillId, string BillNumber, string? PaymentNumber, DateTime PaymentDate, decimal Amount, VendorPaymentMethod Method, string Status);
public sealed record VendorPaymentCreateDto(Guid VendorId, Guid VendorBillId, DateTime PaymentDate, decimal Amount, Guid AccountId, VendorPaymentMethod Method);
public sealed record VendorPaymentUpdateDto(DateTime PaymentDate, decimal Amount, Guid AccountId, VendorPaymentMethod Method, VendorPaymentStatus Status);