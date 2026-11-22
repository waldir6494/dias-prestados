using UnityEngine;

namespace DiasPrestados.Creatures
{
    /// <summary>
    /// Fotogramas de Chronoram ya ordenados de archivo 0001 al último.
    /// CabraWalker invierte turn head y turn around al reproducirlos.
    /// </summary>
    public class CabraSpriteSet : ScriptableObject
    {
        public Sprite[] walking;
        public Sprite[] turnHead;
        public Sprite[] turnAround;
    }
}
