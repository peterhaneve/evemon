using System.Collections.Generic;
using System.Collections.ObjectModel;
using EVEMon.Common.Extensions;
using EVEMon.Common.Models;

namespace EVEMon.Common.Notifications
{
    /// <summary>
    /// Provides notification services for PlanetaryPins.
    /// </summary>
    public sealed class PlanetaryPinsNotificationEventArgs: NotificationEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanetaryPinsNotificationEventArgs"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="pins">The pins.</param>
        /// <exception cref="System.ArgumentNullException">pins</exception>
        public PlanetaryPinsNotificationEventArgs(object sender, IEnumerable<PlanetaryPin> pins)
            : base(sender, NotificationCategory.PlanetaryPinsCompleted)
        {
            pins.ThrowIfNull(nameof(pins));

            PlanetaryPins = new Collection<PlanetaryPin>();
            foreach (PlanetaryPin pin in pins)
            {
                PlanetaryPins.Add(pin);
            }
            UpdateDescription();
        }
   
        /// <summary>
        /// Gets the associated API result.
        /// </summary>
        public Collection<PlanetaryPin> PlanetaryPins { get; }

        /// <summary>
        /// Gets true if the notification has details.
        /// </summary>
        public override bool HasDetails => true;

        /// <summary>
        /// Enqueue the jobs from the given notification at the end of this notification.
        /// </summary>
        /// <param name="other"></param>
        public override void Append(NotificationEventArgs other)
        {
            foreach (PlanetaryPin pin in ((PlanetaryPinsNotificationEventArgs)other).PlanetaryPins)
            {
                PlanetaryPins.Add(pin);
            }

            UpdateDescription();
        }

        /// <summary>
        /// Updates the description.
        /// </summary>
        private void UpdateDescription()
        {
            Description = $"{PlanetaryPins.Count} planetary colon{(PlanetaryPins.Count == 1 ? "y is" : "ies are")} idle.";
        }
    }
 }
