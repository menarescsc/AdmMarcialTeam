using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;

namespace AdmJudoTeam
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Android WebView always reports 0 for CSS env(safe-area-inset-*), so
            // the Blazor layout cannot pad itself away from the system bars.
            // Pad the native decor view with the real window insets instead.
            var decorView = Window?.DecorView;
            if (decorView is null)
            {
                return;
            }

            // Matches --maui-status-bar-safe-area-background (app.css) so the
            // strip behind the status bar blends with the app's indigo top bar.
            decorView.SetBackgroundColor(Android.Graphics.Color.Rgb(79, 70, 229));
            var insetsController = WindowCompat.GetInsetsController(Window!, decorView);
            if (insetsController is not null)
            {
                insetsController.AppearanceLightStatusBars = false;
            }

            ViewCompat.SetOnApplyWindowInsetsListener(decorView, new SystemBarsInsetsListener());
            ViewCompat.RequestApplyInsets(decorView);
        }

        private sealed class SystemBarsInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat? OnApplyWindowInsets(Android.Views.View? view, WindowInsetsCompat? insets)
            {
                if (view is null || insets is null)
                {
                    return insets;
                }

                var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                if (systemBars is null)
                {
                    return insets;
                }

                view.SetPadding(systemBars.Left, systemBars.Top, systemBars.Right, 0);
                return insets;
            }
        }
    }
}
