using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public class Camera
    {
        public float Rotation;
        public float Scale;
        public Vector2 Position;
        public Rectangle Bounds;
        public Vector2 Target;
        public bool AttachedToPlayer;
        public Vector2 Velocity
        {
            get
            {
                return new Vector2(600, 600);
            }
        }
        public Viewport Viewport;

        public Camera(Vector2 position, Vector2 origin, float rotation, float scale, GraphicsDeviceManager graphics)
        {
            ResetCamera(position, origin, rotation, scale, graphics);
            AttachedToPlayer = false;
        }

        public void ResetCamera(Vector2 position, Vector2 origin, float rotation, float scale, GraphicsDeviceManager graphics)
        {
            Rotation = rotation;
            Scale = scale;
            Position = position;
            Bounds = graphics.GraphicsDevice.Viewport.Bounds;
            Viewport = graphics.GraphicsDevice.Viewport;
        }

        //public Vector3 ResetScreenScale(GraphicsDeviceManager graphics, Vector2 screenScale)
        //{
        //    var scaleX = (float)graphics.GraphicsDevice.Viewport.Width / screenScale.X;
        //    var scaleY = (float)graphics.GraphicsDevice.Viewport.Height / screenScale.Y;
        //    Scale = new Vector3(scaleX, scaleY, 1.0f);
        //    Viewport = graphics.GraphicsDevice.Viewport;
        //    return Scale;
        //}

        public Matrix GetMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3((int)-Position.X, (int)-Position.Y, 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Scale) *
                Matrix.CreateTranslation(new Vector3((int)(Bounds.Width * 0.5f), (int)(Bounds.Height * 0.5f), 0));
        }

        public Matrix GetInverseMatrix()
        {
            return
                Matrix.Invert(
                    Matrix.CreateTranslation(new Vector3((int)-Position.X, (int)-Position.Y, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Scale) *
                    Matrix.CreateTranslation(new Vector3((int)(Bounds.Width * 0.5f), (int)(Bounds.Height * 0.5f), 0)));
        }

        public Vector2 ScreenToWorld(Point point)
        {
            return Vector2.Transform(point.ToVector2(), this.GetInverseMatrix());
        }

        public Vector2 WorldToScreen(Point point)
        {
            return Vector2.Transform(point.ToVector2(), this.GetMatrix());
        }

        public bool IsInView(Matrix matrix, Vector2 positionUpperBounds, Vector2 positionLowerBounds)
        {
            return this.Viewport.Bounds.Contains(Vector2.Transform(positionLowerBounds, matrix))
                || this.Viewport.Bounds.Contains(Vector2.Transform(positionUpperBounds, matrix));
        }

    }
}
