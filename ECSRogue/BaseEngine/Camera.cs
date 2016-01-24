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
        public Viewport DungeonViewport;
        public Viewport FullViewport;
        public Viewport DungeonUIViewport;
        public Viewport DungeonUIViewportLeft;

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
            FullViewport = graphics.GraphicsDevice.Viewport;

            DungeonViewport = FullViewport;
            DungeonViewport.Height -= 200;
            DungeonViewport.Width -= 200;
            Bounds = DungeonViewport.Bounds;

            DungeonUIViewport = FullViewport;
            DungeonUIViewport.Height = 200;
            DungeonUIViewport.Width -= DungeonUIViewport.Height;
            DungeonUIViewport.Y = DungeonViewport.Height;

            DungeonUIViewportLeft = FullViewport;
            DungeonUIViewportLeft.Width = DungeonUIViewport.Height;
            DungeonUIViewportLeft.X = DungeonUIViewport.Width;
        }

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

        public bool IsInView(Matrix matrix, Rectangle item)
        {
            //return this.DungeonViewport.Bounds.Contains(Vector2.Transform(positionLowerBounds, matrix))
            //    || this.DungeonViewport.Bounds.Contains(Vector2.Transform(positionUpperBounds, matrix));
            return !Rectangle.Intersect(this.DungeonViewport.Bounds, item).IsEmpty;
        }

    }
}
