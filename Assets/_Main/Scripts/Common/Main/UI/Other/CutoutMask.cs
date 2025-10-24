using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Game.UI {
    [AddComponentMenu("UI/Cutout Mask")]
    public class CutoutMask : Image {
        // https://www.youtube.com/watch?v=XJJl19N2KFM
        public override Material materialForRendering {
            get {
                Material material = new Material(base.materialForRendering);
                material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return material;
            }
        }
    }
}