﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FullScreenWinApp
{
    public partial class PreviewForm : Form
    {

        public PreviewForm()
        {
            InitializeComponent();

            DoubleBuffered = true;
        }


        //https://www.codeproject.com/Articles/21097/PictureBox-Zoom

        private Image? _image = null;
        private string? _currentPath = null;

        private string? _rotationCachePath;

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

            var records = File
                .ReadAllLines(_rotationCachePath)
                .FirstOrDefault(l => l.Split('\t')[0] == Path.GetFileName(_currentPath))?
                .Split('\t')[1]
                .Split('-')
                .Select(s => Enum.Parse<RotateFlipType>(s))
                .ToList();

            if (records is null)
            {
                _actions = new();
                return;
            }

            _actions = records!;
            foreach (var a in _actions)
                _image.RotateFlip(a);
        }

        //cached rotation!
        List<RotateFlipType> _actions;

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

        public void SaveCachedActions()
        {

            if (_actions.Count == 0) return;

            //simplify actions
            //remove 2 consecutive RotateNoneFlipY/RotateNoneFlipX
            for (int i = _actions.Count - 1; i >= 1; i--)
            {
                if (_actions[i] == RotateFlipType.RotateNoneFlipX && _actions[i - 1] == RotateFlipType.RotateNoneFlipX
                    || _actions[i] == RotateFlipType.RotateNoneFlipY && _actions[i - 1] == RotateFlipType.RotateNoneFlipY)
                {
                    _actions.RemoveAt(i);
                    _actions.RemoveAt(i - 1);
                    i--;
                }
            }

            //remove 4 consecutive Rotate90FlipNone/Rotate270FlipNone
            for (int i = _actions.Count - 1; i >= 3; i--)
            {
                if (_actions[i] == RotateFlipType.Rotate90FlipNone && _actions[i - 1] == RotateFlipType.Rotate90FlipNone
                    && _actions[i - 2] == RotateFlipType.Rotate90FlipNone && _actions[i - 3] == RotateFlipType.Rotate90FlipNone
                    || _actions[i] == RotateFlipType.Rotate270FlipNone && _actions[i - 1] == RotateFlipType.Rotate270FlipNone
                    && _actions[i - 2] == RotateFlipType.Rotate270FlipNone && _actions[i - 3] == RotateFlipType.Rotate270FlipNone)
                {
                    _actions.RemoveAt(i);
                    _actions.RemoveAt(i - 1);
                    _actions.RemoveAt(i - 2);
                    _actions.RemoveAt(i - 3);
                    i -= 3;
                }
            }

            //remove pairs of Rotate90FlipNone/Rotate270FlipNone or Rotate270FlipNone/Rotate90FlipNone
            for (int i = _actions.Count - 1; i >= 1; i--)
            {
                if (_actions[i] == RotateFlipType.Rotate90FlipNone && _actions[i - 1] == RotateFlipType.Rotate270FlipNone
                    || _actions[i] == RotateFlipType.Rotate270FlipNone && _actions[i - 1] == RotateFlipType.Rotate90FlipNone)
                {
                    _actions.RemoveAt(i);
                    _actions.RemoveAt(i - 1);
                    i--;
                }
            }

            if (_actions.Count == 0) return;

            string actions = string.Join("-", _actions);

            var records = File.Exists(_rotationCachePath) ?
                File
               .ReadAllLines(_rotationCachePath)
               .Where(l => l.Split('\t')[0] != Path.GetFileName(_currentPath)) : [];

            //rewrite all recrods except the current one
            if (records.Any())
                File.WriteAllLines(_rotationCachePath, records);
            else
                File.Delete(_rotationCachePath);

            //append the current path
            File.AppendAllText(_rotationCachePath, $"{Path.GetFileName(_currentPath)}\t{actions}\r\n");
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0) ProceedPrevious(); //up (+120)
            else if (e.Delta < 0) ProceedNext(); //down (-120)
        }

        private void ProceedNext() { MessageBox.Show("Next"); }

        private void ProceedPrevious() { MessageBox.Show("Previous"); }
    }
}
