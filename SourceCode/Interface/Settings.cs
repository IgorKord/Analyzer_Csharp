namespace TMCAnalyzer.Properties {
    /// <summary>
    /// Settings provides a means to handle specific events:
    /// <br/>The SettingChanging event is raised before a setting's value is changed.
    /// <br/>The PropertyChanged event is raised after a setting's value is changed.
    /// <br/>The SettingsLoaded event is raised after the setting values are loaded.
    /// <br/>The SettingsSaving event is raised before the setting values are saved.</summary>
    internal sealed partial class Settings {
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }

        /// <summary>
        /// Prepare an instance of the <see cref="TMCAnalyzer.MagNetXAnalyzer.Properties.Settings"/>
        /// class.</summary>
        public Settings() {
#if false
            // To add event handlers for saving and changing settings, enable this code:
            this.SettingChanging += this.SettingChangingEventHandler;
            this.SettingsSaving += this.SettingsSavingEventHandler;
#endif
        }
    }
}