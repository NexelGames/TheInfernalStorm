using UnityEngine;
using System;

namespace Game.Core {
    [Serializable]
    public struct SerializableQuaternion {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public SerializableQuaternion(float rX, float rY, float rZ, float rW) {
            X = rX;
            Y = rY;
            Z = rZ;
            W = rW;
        }

        public override string ToString() {
            return $"[{X}, {Y}, {Z}, {W}]";
        }

        public static implicit operator Quaternion(SerializableQuaternion rValue) {
            return new Quaternion(rValue.X, rValue.Y, rValue.Z, rValue.W);
        }

        public static implicit operator SerializableQuaternion(Quaternion rValue) {
            return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }
}