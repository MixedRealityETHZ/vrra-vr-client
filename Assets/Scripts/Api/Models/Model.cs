using UnityEngine;

namespace Assets.Scripts.Api.Models
{
    public class Model
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public Vector3? Bounds { get; set; }

        public int AssetId { get; set; }

        public int? ThumbnailAssetId { get; set; }
    }
}