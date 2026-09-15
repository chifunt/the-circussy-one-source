using UnityEngine;

namespace TheCircussyOne.Visuals
{
    internal sealed class ActorGroundShadowProjector
    {
        private bool hasBase;
        private float baseLocalY;
        private Vector3 baseScale = Vector3.one;

        public void CaptureBase(Transform groundShadow)
        {
            if (groundShadow == null || hasBase)
            {
                return;
            }

            hasBase = true;
            baseLocalY = groundShadow.localPosition.y;
            baseScale = groundShadow.localScale;
        }

        public void Project(Transform groundShadow, float rootWorldY, float referenceJumpHeight)
        {
            if (groundShadow == null)
            {
                return;
            }

            CaptureBase(groundShadow);
            Vector3 localPosition = groundShadow.localPosition;
            localPosition.x = 0f;
            localPosition.y = baseLocalY - rootWorldY;
            localPosition.z = 0f;
            groundShadow.localPosition = localPosition;

            float height01 = Mathf.Clamp01(rootWorldY / Mathf.Max(0.01f, referenceJumpHeight));
            float scale = Mathf.Lerp(1f, 0.72f, height01);
            groundShadow.localScale = new Vector3(
                baseScale.x * scale,
                baseScale.y * scale,
                baseScale.z);
        }
    }
}
