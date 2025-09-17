using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuggets.Domain.Entities;

[Table("journal_entry")]
public sealed class JournalEntry : BaseEntity
{
    [MaxLength(100)] public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }

    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

    public ICollection<JournalItem> Items { get; set; } = new List<JournalItem>();
}

public enum JournalEntryStatus
{
    Draft = 1,
    Posted = 2,
    Cancelled = 9
}