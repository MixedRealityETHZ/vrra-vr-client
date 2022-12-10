using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Api.Models
{
    public class Obj
    {
        public int Id { get; set; }
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public bool Movable { get; set; }
        public int RoomId { get; set; }
        public Model Model { get; set; }
    }

    public class AddObjBody
    {
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Translation { get; set; }

        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion Rotation { get; set; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Scale { get; set; }

        public bool Movable { get; set; }

        public int ModelId { get; set; }
    }
}