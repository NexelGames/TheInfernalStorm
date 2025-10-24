using UnityEngine;
using System;

namespace Game.Core {
    [Serializable]
    public struct SerializableVector3 {
        public float X;
        public float Y;
        public float Z;

        public SerializableVector3(float rX, float rY, float rZ) {
            X = rX;
            Y = rY;
            Z = rZ;
        }

        public override string ToString() {
            return $"[{X}, {Y}, {Z}]";
        }

        public static implicit operator Vector3(SerializableVector3 rValue) {
            return new Vector3(rValue.X, rValue.Y, rValue.Z);
        }

        public static implicit operator SerializableVector3(Vector3 rValue) {
            return new SerializableVector3(rValue.x, rValue.y, rValue.z);
        }
    }
}