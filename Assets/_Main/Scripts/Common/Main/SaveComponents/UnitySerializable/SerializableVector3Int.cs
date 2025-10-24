using UnityEngine;
using System;

namespace Game.Core {
    [Serializable]
    public struct SerializableVector3Int {
        public int X;
        public int Y;
        public int Z;

        public SerializableVector3Int(int rX, int rY, int rZ) {
            X = rX;
            Y = rY;
            Z = rZ;
        }

        public override string ToString() {
            return $"[{X}, {Y}, {Z}]";
        }

        public static implicit operator Vector3(SerializableVector3Int rValue) {
            return new Vector3(rValue.X, rValue.Y, rValue.Z);
        }

        public static implicit operator Vector3Int(SerializableVector3Int rValue) {
            return new Vector3Int(rValue.X, rValue.Y, rValue.Z);
        }

        public static implicit operator SerializableVector3Int(Vector3Int rValue) {
            return new SerializableVector3Int(rValue.x, rValue.y, rValue.z);
        }
    }
}
