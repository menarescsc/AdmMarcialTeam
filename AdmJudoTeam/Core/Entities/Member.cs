using AdmJudoTeam.Core.Enums;
using SQLite;

namespace AdmJudoTeam.Core.Entities;

/// <summary>
/// Ficha de atleta (judoka): grado, categoría de peso IJF, foto local y ficha médica de emergencia.
/// La foto se guarda en disco; en SQLite solo se persiste la ruta.
/// </summary>
[Table("Members")]
public class Member
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ClubId { get; set; }

    [NotNull, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [NotNull, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    public BeltGrade BeltGrade { get; set; }

    public WeightCategory WeightCategory { get; set; }

    /// <summary>Último peso registrado en kg.</summary>
    public double? ActualWeightKg { get; set; }

    /// <summary>Ruta física de la foto de perfil (nunca Base64 en la base).</summary>
    [MaxLength(300)]
    public string? PhotoFilePath { get; set; }

    [MaxLength(200)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(40)]
    public string? EmergencyContactPhone { get; set; }

    /// <summary>Ficha médica de emergencia (alergias, lesiones, medicación).</summary>
    [MaxLength(1000)]
    public string? MedicalNotes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
