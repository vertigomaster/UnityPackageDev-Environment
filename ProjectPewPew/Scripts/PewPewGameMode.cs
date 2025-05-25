using System.Collections.Generic;
using IDEK.Tools.GameplayEssentials.Characters.Unity;
using IDEK.Tools.GameplayEssentials.Core;
using UnityEngine;

namespace IDEK.Tools.GameplayEssentials.Samples.PewPew
{
    public class PewPewGameMode : GameMode
    {
        #region Overrides of GameMode

        /// <inheritdoc />
        public override IEnumerable<IPlayerCharacter> GetAllPlayers()
        {
            var x = RuntimeCharacterRegistry.Singleton.GetAllStates();
            return x;
        }

        #endregion
    }
}