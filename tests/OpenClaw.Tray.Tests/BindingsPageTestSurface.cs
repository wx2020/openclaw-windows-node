// Headless generated-control and dispatcher substitutes for the linked Bindings page.
// These tests exercise the page owner, not native rendering or dispatcher thread affinity.
namespace Microsoft.UI.Xaml
{
    public class Application
    {
        private static readonly AsyncLocal<Application?> CurrentApplication = new();
        public static Application? Current
        {
            get => CurrentApplication.Value;
            set => CurrentApplication.Value = value;
        }
    }

    public enum Visibility { Visible, Collapsed }
    public class RoutedEventArgs : EventArgs { }
}

namespace Microsoft.UI.Xaml.Controls
{
    public class Page
    {
        public event EventHandler<Microsoft.UI.Xaml.RoutedEventArgs>? Unloaded;
        public TestDispatcher DispatcherQueue { get; } = new();
        public void Unload() => Unloaded?.Invoke(this, new());
    }

    public sealed class TestDispatcher
    {
        private readonly Queue<Action> _callbacks = new();
        public bool TryEnqueue(Action callback)
        {
            _callbacks.Enqueue(callback);
            return true;
        }
        public void Drain()
        {
            while (_callbacks.TryDequeue(out var callback)) callback();
        }
    }

    public enum InfoBarSeverity { Warning, Error }
    public sealed class TestControl
    {
        public Microsoft.UI.Xaml.Visibility Visibility { get; set; }
        public object? ItemsSource { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsOpen { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public InfoBarSeverity Severity { get; set; }
    }
}

namespace OpenClawTray
{
    public class App : Microsoft.UI.Xaml.Application
    {
        internal Services.AppState AppState { get; } = new();
        public OpenClaw.Shared.IOperatorGatewayClient? GatewayClient { get; set; }
        public OpenClaw.Connection.GatewayConnectionManager? ConnectionManager => null;
    }
}

namespace OpenClawTray.Helpers
{
    /// <summary>
    /// Test stand-in for the WinUI resource helper. Resolves en-us values from the
    /// repository resource file so source-linked pages see the same copy the app ships,
    /// without a WinRT dependency. Unknown keys fall back to the key itself.
    /// </summary>
    public static class LocalizationHelper
    {
        private static readonly System.Collections.Generic.Dictionary<string, string> s_en = Load();

        public static string GetString(string key) =>
            s_en.TryGetValue(key, out var value) ? value : key;

        private static System.Collections.Generic.Dictionary<string, string> Load()
        {
            var map = new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);
            var dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                var candidate = Path.Combine(dir, "src", "OpenClaw.Tray.WinUI", "Strings", "en-us", "Resources.resw");
                if (File.Exists(candidate))
                {
                    var doc = System.Xml.Linq.XDocument.Load(candidate);
                    foreach (var data in doc.Root!.Elements("data"))
                        map[(string?)data.Attribute("name") ?? ""] = data.Element("value")?.Value ?? "";
                    break;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return map;
        }
    }
}

namespace OpenClawTray.Pages
{
    public sealed partial class BindingsPage
    {
        internal Microsoft.UI.Xaml.Controls.TestControl LoadingState { get; } = new();
        internal Microsoft.UI.Xaml.Controls.TestControl BindingsList { get; } = new();
        internal Microsoft.UI.Xaml.Controls.TestControl SingleAgentInfoBar { get; } = new();
        internal Microsoft.UI.Xaml.Controls.TestControl ConnectionInfoBar { get; } = new();
        internal Microsoft.UI.Xaml.Controls.TestControl RefreshButton { get; } = new();
        private void InitializeComponent() { }
    }
}
