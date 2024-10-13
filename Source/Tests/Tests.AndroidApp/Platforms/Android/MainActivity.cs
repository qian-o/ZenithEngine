using Android.App;
using Android.Content.PM;

#pragma warning disable IDE0130
namespace Tests.AndroidApp;
#pragma warning restore IDE0130

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[MetaData("com.android.graphics.injectLayers.enable", Value = "true")]
public class MainActivity : MauiAppCompatActivity
{
}
