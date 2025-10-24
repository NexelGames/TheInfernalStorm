using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class DebugBuildEnterPoint : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            SceneManager.LoadScene("_Loader");
        }
    }
}