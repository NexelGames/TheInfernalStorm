using UnityEngine;
using System;

namespace Game.Core {
    [Serializable]
    public struct SerializableVector2 {
        public float X;
        public float Y;

        public SerializableVector2(float rX, float rY) {
            X = rX;
            Y = rY;
        }

        public override string ToString() {
            return $"[{X}, {Y}]";
        }

        public static implicit operator Vector3(SerializableVector2 rValue) {
            return new Vector3(rValue.X, rValue.Y);
        }

        public static implicit operator SerializableVector2(Vector2 rValue) {
            return new SerializableVector2(rValue.x, rValue.y);
        }
    }
}
