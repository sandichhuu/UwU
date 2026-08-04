using UnityEngine;

namespace UwU.EasyData
{
    public class TableSetAsset : ScriptableObject
    {
        public TableSet tableSet;
        [HideInInspector] public byte[] bytes;
    }
}