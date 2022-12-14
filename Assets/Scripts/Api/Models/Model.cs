using UnityEngine;

namespace Assets.Scripts.Api.Models
{
    public class Bounds3
    {
        public Vector3 PMin { get; set; }
        
        public Vector3 PMax { get; set; }
        
        public Vector3 Center => (PMin + PMax) / 2;
        
        public Vector3 Size => PMax - PMin;
    }

    public class Model
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public Bounds3 Bounds { get; set; }

        public int AssetId { get; set; }

        public int? ThumbnailAssetId { get; set; }
    }
}