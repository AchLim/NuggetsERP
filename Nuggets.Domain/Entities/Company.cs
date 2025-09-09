using System.ComponentModel.DataAnnotations;

namespace Nuggets.Domain.Entities;

public class Company : BaseEntity
{
    // 🏢 Basic Info
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LegalName { get; set; }   // For legal/tax documents
    
    [MaxLength(50)]
    public string? RegistrationNumber { get; set; } // Nomor Induk Badan Usaha (NIB) / SIUP

    // 📜 Tax Info (Indonesia specific)
    [MaxLength(20)]
    public string? NPWP { get; set; } // Nomor Pokok Wajib Pajak

    public bool PKP { get; set; } = false; // Pengusaha Kena Pajak (VAT Registered)

    [MaxLength(50)]
    public string? KLU { get; set; } // Klasifikasi Lapangan Usaha

    // 🏠 Address
    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string Country { get; set; } = "Indonesia"; // fixed default

    // 📞 Contact
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    // 🧾 Finance/Banking (for invoices/payroll)
    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(50)]
    public string? BankAccountNumber { get; set; }

    [MaxLength(100)]
    public string? BankAccountHolder { get; set; }

    // 🔗 Relationships
    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
}
