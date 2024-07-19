using UnityEngine;

namespace NWH.Common.AssetInfo
{
    [CreateAssetMenu(fileName = "AssetInfo", menuName = "NWH/AssetInfo", order = 0)]
    public class AssetInfo : ScriptableObject
    {
        public string assetName = "Asset";
        public string version = "1.0";
        public string upgradeNotesURL = "";
        public string changelogURL = "";
        public string quickStartURL = "";
        public string documentationURL = "";
        public string discordURL = "https://discord.gg/59CQGEJ";
        public string forumURL = "";
        public string emailURL = "mailto:arescec@gmail.com";
        public string assetURL = "https://assetstore.unity.com/packages/tools/physics/nwh-vehicle-physics-2-166252";
    }
}