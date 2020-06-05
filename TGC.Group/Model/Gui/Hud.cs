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
using TGC.Group.Model.Crafting;
using System.Runtime.CompilerServices;

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

        struct ItemSprite
        {
            public CustomSprite icon;
            public CustomSprite background;
            public TgcText2D amount;
            public void Dispose() { 
                background.Dispose();
                amount.Dispose();
                if (icon != null)
                    icon.Dispose();
            }
        }

        static Inventory Inventory;

        //Control vars
        static Status CurrentStatus = Status.MainMenu;
        static Status LastStatus = Status.MainMenu;

        static TgcText2D SelectedText;
        static int SelectedItemIndex;

        //"Consts"
        static int WIDTH;
        static int HEIGHT;
        const int MAX_ITEM_SPRITES = 30; //Deberia ser multiplo de 10.

        //Drawing vars
        static Drawer2D drawer;
        static TgcText2D Start;
        static TgcText2D Exit;

        static CustomSprite Logo;
        static CustomSprite Overlay; //Inventario y crafting comparten el mismo sprite para el fondo
        static CustomSprite Background;

        static CustomSprite ItemBackgroundPreset;
        static List<ItemSprite> ItemSprites;

        static TgcText2D GameOver;

        




        public static void Init(string MediaDir, Inventory inventory)
        {
            WIDTH = D3DDevice.Instance.Width;
            HEIGHT = D3DDevice.Instance.Height;

            Inventory = inventory;

            ItemSprites = new List<ItemSprite>();
            SelectedItemIndex = 0;

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
            spriteSize = Overlay.Bitmap.Size;
            Overlay.Position = new TGCVector2(WIDTH / 2 - spriteSize.Width / 2, HEIGHT / 2 - spriteSize.Height / 2);

            //Load item background preset to be used on every item sprite
            ItemBackgroundPreset = new CustomSprite();
            ItemBackgroundPreset.Bitmap = new CustomBitmap(MediaDir + "item_placeholder.png", D3DDevice.Instance.Device);
            spriteSize = ItemBackgroundPreset.Bitmap.Size;
            //10 items per line
            for (int j = 0; j <= MAX_ITEM_SPRITES / 10 - 1; j++)
            {
                for (int i = 0; i <= 9; i++)
                {
                    var item = new ItemSprite();

                    item.background = new CustomSprite();
                    item.background.Bitmap = ItemBackgroundPreset.Bitmap;
                    var xoffset = 45 + i * spriteSize.Width * 1.5f;
                    var yoffset = 25 + j * spriteSize.Height * 1.5f;
                    item.background.Position = Overlay.Position + new TGCVector2(xoffset, yoffset);
                    item.background.Color = Color.CadetBlue;

                    item.amount = new TgcText2D();
                    item.amount.Align = TgcText2D.TextAlign.LEFT;
                    item.amount.Text = "";
                    var xpos = Round(item.background.Position.X + spriteSize.Width / 3);
                    var ypos = Round(item.background.Position.Y + spriteSize.Height / 2);
                    item.amount.Position = new Point(xpos, ypos);
                    item.amount.Size = new Size(25, 15);
                    item.amount.changeFont(new Font("Calibri", 15, FontStyle.Bold));
                    item.amount.Color = Color.White;

                    ItemSprites.Add(item);
                }
            }

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

        public static void Update(TgcD3dInput Input)
        {
            bool up = Input.keyDown(Key.Up);
            bool down = Input.keyDown(Key.Down);
            bool left = Input.keyDown(Key.Left);
            bool right = Input.keyDown(Key.Right);
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
            else if (CurrentStatus == Status.Inventory || CurrentStatus == Status.Crafting)
            {
                if(LastStatus != Status.Inventory && LastStatus != Status.Crafting)
                    UpdateIconSprites();

                ItemSprites[SelectedItemIndex].background.Color = Color.CadetBlue;

                if (up)
                    SelectedItemIndex -= 10;
                else if (down)
                    SelectedItemIndex += 10;
                else if (left)
                    SelectedItemIndex--;
                else if (right)
                    SelectedItemIndex++;

                SelectedItemIndex = FastMath.Clamp(SelectedItemIndex, 0, MAX_ITEM_SPRITES - 1);
                ItemSprites[SelectedItemIndex].background.Color = Color.Orange;
            }

            //Update last status so it doesn't UpdateIconSprites() multiple times
            LastStatus = CurrentStatus;
        }

            public static void Render()
        {
            //If there's no HUD to be rendered then skip testing status
            if(CurrentStatus == Status.None) { return; }

            //Main Menu
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
            //Inventory + Crafting
            else if (CurrentStatus == Status.Inventory || CurrentStatus == Status.Crafting)
            {
                //Draw sprites
                drawer.BeginDrawSprite();
                drawer.DrawSprite(Overlay);
                

                foreach (var item in ItemSprites)
                {
                    drawer.DrawSprite(item.background);
                    if(item.icon != null)
                        drawer.DrawSprite(item.icon);
                }
                drawer.EndDrawSprite();

                //Draw text
                foreach (var item in ItemSprites)
                    item.amount.render();

                //Show crafting + inventory
                if (CurrentStatus == Status.Crafting)
                {

                }
            }
            //GameOver
            else if (CurrentStatus == Status.GameOver)
            {
                GameOver.render();
            }
        }

        public static void Dispose()
        {
            Start.Dispose();
            Exit.Dispose();
            GameOver.Dispose();
            Logo.Dispose();
            Background.Dispose();
            Overlay.Dispose();
            foreach (var item in ItemSprites)
                item.Dispose();
        }

        //Interactive functions
        public static void ChangeStatus(Status status) { 
            LastStatus = CurrentStatus; 
            CurrentStatus = status; 
        }

        //Internal functions
        private static int Round(float n) { return (int)Math.Round(n); }

        private static void UpdateIconSprites()
        {
            List<Item> InventoryItems = Inventory.GetList();
            for (int i=0;i < InventoryItems.Count;i++)
            {
                ItemSprites[i].amount.Text = InventoryItems[i].Amount().ToString();
            }
        }
    }
}
