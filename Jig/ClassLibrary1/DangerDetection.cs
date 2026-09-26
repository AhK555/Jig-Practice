using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NSVLib.Utilities;
using System;
using System.Collections.Generic;

namespace jigPractice
{
    public class DangerDetection
    {
        private ObjectId _movingBlockID;
        public List<BlockReference> blocks = new List<BlockReference>();
        public BlockReference movingBlock;

        public DangerDetection(BlockReference block , List<BlockReference> blks)
        {
            movingBlock = block;
            _movingBlockID = block.ObjectId;
            blocks = blks;
        }

        public List<(BlockReference point, double distance, string shape)> Detect(Point3d position)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var objectPosition = new List<Point3d>();
            List<(BlockReference point, double distance , string shape)> points = new List<(BlockReference point, double distance, string shape)>();
            Dictionary<string,string> data = new Dictionary<string,string>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead)as BlockTableRecord;
                data = XDataUtil.ReadXData(movingBlock.ObjectId, "aaa");
                double movingBlockDanger = double.Parse(data["Danger"]);
                foreach (BlockReference blkRef in blocks)
                {
                    if (blkRef.ObjectId == _movingBlockID) continue;
                    data = XDataUtil.ReadXData(blkRef.ObjectId, "aaa");

                    string shape = data["Shape"];  
                    double _dangerDistance = double.Parse(data["Danger"]);
                    double distance = blkRef.Position.DistanceTo(movingBlock.Position);
                    if (distance <= Math.Max(movingBlockDanger,_dangerDistance))
                        points.Add((blkRef, Math.Max(movingBlockDanger, _dangerDistance), shape));
                    Dictionary<string, string> map = new Dictionary<string, string>();
                }
                tr.Commit();
            }
            return points;
        }
    }
}