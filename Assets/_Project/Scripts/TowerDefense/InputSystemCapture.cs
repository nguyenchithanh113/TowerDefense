using System;
using UnityEngine;

namespace TowerDefense
{
    public class InputSystemCapture : MonoBehaviour
    {
        public static bool IsMouseDown;
        public static Vector2 MousePosition;

        private void Update()
        {
            IsMouseDown = Input.GetMouseButtonDown(0);
            MousePosition = Input.mousePosition;
        }

        private void OnDestroy()
        {
            IsMouseDown = false;
        }
    }
}