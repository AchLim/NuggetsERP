using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class VendorService(IVendorRepository repo) : IVendorService
{
    public async Task<Result<PagedResult<Vendor>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        return Result<PagedResult<Vendor>>.Ok(new PagedResult<Vendor>(items, totalCount, page, pageSize));
    }

    public async Task<Result<Vendor>> CreateAsync(VendorCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<Vendor>.Err("Vendor name is required!", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var vendor = new Vendor
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
            };

            await repo.AddAsync(vendor);
            await tx.CommitAsync();
            return Result<Vendor>.Ok(vendor);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<Vendor>.Err($"Failed to create vendor: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<IReadOnlyList<Vendor>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<Vendor>>.Ok(list);
    }

    public async Task<Result<Vendor>> GetByIdAsync(Guid id)
    {
        var vendor = await repo.GetByIdAsync(id);
        return vendor is not null
            ? Result<Vendor>.Ok(vendor)
            : Result<Vendor>.Err("Vendor not found", "NOT_FOUND");
    }

    public async Task<Result<Vendor>> UpdateAsync(Guid id, VendorUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<Vendor>.Err("Vendor not found", "NOT_FOUND");

            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.Address = dto.Address;
            existing.Phone = dto.Phone;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<Vendor>.Ok(existing);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<Vendor>.Err($"Failed to update vendor: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Vendor not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete vendor: {ex.Message}", "DB_ERROR");
        }
    }
}