using TGC.Core.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TGC.Core.Text;
using TGC.Core.Mathematica;
using TGC.Examples.Engine2D.Spaceship.Core;
using TGC.Core.Input;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Example;

namespace TGC.Group.Model.Gui
{
    static class Hud
    {
        public enum Status { 
            MainMenu, //This state only works at the start of the game
            Ship,
            Water,
            Inventory,
            Crafting,
            GameOver, //Player changes to this state when IsDead();
            None
        };

        //Control vars
        static Status CurrentStatus = Status.MainMenu;
        static TgcText2D SelectedText;
        static CustomSprite SelectedSprite;

        //"Consts"
        static int WIDTH;
        static int HEIGHT;

        //Drawing vars
        static Drawer2D drawer;
        static TgcText2D Start;
        static TgcText2D Exit;

        static CustomSprite Logo;
        static CustomSprite Overlay; //Inventario y crafting comparten el mismo sprite para el fondo
        static CustomSprite Background;

        static TgcText2D GameOver;

        




        static public void Init(string MediaDir)
        {
            WIDTH = D3DDevice.Instance.Width;
            HEIGHT = D3DDevice.Instance.Height;

            drawer = new Drawer2D();
            
            //Main Menu
            Logo = new CustomSprite();
            Logo.Bitmap = new CustomBitmap(MediaDir + "logo_subnautica.png", D3DDevice.Instance.Device);
            var spriteSize = Logo.Bitmap.Size;
            Logo.Position = new TGCVector2(WIDTH / 2 - spriteSize.Width / 2, Round(HEIGHT * 0.2f) - spriteSize.Height / 2);

            Background = new CustomSprite();
            Background.Bitmap = new CustomBitmap(MediaDir + "background_subnautica.jpg", D3DDevice.Instance.Device);
            spriteSize = Background.Bitmap.Size;
            Background.Position = new TGCVector2(0,0);


            Start = new TgcText2D();
            Start.Text = "Start";
            Start.Align = TgcText2D.TextAlign.LEFT;
            Start.Position = new Point(Round(WIDTH * 0.45f), Round(HEIGHT * 0.7f));
            Start.Size = new Size(300, 100);
            Start.changeFont(new Font("Calibri", 35, FontStyle.Bold));
            Start.Color = Color.White;

            Exit = new TgcText2D();
            Exit.Text = "Exit";
            Exit.Align = TgcText2D.TextAlign.LEFT;
            Exit.Position = new Point(Round(WIDTH * 0.45f), Round(HEIGHT * 0.75f));
            Exit.Size = new Size(300, 100);
            Exit.changeFont(new Font("Calibri", 35, FontStyle.Bold));
            Exit.Color = Color.White;

            SelectedText = Start;


            //Gameplay
            


            //Inventory
            Overlay = new CustomSprite();
            Overlay.Bitmap = new CustomBitmap(MediaDir + "overlay_bck.png", D3DDevice.Instance.Device);
            spriteSize = Background.Bitmap.Size;
            Overlay.Position = new TGCVector2(WIDTH / 2 - spriteSize.Width / 2, HEIGHT / 2 - spriteSize.Height / 2);

            //Crafting
            


            //GameOver
            GameOver = new TgcText2D();
            GameOver.Text = "GAME OVER";
            GameOver.Align = TgcText2D.TextAlign.LEFT;
            GameOver.Position = new Point(Round(WIDTH * 0.28f), Round(HEIGHT * 0.4f));
            GameOver.Size = new Size(750, 500);
            GameOver.changeFont(new Font("Calibri", 100, FontStyle.Bold));
            GameOver.Color = Color.Red;
            
        }

        static public void Update(TgcD3dInput Input)
        {
            bool up = Input.keyDown(Key.Up);
            bool down = Input.keyDown(Key.Down);
            bool left = Input.keyPressed(Key.Left);
            bool right = Input.keyPressed(Key.Right);
            bool anyKey = up || down || left || right;

            //Reset effect when any key is pressed
            if (anyKey)
            {
                SelectedText.Color = Color.White;
            }

            //Update selection var
            if (CurrentStatus == Status.MainMenu)
            {
                if (up)
                    SelectedText = Start;
                else if (down)
                    SelectedText = Exit;
            }
        }

        static public void Render()
        {
            //If there's no HUD to be rendered then skip testing status
            if(CurrentStatus == Status.None) { return; }

            if(CurrentStatus == Status.MainMenu)
            {
                drawer.BeginDrawSprite();
                drawer.DrawSprite(Background);
                drawer.DrawSprite(Logo);
                drawer.EndDrawSprite();

                SelectedText.Color = Color.Orange;
                Start.render();
                Exit.render();
            }
            else if (CurrentStatus == Status.Inventory)
            {
                drawer.BeginDrawSprite();
                drawer.DrawSprite(Overlay);

                drawer.EndDrawSprite();
            }
            else if (CurrentStatus == Status.Crafting)
            {
                drawer.BeginDrawSprite();
                drawer.DrawSprite(Background);

                drawer.EndDrawSprite();
            }
            else if (CurrentStatus == Status.GameOver)
            {
                GameOver.render();
            }
        }

        static public void Dispose()
        {
            Start.Dispose();
            Exit.Dispose();
            GameOver.Dispose();
            Logo.Dispose();
            Background.Dispose();
            Overlay.Dispose();
        }

        //Interactive functions
        static public void ChangeStatus(Status status) { CurrentStatus = status; }
        


        //Internal functions
        private static int Round(float n) { return (int)Math.Round(n); }
    }
}
