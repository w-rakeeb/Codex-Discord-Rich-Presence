using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace CodexPresence;

public partial class MainWindow
{
    private readonly ReleaseUpdates releaseUpdates = new();
    private UpdateResult? lastUpdateResult;
    private bool checkingUpdates;
    private TextBlock? updateStatus;
    private Button? updateCheckButton;
    private Button? updateReleaseButton;

    private void RenderUpdates()
    {
        var card = Card("APP UPDATES", "Check public GitHub releases. Downloads and installation start only when you choose them.");
        Row(card, "Installed version", typeof(MainWindow).Assembly.GetName().Version!.ToString(3));
        var status = Text(lastUpdateResult?.Message ?? "Check for a new version.", 13, Muted);
        AutomationProperties.SetName(status, "Update status");
        AutomationProperties.SetLiveSetting(status, AutomationLiveSetting.Polite);
        var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
        var check = new Button { Content = checkingUpdates ? "Checking…" : "Check for updates", IsEnabled = !checkingUpdates, Margin = new Thickness(0, 0, 8, 0) };
        AutomationProperties.SetName(check, "Check for updates");
        var release = ActionButton(lastUpdateResult?.Available == true ? "View update" : "Open releases", () => OpenPath(lastUpdateResult?.Url ?? ReleaseUpdates.ProjectUrl + "/releases"));
        updateStatus = status;
        updateCheckButton = check;
        updateReleaseButton = release;
        check.Click += async (_, _) =>
        {
            if (checkingUpdates) return;
            checkingUpdates = true;
            RefreshUpdateControls();
            try
            {
                lastUpdateResult = await releaseUpdates.CheckAsync(typeof(MainWindow).Assembly.GetName().Version!);
                RefreshUpdateControls();
            }
            finally
            {
                checkingUpdates = false;
                RefreshUpdateControls();
                if (page == "Tools") { Footer.Text = lastUpdateResult?.Message ?? "Update check finished."; SendWebState(); }
            }
        };
        actions.Children.Add(check);
        actions.Children.Add(release);
        card.Children.Add(status);
        card.Children.Add(actions);
    }

    private void RefreshUpdateControls()
    {
        if (updateCheckButton != null) { updateCheckButton.IsEnabled = !checkingUpdates; updateCheckButton.Content = checkingUpdates ? "Checking…" : "Check for updates"; }
        if (updateStatus != null) updateStatus.Text = checkingUpdates ? "Checking GitHub for a stable release…" : lastUpdateResult?.Message ?? "Check for a new version.";
        if (updateReleaseButton != null) updateReleaseButton.Content = lastUpdateResult?.Available == true ? "View update" : "Open releases";
        if (page == "Tools") SendWebState();
    }
}
