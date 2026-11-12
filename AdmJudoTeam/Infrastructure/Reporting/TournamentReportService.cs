using AdmJudoTeam.Core.Entities;
using AdmJudoTeam.Core.Interfaces;
using AdmJudoTeam.Core.Models;
using Microsoft.Maui.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AdmJudoTeam.Infrastructure.Reporting;

/// <summary>
/// Genera la nómina oficial de pesaje en PDF con QuestPDF: encabezado del club
/// y torneo, tabla de inscritos con peso inscrito, peso actual y diferencia
/// contra el límite IJF, y objetivos tácticos. El PDF se guarda como archivo
/// local en AppDataDirectory/Reports.
/// </summary>
public sealed class TournamentReportService : ITournamentReportService
{
    private readonly IJudoRepository _repository;

    static TournamentReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public TournamentReportService(IJudoRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> GenerateRosterReportAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        var tournament = await _repository.GetTournamentAsync(tournamentId).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Torneo no encontrado.");

        var entries = await _repository.GetTournamentEntriesAsync(tournamentId).ConfigureAwait(false);
        var members = await _repository.GetMembersAsync(tournament.ClubId, activeOnly: false).ConfigureAwait(false);
        var clubName = (await _repository.GetClubAsync(tournament.ClubId).ConfigureAwait(false))?.Name ?? "Club de Judo";

        var rows = entries
            .Select(entry => new ReportRow(
                members.FirstOrDefault(member => member.Id == entry.MemberId),
                entry))
            .Where(row => row.Member is not null)
            .OrderBy(row => row.Member!.LastName)
            .ThenBy(row => row.Member!.FirstName)
            .ToList();

        var reportsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Reports");
        Directory.CreateDirectory(reportsDirectory);
        var filePath = Path.Combine(
            reportsDirectory,
            $"nomina_{tournamentId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

        QuestPDF.Fluent.Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("NÓMINA OFICIAL DE PESAJE").FontSize(16).Bold();
                    column.Item().Text(tournament.Name).FontSize(12).SemiBold();
                    column.Item().PaddingTop(4).Text($"{clubName} · Ubicación: {tournament.Location ?? "—"}");
                    column.Item().Text(
                        $"Inicio: {tournament.StartDate:dd/MM/yyyy} · Pesaje oficial: {tournament.WeighInDate:dd/MM/yyyy}");
                });

                page.Content().PaddingVertical(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("#").Bold();
                        header.Cell().Text("Atleta").Bold();
                        header.Cell().Text("Categoría").Bold();
                        header.Cell().Text("Peso inscrito (kg)").Bold();
                        header.Cell().Text("Peso actual (kg)").Bold();
                        header.Cell().Text("Objetivos tácticos").Bold();
                    });

                    var index = 1;
                    foreach (var row in rows)
                    {
                        var member = row.Member!;
                        table.Cell().Text(index.ToString());
                        table.Cell().Text($"{member.LastName}, {member.FirstName}");
                        table.Cell().Text(WeightCategoryInfo.GetLabel(member.WeightCategory));
                        table.Cell().Text(row.Entry.RegisteredWeightKg.ToString("0.0"));
                        table.Cell().Text(
                            member.ActualWeightKg is null ? "—" : member.ActualWeightKg.Value.ToString("0.0"));
                        table.Cell().Text(row.Entry.TacticalGoals ?? "—");
                        index++;
                    }
                });

                page.Footer().AlignRight().Text(
                    $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} · {clubName}").FontSize(8);
            });
        }).GeneratePdf(filePath);

        return filePath;
    }

    private sealed record ReportRow(Member? Member, TournamentEntry Entry);
}
