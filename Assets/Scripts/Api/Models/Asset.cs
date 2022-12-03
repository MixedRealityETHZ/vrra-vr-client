using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts.Api.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssetStatus
    {
        Uploading,
        Ready,
    }

    public class Asset
    {
        public int Id { get; set; }

        public AssetStatus Status { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string? Url { get; set; }
    }
}