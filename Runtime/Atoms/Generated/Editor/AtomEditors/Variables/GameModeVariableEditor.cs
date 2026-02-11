using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `GameMode`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(GameModeVariable))]
    public sealed class GameModeVariableEditor : AtomVariableEditor<GameMode, GameModePair> { }
}
