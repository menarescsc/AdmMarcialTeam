namespace AdmJudoTeam.Core.Enums;

/// <summary>
/// Categorías de peso oficiales IJF (sénior). El límite en kg se resuelve
/// vía <see cref="Models.WeightCategoryInfo"/>.
/// </summary>
public enum WeightCategory
{
    MenUnder60 = 0,
    MenUnder66 = 1,
    MenUnder73 = 2,
    MenUnder81 = 3,
    MenUnder90 = 4,
    MenUnder100 = 5,
    MenOver100 = 6,

    WomenUnder48 = 10,
    WomenUnder52 = 11,
    WomenUnder57 = 12,
    WomenUnder63 = 13,
    WomenUnder70 = 14,
    WomenUnder78 = 15,
    WomenOver78 = 16
}
