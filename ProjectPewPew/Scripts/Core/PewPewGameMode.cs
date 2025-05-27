using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Characters.Unity;
using IDEK.Tools.GameplayEssentials.Core;
using IDEK.Tools.Logging;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class PewPewGameMode : GameMode
    {
        protected virtual void Start()
        {
            ConsoleLog.LogWarning("TODO: Need to implement player leveling so that " +
                "GetPlayerEncounterLevel() can be properly filled out");
        }

        #region Overrides of GameMode

        /// <inheritdoc />
        public override IEnumerable<IPlayerCharacter> GetAllPlayers()
        {
            var x = RuntimeCharacterRegistry.Singleton.GetAllStates();
            return x;
        }

        /// <inheritdoc />
        public override int GetPlayerEncounterLevel()
        {
            //TODO: implement player leveling so that we can fill this out properly
            return 1;
        }

        #endregion
    }
}