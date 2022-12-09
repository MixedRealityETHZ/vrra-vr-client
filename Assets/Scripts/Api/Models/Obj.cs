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
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public bool Movable { get; set; }
        public int ModelId { get; set; }
    }
}
