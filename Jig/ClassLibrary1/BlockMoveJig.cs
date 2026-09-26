
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using NSVLib.Utilities;
using System;
using System.Collections.Generic;

namespace jigPractice
{
    public class BlockMoveJig : DrawJig
    {
        private Point3d _position;
        BlockReference _block;
        List<(BlockReference point, double distance)> points = new List<(BlockReference point, double distance)>();
        List<BlockReference> blks = new List<BlockReference>();
        string info;
        MText text;
        public BlockMoveJig(BlockReference block, List<BlockReference> blkList)
        {
            _block = block;
            _position = block.Position;
            blks = blkList;
        }

        public Point3d Position => _position;

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options =
                new JigPromptPointOptions("\nSpecify new location: ");

            PromptPointResult result =
                prompts.AcquirePoint(options);

            if (result.Status != PromptStatus.OK)
                return SamplerStatus.Cancel;

            if (result.Value.IsEqualTo(
                _position,
                new Tolerance(1e-6, 1e-6)))
            {
                return SamplerStatus.NoChange;
            }

            _position = result.Value;

            var dg = new DangerDetection(_block , blks);
            points = dg.Detect(_position);

            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            BlockReference block = _block;
            BlockReference DBlk;
            block.Position = _position;
            draw.Geometry.Draw(block);

            if (points.Count > 0)
            {
                // * TODO: memmory leaks
                Circle c = new Circle
                {
                    ColorIndex = 1,
                };
                c.Center = _position;
                draw.Geometry.Draw(c);

                foreach (var bk in points)
                {
                    DBlk = bk.point;
                    DataWriter(DBlk);
                    c.Center = DBlk.Position;
                    c.Radius = bk.distance;
                    draw.Geometry.Draw(c);
                    draw.Geometry.Draw(text);
                }
            }
            return true;
        }

        private void DataWriter(BlockReference bk)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data = XDataUtil.ReadXData(bk.ObjectId, "aaa");
            string z = data["Zfactor"];
            string dist = data["dist"];
            string d = data["Danger"];
            info = $@"Z : {z}
            distance : {dist}
            Danger Distance : {d}";
            if (!string.IsNullOrEmpty(info))
            {
                text = new MText
                {
                    Location = bk.Position + new Vector3d(5, 5, 0),
                    TextHeight = 1.5,
                    Contents = info,
                    ColorIndex = 3,
                    Attachment = AttachmentPoint.BottomLeft
                };
            }
        }
    }
}
