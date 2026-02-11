using System;
using UnityEngine;
namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// IPair of type `&lt;GameMode&gt;`. Inherits from `IPair&lt;GameMode&gt;`.
    /// </summary>
    [Serializable]
    public struct GameModePair : IPair<GameMode>
    {
        public GameMode Item1 { get => _item1; set => _item1 = value; }
        public GameMode Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private GameMode _item1;
        [SerializeField]
        private GameMode _item2;

        public void Deconstruct(out GameMode item1, out GameMode item2) { item1 = Item1; item2 = Item2; }
    }
}