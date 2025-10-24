#if UNITY_ANDROID
using Google.Play.Review;
#endif
using System.Collections;
using UnityEngine;

namespace Game.Core {
    public class InAppReviewManager {
        /// <summary>
        /// Should work for Android and iOS
        /// </summary>
        public static void ShowInAppReview() {
#if UNITY_ANDROID
            CoroutineHelper.Run(IAR_Process());
#endif
#if UNITY_IOS || UNITY_IPHONE
            UnityEngine.iOS.Device.RequestStoreReview();
#endif
        }

#if UNITY_ANDROID
        static IEnumerator IAR_Process() {
            var reviewManager = new ReviewManager();
            var requestFlowOperation = reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError) {
                // Log error.
                Debug.Log("Failed to request In App Review, reason:" + requestFlowOperation.Error.ToString());
                yield break;
            }
            var playReviewInfo = requestFlowOperation.GetResult();

            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            yield return launchFlowOperation;

            if (launchFlowOperation.Error != ReviewErrorCode.NoError) {
                // Log error.
                Debug.Log("Failed to launch In App Review, reason:" + launchFlowOperation.Error.ToString());
                yield break;
            }
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
        }
#endif
    }
}