namespace Nuggets.Domain.Entities;

public class UserCompany
{
    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}