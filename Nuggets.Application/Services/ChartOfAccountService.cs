// ChartOfAccountService.cs
using Nuggets.Application.Common;
using Nuggets.Application.Common.Interfaces;
using Nuggets.Application.Common.Services;
using Nuggets.Application.DTOs;
using Nuggets.Domain.Entities;

namespace Nuggets.Application.Services;

public sealed class ChartOfAccountService(IChartOfAccountRepository repo) : IChartOfAccountService
{
    public async Task<Result<PagedResult<ChartOfAccountListDto>>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await repo.GetPagedAsync(page, pageSize);
        var list = items.Select(ToListDto).ToList();
        return Result<PagedResult<ChartOfAccountListDto>>.Ok(new PagedResult<ChartOfAccountListDto>(list, totalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<ChartOfAccountListDto>>> GetAllAsync()
    {
        var list = await repo.GetAllAsync();
        return Result<IReadOnlyList<ChartOfAccountListDto>>.Ok(list.Select(ToListDto).ToList());
    }

    public async Task<Result<ChartOfAccountReadDto>> GetByIdAsync(Guid id)
    {
        var entity = await repo.GetByIdAsync(id);
        return entity is not null
            ? Result<ChartOfAccountReadDto>.Ok(ToReadDto(entity))
            : Result<ChartOfAccountReadDto>.Err("Account not found", "NOT_FOUND");
    }

    public async Task<Result<ChartOfAccountReadDto>> CreateAsync(ChartOfAccountCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return Result<ChartOfAccountReadDto>.Err("Code and Name are required", "VALIDATION_ERROR");

        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var entity = new ChartOfAccount
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Type = dto.Type
            };

            await repo.AddAsync(entity);
            await tx.CommitAsync();
            return Result<ChartOfAccountReadDto>.Ok(ToReadDto(entity));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<ChartOfAccountReadDto>.Err($"Failed to create account: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<ChartOfAccountReadDto>> UpdateAsync(Guid id, ChartOfAccountUpdateDto dto)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<ChartOfAccountReadDto>.Err("Account not found", "NOT_FOUND");

            existing.Code = dto.Code.Trim();
            existing.Name = dto.Name.Trim();
            existing.Type = dto.Type;

            await repo.UpdateAsync(existing);
            await tx.CommitAsync();
            return Result<ChartOfAccountReadDto>.Ok(ToReadDto(existing));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<ChartOfAccountReadDto>.Err($"Failed to update account: {ex.Message}", "DB_ERROR");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        await using var tx = await repo.BeginTransactionAsync();
        try
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null)
                return Result<bool>.Err("Account not found", "NOT_FOUND");

            await repo.DeleteAsync(existing);
            await tx.CommitAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return Result<bool>.Err($"Failed to delete account: {ex.Message}", "DB_ERROR");
        }
    }

    private static ChartOfAccountListDto ToListDto(ChartOfAccount a) =>
        new(a.Id, a.Code, a.Name, a.Type.ToString());

    private static ChartOfAccountReadDto ToReadDto(ChartOfAccount a) =>
        new(a.Id, a.Code, a.Name, a.Type.ToString());
}
