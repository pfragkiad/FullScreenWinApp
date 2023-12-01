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

        public PreviewForm(IConfiguration configuration)
        {
            InitializeComponent();

            DoubleBuffered = true;
            
            _configuration = configuration;

            string? initialDirectory = _configuration["browser:initialDirectory"];
            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
                SetDirectory(initialDirectory!);

        }
        List<string> extensions = new();

        //TODO: Add zoom (remember it too)
        //TODO: Cache directory in settings

        //https://www.codeproject.com/Articles/21097/PictureBox-Zoom

        private Image? _image = null;
        private string? _currentPath = null;
        private string? _rotationCachePath;

        private List<string> _imageFiles =new ();
        private int _currentIndex = -1;

        public void SetDirectory(string directoryName)
        {
            var extensions = _configuration.GetSection("browser:extensions").GetChildren().Select(c=> c.Value).ToList();
           
            //todo: read image extensions from config file
            _imageFiles = Directory
                .GetFiles(directoryName, "*.*")
                .Where(f=>extensions.Contains(Path.GetExtension(f),StringComparer.OrdinalIgnoreCase))
                .ToList();

            //_imageFiles = Directory.GetFiles(directoryName).Where(ImageExtensions.IsImageFile).ToList();
            _currentIndex = -1;
            ProceedNext();
        }

        public void SetImage(string filePath)
        {
            _currentPath = filePath;
            _image = Image.FromFile(filePath);
            BackgroundImage = _image;

            _rotationCachePath = Path.Combine(Application.StartupPath, "rotation_cache.txt");
            if (!File.Exists(_rotationCachePath))
            {
                _actions = new();
                return;
            }

            var record = RotationCacheFileRecord.ReadFromFile(_rotationCachePath, Path.GetFileName(_currentPath));

            if (record is null)
            {
                _actions = new();
                return;
            }

            _actions = record.Rotations;
            _image.ApplyAllRotations(_actions);
        }

        //cached rotation!
        List<RotateFlipType> _actions = [];
        private readonly IConfiguration _configuration;

        private void PreviewForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape: SaveCachedActions(); Hide(); break;
                case Keys.Left: ProceedPrevious(); break;
                case Keys.Right: ProceedNext(); break;

                case Keys.Up:
                    {   //ok
                        if (_image is null) return;
                        RotateFlipType rotate = !e.Control ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone;
                        //var newImage = (Image)_image!.Clone();
                        _image.RotateFlip(rotate);
                        _actions.Add(rotate);
                        BackgroundImage = (Image)_image.Clone();
                        break;
                    }
                case Keys.Down:
                    { //ok
                        if (_image is null) return;
                        RotateFlipType rotate;
                        //if (e.Shift && e.Control) rotate = RotateFlipType.RotateNoneFlipNone ;
                        //else
                        //if (e.Shift) rotate = RotateFlipType.RotateNoneFlipX;
                        if (e.Control) rotate = RotateFlipType.RotateNoneFlipX;
                        else rotate = RotateFlipType.RotateNoneFlipY;

                        _actions.Add(rotate);

                        //var newImage = (Image)_image!.Clone();
                        _image.RotateFlip(rotate);
                        BackgroundImage = (Image)_image.Clone();
                        break;
                    }
            }
        }

        public void SaveCachedActions() =>
            _actions.AppendRotationsActionsToCacheFile(_currentPath, _rotationCachePath);
        

        protected override void OnMouseWheel(MouseEventArgs e)
        { 
            if (e.Delta > 0) ProceedPrevious(); //up (+120)
            else if (e.Delta < 0) ProceedNext(); //down (-120)
        }

        private void ProceedNext() {
            if (_imageFiles.Count == 0) return;
            if (_actions.Count > 0) SaveCachedActions();
            
            _currentIndex++;
            if (_currentIndex == _imageFiles.Count) _currentIndex = 0;


            SetImage(_imageFiles[_currentIndex]);
        }

        private void ProceedPrevious() {
            if (_imageFiles.Count == 0) return;
            if (_actions.Count > 0) SaveCachedActions();

            _currentIndex--;
            if (_currentIndex == -1) _currentIndex = _imageFiles.Count - 1;
            SetImage(_imageFiles[_currentIndex]);
        
        }
    }
}
