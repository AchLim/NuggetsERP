namespace Nuggets.Application.Common.Interfaces;

public interface IUserContext
{
    Guid UserId { get; }
    IEnumerable<Guid> CompanyIds { get; }        // companies user belongs to
    IEnumerable<Guid> ActiveCompanyIds { get; }  // companies user selected in UI
}