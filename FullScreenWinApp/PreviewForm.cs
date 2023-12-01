using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ImagesAdvanced;
namespace FullScreenWinApp
{
    public partial class PreviewForm : Form
    {
        private readonly BrowserWithRotationCache _browser;

        public PreviewForm(BrowserWithRotationCache browser)
        {
            InitializeComponent();

            DoubleBuffered = true;
            
            _browser = browser;
            _browser.ImageChanged += (o, e) => BackgroundImage = (Image)_browser.CurrentImage?.Clone(); ;
  
            BackgroundImage = (Image)_browser.CurrentImage?.Clone();
        }

        //TODO: Add zoom (remember it too)
        //TODO: Remember modified time
        ////https://www.codeproject.com/Articles/21097/PictureBox-Zoom

        private void PreviewForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape: _browser.SaveCachedActions(); Hide(); break;
                case Keys.Left: _browser.ProceedPrevious(); break;
                case Keys.Right: _browser.ProceedNext(); break;

                case Keys.Up:
                    {   //ok
                        if (_browser.CurrentImage is null) return;

                        RotateFlipType rotate = !e.Control ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone;
                        _browser.AddRotateAction(rotate);
                        break;
                    }
                case Keys.Down:
                    { //ok
                        if (_browser.CurrentImage is null) return;
                        RotateFlipType rotate;
                        if (e.Control) rotate = RotateFlipType.RotateNoneFlipX;
                        else rotate = RotateFlipType.RotateNoneFlipY;

                        _browser.AddRotateAction(rotate);
                        break;
                    }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        { 
            if (e.Delta > 0) _browser.ProceedPrevious(); //up (+120)
            else if (e.Delta < 0) _browser.ProceedNext(); //down (-120)
        }

    }
}
