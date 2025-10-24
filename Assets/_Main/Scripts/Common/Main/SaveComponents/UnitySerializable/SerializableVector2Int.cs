using UnityEngine;
using System;

namespace Game.Core {
    [Serializable]
    public struct SerializableVector2Int {
        public int X;
        public int Y;

        public SerializableVector2Int(int rX, int rY) {
            X = rX;
            Y = rY;
        }

        public override string ToString() {
            return $"[{X}, {Y}]";
        }

        public static implicit operator Vector2(SerializableVector2Int rValue) {
            return new Vector2(rValue.X, rValue.Y);
        }

        public static implicit operator Vector2Int(SerializableVector2Int rValue) {
            return new Vector2Int(rValue.X, rValue.Y);
        }

        public static implicit operator SerializableVector2Int(Vector2Int rValue) {
            return new SerializableVector2Int(rValue.x, rValue.y);
        }
    }
}
