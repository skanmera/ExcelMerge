using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FastWpfGrid
{
    public class FastGridBlockImpl : IFastGridCellBlock
    {
        public FastGridBlockType BlockType { get; set; }
        public Color? FontColor { get; set; }
        public bool IsItalic { get; set; }
        public bool IsBold { get; set; }
        public string TextData { get; set; }
        public string ImageSource { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public MouseHoverBehaviours MouseHoverBehaviour { get; set; } 
        public object CommandParameter { get; set; }
        public string ToolTip { get; set; }

        public FastGridBlockImpl()
        {
            MouseHoverBehaviour = MouseHoverBehaviours.ShowAllWhenMouseOut;
        }
    }

    public class FastGridCellImpl : IFastGridCell
    {
        public Color? BackgroundColor { get; set; }
        public CellDecoration Decoration { get; set; }
        public Color? DecorationColor { get; set; }
        public string ToolTipText { get; set; }
        public TooltipVisibilityMode ToolTipVisibility { get; set; }

        public List<FastGridBlockImpl> Blocks = new List<FastGridBlockImpl>();

        public int BlockCount
        {
            get { return Blocks.Count; }
        }

        public int RightAlignBlockCount { get; set; }

        public IFastGridCellBlock GetBlock(int blockIndex)
        {
            return Blocks[blockIndex];
        }

        public string GetEditText()
        {
            return null;
        }

        public void SetEditText(string value)
        {
        }

        public IEnumerable<FastGridBlockImpl> SetBlocks
        {
            set
            {
                Blocks.Clear();
                Blocks.AddRange(value);
            }
        }

        public FastGridBlockImpl AddImageBlock(string image, int width = 16, int height = 16)
        {
            var res = new FastGridBlockImpl
                {
                    BlockType = FastGridBlockType.Image,
                    ImageWidth = width,
                    ImageHeight = height,
                    ImageSource = image,
                };
            Blocks.Add(res);
            return res;
        }

        public FastGridBlockImpl AddTextBlock(object text)
        {
            var res = new FastGridBlockImpl
                {
                    BlockType = FastGridBlockType.Text,
                    TextData = text == null ? null : text.ToString(),
                };
            Blocks.Add(res);
            return res;
        }
    }
}
