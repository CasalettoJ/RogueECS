using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public class Camera
    {
        public float Rotation;
        public Vector3 Scale;
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

        public Camera(Vector2 position, Vector2 origin, float rotation, Vector2 scale, GraphicsDeviceManager graphics)
        {
            ResetCamera(position, origin, rotation, scale, graphics);
            AttachedToPlayer = false;
        }

        public void ResetCamera(Vector2 position, Vector2 origin, float rotation, Vector2 scale, GraphicsDeviceManager graphics)
        {
            Rotation = rotation;
            ResetScreenScale(graphics, scale);
            Position = position;
            Bounds = graphics.GraphicsDevice.Viewport.Bounds;
        }

        public Vector3 ResetScreenScale(GraphicsDeviceManager graphics, Vector2 screenScale)
        {
            var scaleX = (float)graphics.GraphicsDevice.Viewport.Width / screenScale.X;
            var scaleY = (float)graphics.GraphicsDevice.Viewport.Height / screenScale.Y;
            Scale = new Vector3(scaleX, scaleY, 1.0f);
            return Scale;
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

        //public Rectangle GetVisibleArea(GraphicsDeviceManager graphics)
        //{
        //    var inverseViewMatrix = GetMatrix();
        //        var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
        //        var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
        //        var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
        //        var br = Vector2.Transform(new Vector2(Bounds.X, Bounds.Y), inverseViewMatrix);
        //        var min = new Vector2(
        //            MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
        //            MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
        //        var max = new Vector2(
        //            MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
        //            MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
        //        return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        //}

    }
}
