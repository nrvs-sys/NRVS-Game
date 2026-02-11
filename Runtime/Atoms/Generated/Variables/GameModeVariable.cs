using UnityEngine;
using System;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable of type `GameMode`. Inherits from `AtomVariable&lt;GameMode, GameModePair, GameModeEvent, GameModePairEvent, GameModeGameModeFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/GameMode", fileName = "GameModeVariable")]
    public sealed class GameModeVariable : AtomVariable<GameMode, GameModePair, GameModeEvent, GameModePairEvent, GameModeGameModeFunction>
    {
        protected override bool ValueEquals(GameMode other)
        {
            throw new NotImplementedException();
        }
    }
}
