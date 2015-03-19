using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CleanWindow
{
    class PlayerManager
    {
        public Sprite playerSprite;
        private Rectangle playerAreaLimit;

        public long PlayerScore = 0;
        public int LivesRemaining = 3;
        public bool Destroyed = false;

        private int playerRadius = 70;
        
        
        private Rectangle initialFrame;

        public PlayerManager(
            Texture2D texture,  
            int frameCount,
            Rectangle screenBounds)
        {

            initialFrame = new Rectangle(0, 0, 145, 145);


            playerSprite = new Sprite(
                new Vector2(0,0),
                texture,
                new Rectangle(0, 0, 145, 145),
                Vector2.Zero);

            playerAreaLimit = screenBounds;


            for (int x = 1; x < frameCount; x++)
            {
                playerSprite.AddFrame(
                    new Rectangle(
                        initialFrame.X + (initialFrame.Width * x),
                        initialFrame.Y,
                        initialFrame.Width,
                        initialFrame.Height));
            }
            playerSprite.CollisionRadius = playerRadius;
        
        }

        private void imposeMovementLimits()
        {

            Vector2 location = playerSprite.Location;

            if (location.X < playerAreaLimit.X)
                location.X = playerAreaLimit.X;

            if (location.X >
                (playerAreaLimit.Right - playerSprite.Source.Width))
                location.X =
                    (playerAreaLimit.Right - playerSprite.Source.Width);

            if (location.Y < playerAreaLimit.Y)
                location.Y = playerAreaLimit.Y;

            if (location.Y >
                (playerAreaLimit.Bottom - playerSprite.Source.Height))
                location.Y =
                    (playerAreaLimit.Bottom - playerSprite.Source.Height);

            playerSprite.Location = location;

            
        }

        public void Update(GameTime gameTime)
        {
            
            if (!Destroyed)
            {
               
                playerSprite.Update(gameTime);
                //imposeMovementLimits();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
           
            if (!Destroyed)
            {
                playerSprite.Draw(spriteBatch);
            }
        }

    }
}
