using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVEMon.Common;
using EVEMon.Common.Controls;
using EVEMon.Common.SettingsObjects;
using EVEMon.Common.Extensions;

namespace EVEMon.ApiErrorHandling
{
    /// <summary>
    /// Http Timeout Troubleshooter displays when a HTTP Timeout has occurred.
    /// </summary>
    public partial class HttpTimeoutTroubleshooter : ApiErrorTroubleshooter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTimeoutTroubleshooter"/> class.
        /// </summary>
        public HttpTimeoutTroubleshooter()
        {
            InitializeComponent();

            List<TimeoutOption> options = new List<TimeoutOption>();
            UpdateSettings updateSettings = new UpdateSettings();

            // Lets add 10 - 60 to the list
            for (int i = 10; i <= 60; i += 10)
            {
                string text = String.Empty;

                if (i == updateSettings.HttpTimeout)
                    text = "Default";

                if (i == Settings.Updates.HttpTimeout)
                    text = "Current";

                options.Add(new TimeoutOption(i, text));
            }

            // If the current is set to something odd we add it and sort by Seconds
            if (options.All(x => x.Seconds != Settings.Updates.HttpTimeout))
                options.Add(new TimeoutOption(Settings.Updates.HttpTimeout, "Current"));

            // If the default is not in the list we add it
            if (options.All(x => x.Seconds != updateSettings.HttpTimeout))
                options.Add(new TimeoutOption(updateSettings.HttpTimeout, "Default"));

            options.Sort((a, b) => a.Seconds.CompareTo(b.Seconds));

            // Databind
            TimeoutDropDown.DataSource = options;
            TimeoutDropDown.DisplayMember = "Label";
            TimeoutDropDown.ValueMember = "Seconds";
        }

        /// <summary>
        /// Handles the Click event of the SetTimeoutButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SetTimeoutButton_Click(object sender, EventArgs e)
        {
            Settings.Updates.HttpTimeout = (int)TimeoutDropDown.SelectedValue;
            OnErrorResolved(ResolutionAction.Close);
        }

        /// <summary>
        /// Private class to store and format suitable time spans.
        /// </summary>
        private class TimeoutOption
        {
            private readonly string m_text;

            /// <summary>
            /// Initializes a new instance of the <see cref="TimeoutOption"/> class.
            /// </summary>
            /// <param name="seconds">The seconds.</param>
            /// <param name="text">The text.</param>
            public TimeoutOption(int seconds, string text)
            {
                Seconds = seconds;
                m_text = text;
            }

            /// <summary>
            /// Gets or sets the seconds.
            /// </summary>
            /// <value>The seconds.</value>
            public int Seconds { get; }

            /// <summary>
            /// Gets the label.
            /// </summary>
            /// <value>The label.</value>
            public String Label
            {
                get
                {
                    StringBuilder builder = new StringBuilder();

                    if (Seconds % 60 == 0)
                    {
                        int minutes = Seconds / 60;
                        builder.Append($"{minutes} Minute{(minutes.S())}");
                    }
                    else
                        builder.Append($"{Seconds} Seconds");

                    if (!String.IsNullOrEmpty(m_text))
                        builder.Append($" ({m_text})");

                    return builder.ToString();
                }
            }
        }
    }
}
