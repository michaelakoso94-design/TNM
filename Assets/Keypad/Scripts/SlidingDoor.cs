using System.Collections;
using UnityEngine;
namespace NavKeypad
{
    public class SlidingDoor : MonoBehaviour
    {
        [Header("Slide Settings")]
        [Tooltip("Local Y position the door slides to when access is granted.")]
        [SerializeField] private float openHeight = 4.42f;
        [Tooltip("How long (seconds) the slide takes.")]
        [SerializeField] private float slideDuration = 2f;
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Component References")]
        [Tooltip("Optional. When assigned the door is driven by the Animator's isOpen bool instead of sliding on Y.")]
        [SerializeField] private Animator anim;

        public bool IsOpoen => isOpen;
        private bool isOpen = false;

        private Vector3 closedPos;
        private Coroutine slideRoutine;

        private void Awake()
        {
            closedPos = transform.localPosition;
        }

        public void ToggleDoor()
        {
            isOpen = !isOpen;
            ApplyState();
        }

        public void OpenDoor()
        {
            isOpen = true;
            ApplyState();
        }
        public void CloseDoor()
        {
            isOpen = false;
            ApplyState();
        }

        private void ApplyState()
        {
            if (anim != null)
            {
                anim.SetBool("isOpen", isOpen);
                return;
            }

            Vector3 targetPos = isOpen ? new Vector3(closedPos.x, openHeight, closedPos.z) : closedPos;

            if (slideDuration <= 0f)
            {
                transform.localPosition = targetPos;
                return;
            }

            if (slideRoutine != null) StopCoroutine(slideRoutine);
            slideRoutine = StartCoroutine(SlideRoutine(targetPos));
        }

        private IEnumerator SlideRoutine(Vector3 targetPos)
        {
            Vector3 startPos = transform.localPosition;

            float elapsedTime = 0f;
            while (elapsedTime < slideDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / slideDuration);

                transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, slideCurve.Evaluate(t));

                yield return null;
            }
            transform.localPosition = targetPos;

            slideRoutine = null;
        }
    }
}