using AdmJudoTeam.Application.ViewModels;
using AdmJudoTeam.Core.Interfaces;
using AdmJudoTeam.Core.Services;
using AdmJudoTeam.Infrastructure.Data;
using AdmJudoTeam.Infrastructure.Device;
using AdmJudoTeam.Infrastructure.Reporting;
using AdmJudoTeam.Infrastructure.Repositories;
using AdmJudoTeam.Infrastructure.State;
using Microsoft.Extensions.Logging;

namespace AdmJudoTeam
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Persistencia local (SQLite offline-first).
            builder.Services.AddSingleton<JudoDatabase>();
            builder.Services.AddSingleton<IJudoRepository, JudoRepository>();

            // Estado de la aplicación: club activo (persistido en Preferences).
            builder.Services.AddSingleton<IClubContextService, ClubContextService>();

            // Dominio y hardware.
            builder.Services.AddSingleton<IJudoPeriodizationService, JudoPeriodizationService>();
            builder.Services.AddSingleton<ITimerHardwareService, TimerHardwareService>();
            builder.Services.AddSingleton<IPhotoStorageService, PhotoStorageService>();
            builder.Services.AddSingleton<IFileShareService, FileShareService>();
            builder.Services.AddSingleton<ITournamentReportService, TournamentReportService>();

            // ViewModels (MVVM estricto: las vistas .razor solo consumen estas clases).
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<ClubPanelViewModel>();
            builder.Services.AddTransient<ClubViewModel>();
            builder.Services.AddTransient<ClubSwitcherViewModel>();
            builder.Services.AddTransient<MembersViewModel>();
            builder.Services.AddTransient<TournamentRosterViewModel>();
            builder.Services.AddTransient<TatamiModeViewModel>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
