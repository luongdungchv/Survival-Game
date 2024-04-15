using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DL.StateMachine
{
    public class Manipulator
    {
        private Rect initialRect;
        private Rect currentRect;
        private Matrix4x4 _prevGuiMatrix;
        private StateMachineWindow window;

        private float zoomScale;
        public Vector2 position => new Vector2(currentRect.x, currentRect.y);
        public float Scale => this.zoomScale;

        public void SetUp(Rect initialRect, StateMachineWindow window)
        {
            this.initialRect = initialRect;
            this.currentRect = initialRect;
            this.zoomScale = 1;
            this.window = window;
        }

        public Rect BeginGUI()
        {
            GUI.EndGroup();

            GUI.BeginGroup(currentRect);

            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 translation = Matrix4x4.TRS(currentRect.position, Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

            return currentRect;
        }
        public void EndGUI()
        {
            GUI.matrix = _prevGuiMatrix;

            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, 21, this.window.position.width, this.window.position.height));
        }

        public void SetRectPosition(Vector2 position)
        {
            currentRect.x = position.x;
            currentRect.y = position.y;
        }
        public void SetZoomScale(float zoomScale)
        {
            this.zoomScale = zoomScale;
        }
        public void AlignToPivot(Vector2 pivot)
        {
            var scaledWidth = currentRect.width * zoomScale;
            var scaledHeight = currentRect.height * zoomScale;
            currentRect.position = pivot - new Vector2(scaledWidth, scaledHeight) / 2;
        }
        public void Translate(Vector2 direction)
        {
            this.SetRectPosition(this.position + direction);
        }
    }
}