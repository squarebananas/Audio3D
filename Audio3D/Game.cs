#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Audio3D
{
    /// <summary>
    /// Sample showing how to implement 3D audio.
    /// </summary>
    public class Audio3DGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        AudioManager audioManager;

        SpriteEntity cat;
        SpriteEntity dog;

        Texture2D checkerTexture;

        QuadDrawer quadDrawer;

        Vector3 cameraPosition = new Vector3(0, 512, 0);
        Vector3 cameraForward = Vector3.Forward;
        Vector3 cameraUp = Vector3.Up;
        Vector3 cameraVelocity = Vector3.Zero;

        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        #endregion

        #region Initialization


        public Audio3DGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            audioManager = new AudioManager(this);

            Components.Add(audioManager);

            cat = new Cat();
            dog = new Dog();
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            cat.Texture = Content.Load<Texture2D>("CatTexture");
            dog.Texture = Content.Load<Texture2D>("DogTexture");

            checkerTexture = Content.Load<Texture2D>("checker");

            quadDrawer = new QuadDrawer(graphics.GraphicsDevice);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera();

            // Tell the AudioManager about the new camera position.
            audioManager.Listener.Position = cameraPosition;
            audioManager.Listener.Forward = cameraForward;
            audioManager.Listener.Up = cameraUp;
            audioManager.Listener.Velocity = cameraVelocity;

            // Tell our game entities to move around and play sounds.
            (cat as Cat).relativeVelocityTestCameraPosition = cameraPosition;
            (cat as Cat).relativeVelocityTestCameraMatrix = Matrix.CreateWorld(Vector3.Zero, cameraForward, cameraUp);
            cat.Update(gameTime, audioManager);
            dog.Update(gameTime, audioManager);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            device.BlendState = BlendState.AlphaBlend;

            // Compute camera matrices.
            Matrix view = Matrix.CreateLookAt(cameraPosition,
                                              cameraPosition + cameraForward,
                                              cameraUp);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, device.Viewport.AspectRatio,
                                                                    1, 100000);

            // Draw the checkered ground polygon.
            Matrix groundTransform = Matrix.CreateScale(20000) *
                                     Matrix.CreateRotationX(MathHelper.PiOver2);

            quadDrawer.DrawQuad(checkerTexture, 32, groundTransform, view, projection);

            // Draw the game entities.
            cat.Draw(quadDrawer, cameraPosition, view, projection);
            dog.Draw(quadDrawer, cameraPosition, view, projection);

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Toggle Tone Test
            // (The tone's pitch should increase when the cat approaches the camera and decrease when going further away)
            if ((currentKeyboardState.IsKeyDown(Keys.F1)) || (currentGamePadState.Buttons.A == ButtonState.Pressed))
                (cat as Cat).toneTestActive = true;
            if ((currentKeyboardState.IsKeyDown(Keys.F2)) || (currentGamePadState.Buttons.B == ButtonState.Pressed))
                (cat as Cat).toneTestActive = false;

            // Toggle Relative Velocity Test
            // (The cat will move with the camera at a fixed distance. As both velocities are the same the relative velocity will be zero.
            // This means with the tone test active the pitch should remain unchanged when moving the camera forwards)
            if ((currentKeyboardState.IsKeyDown(Keys.F3)) || (currentGamePadState.Buttons.X == ButtonState.Pressed))
                (cat as Cat).relativeVelocityTestActive = true;
            if ((currentKeyboardState.IsKeyDown(Keys.F4)) || (currentGamePadState.Buttons.Y == ButtonState.Pressed))
                (cat as Cat).relativeVelocityTestActive = false;

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }


        /// <summary>
        /// Handles input for moving the camera.
        /// </summary>
        void UpdateCamera()
        {
            const float turnSpeed = 0.05f;
            const float accelerationSpeed = 4;
            const float frictionAmount = 0.98f;

            // Turn left or right.
            float turn = -currentGamePadState.ThumbSticks.Left.X * turnSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.Left))
                turn += turnSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.Right))
                turn -= turnSpeed;

            if (turn != 0f)
            {
                cameraForward = Vector3.TransformNormal(cameraForward, Matrix.CreateRotationY(turn));
                Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(cameraForward, cameraUp));
                cameraUp = Vector3.Normalize(Vector3.Cross(cameraRight, cameraForward));
            }

            // Roll left or right
            float rollLeftRight = currentGamePadState.ThumbSticks.Right.X * turnSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.A))
                rollLeftRight -= turnSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.D))
                rollLeftRight += turnSpeed;

            if (rollLeftRight != 0f)
            {
                cameraUp = Vector3.TransformNormal(cameraUp, Matrix.CreateFromAxisAngle(cameraForward, rollLeftRight));
                Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(cameraForward, cameraUp));
                cameraForward = Vector3.Normalize(Vector3.Cross(cameraUp, cameraRight));
            }

            // Roll up or down
            float rollUpDown = currentGamePadState.ThumbSticks.Right.Y * turnSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.W))
                rollUpDown += turnSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.S))
                rollUpDown -= turnSpeed;

            if (rollUpDown != 0f)
            {
                Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(cameraForward, cameraUp));
                cameraUp = Vector3.TransformNormal(cameraUp, Matrix.CreateFromAxisAngle(cameraRight, rollUpDown));
                cameraForward = Vector3.Normalize(Vector3.Cross(cameraUp, cameraRight));
            }

            // Accelerate forward or backward.
            float accel = currentGamePadState.ThumbSticks.Left.Y * accelerationSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.Up))
                accel += accelerationSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.Down))
                accel -= accelerationSpeed;

            cameraVelocity += cameraForward * accel;

            // Add velocity to the current position.
            cameraPosition += cameraVelocity;

            // Apply the friction force.
            cameraVelocity *= frictionAmount;
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>

    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Audio3DGame())
                game.Run();
        }
    }

    #endregion
}
