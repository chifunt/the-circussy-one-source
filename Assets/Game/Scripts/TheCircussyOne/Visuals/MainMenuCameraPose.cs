using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class MainMenuCameraPose : MonoBehaviour
    {
        [SerializeField, Range(1f, 170f)] private float fieldOfView = 60f;

        public float FieldOfView
        {
            get => Mathf.Clamp(fieldOfView, 1f, 170f);
            set => fieldOfView = Mathf.Clamp(value, 1f, 170f);
        }
    }
}
