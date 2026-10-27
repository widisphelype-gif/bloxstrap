namespace Bloxstrap.UI.ViewModels.Settings
{
    public class BehaviourViewModel : NotifyPropertyChangedViewModel
    {
        public bool ConfirmLaunches
        {
            get => App.Settings.Prop.ConfirmLaunches;
            set => App.Settings.Prop.ConfirmLaunches = value;
        }

        public bool BackgroundUpdates
        {
            get => App.Settings.Prop.BackgroundUpdatesEnabled;
            set => App.Settings.Prop.BackgroundUpdatesEnabled = value;
        }

        public bool BoostCleanLogs
        {
            get => App.Settings.Prop.BoostCleanLogs;
            set => App.Settings.Prop.BoostCleanLogs = value;
        }

        public bool BoostHighPerformancePlan
        {
            get => App.Settings.Prop.BoostHighPerformancePlan;
            set => App.Settings.Prop.BoostHighPerformancePlan = value;
        }

        public bool BoostHighPriority
        {
            get => App.Settings.Prop.BoostHighPriority;
            set => App.Settings.Prop.BoostHighPriority = value;
        }

        public bool BoostCloseApps
        {
            get => App.Settings.Prop.BoostCloseApps;
            set => App.Settings.Prop.BoostCloseApps = value;
        }

        public string BoostAppsToClose
        {
            get => App.Settings.Prop.BoostAppsToClose;
            set => App.Settings.Prop.BoostAppsToClose = value;
        }

        public bool IsRobloxInstallationMissing => !App.IsPlayerInstalled && !App.IsStudioInstalled;

        public bool ForceRobloxReinstallation
        {
            get => App.State.Prop.ForceReinstall || IsRobloxInstallationMissing;
            set => App.State.Prop.ForceReinstall = value;
        }
    }
}
