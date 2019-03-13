using System;
using System.Collections.Generic;
using System.Linq;
using EVEMon.Common.Collections;
using EVEMon.Common.Enumerations;
using EVEMon.Common.Enumerations.CCPAPI;
using EVEMon.Common.Interfaces;
using EVEMon.Common.Serialization.Esi;

namespace EVEMon.Common.Models.Collections
{
    public sealed class PlanetaryColonyCollection : ReadonlyCollection<PlanetaryColony>
    {
        private readonly CCPCharacter m_ccpCharacter;


        #region Constructor

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="ccpCharacter">The CCP character.</param>
        internal PlanetaryColonyCollection(CCPCharacter ccpCharacter)
        {
            m_ccpCharacter = ccpCharacter;

            EveMonClient.TimerTick += EveMonClient_TimerTick;
        }

        #endregion


        #region Inherited Events

        /// <summary>
        /// Called when the object gets disposed.
        /// </summary>
        internal void Dispose()
        {
            EveMonClient.TimerTick -= EveMonClient_TimerTick;
        }

        /// <summary>
        /// Handles the TimerTick event of the EveMonClient control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EveMonClient_TimerTick(object sender, EventArgs e)
        {
            IQueryMonitor charPlanetaryColoniesMonitor = m_ccpCharacter.QueryMonitors[ESIAPICharacterMethods.PlanetaryColonies];

            if (charPlanetaryColoniesMonitor == null || !charPlanetaryColoniesMonitor.Enabled)
                return;

            UpdateOnTimerTick();
        }

        #endregion


        #region Importation

        /// <summary>
        /// Imports an enumeration of API objects.
        /// </summary>
        /// <param name="src">The enumeration of serializable planetary colony log from the API.</param>
        internal void Import(IEnumerable<EsiPlanetaryColonyListItem> src)
        {
            Items.Clear();

            // Import the palnetary colony from the API
            foreach (EsiPlanetaryColonyListItem srcColony in src)
            {
                Items.Add(new PlanetaryColony(m_ccpCharacter, srcColony));
            }
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Notify the user on a pin completion.
        /// </summary>
        private void UpdateOnTimerTick()
        {
            // We exit if there are no pins
            if (!Items.Any())
                return;

            // Add the not notified idle pins to the completed list
            // Updated 3/10/2019, removed check on Pin state = Idle, as the state doesn't get updated unless the planetary column is visible
            // Left in the check on PinState not equal to none as that prevents alerts popping for non-activatable items (storages, etc.)
            List<PlanetaryPin> pinsCompleted = Items.SelectMany(x => x.Pins).Where(
                pin => pin.State != PlanetaryPinState.None && pin.TTC.Length == 0 && !pin.NotificationSend).ToList();

            pinsCompleted.ForEach(pin => pin.NotificationSend = true);

            // We exit if no pins have finished
            if (!pinsCompleted.Any())
                return;

            // Fires the event regarding the character's pins finished
            EveMonClient.OnCharacterPlanetaryPinsCompleted(m_ccpCharacter, pinsCompleted);
        }

        #endregion
    }
}
