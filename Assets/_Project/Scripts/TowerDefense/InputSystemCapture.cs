using System;
using UnityEngine;

namespace TowerDefense
{
    public class InputSystemCapture : MonoBehaviour
    {
        [SerializeField] private Joystick _joystick;
        public static bool IsMouseDown;
        public static bool IsMouseUp;
        public static Vector2 MousePosition;

        public static Vector2 JoystickDirection;

        private void Awake()
        {
            
        }

        private void Update()
        {
            IsMouseDown = Input.GetMouseButtonDown(0);
            IsMouseUp = Input.GetMouseButtonUp(0);
            MousePosition = Input.mousePosition;

            //JoystickDirection = _joystick.Direction;

        }

        private void OnDestroy()
        {
            IsMouseDown = false;
        }
    }
}