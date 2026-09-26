using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NSVLib.Utilities;
using System.Collections.Generic;

namespace jigPractice
{
    public class DangerDetection
    {
        private ObjectId _movingBlockID;
        private double _dangerDistance;
        public List<BlockReference> blocks = new List<BlockReference>();
        public BlockReference movingBlock;

        public DangerDetection(BlockReference block , List<BlockReference> blks)
        {
            movingBlock = block;
            _movingBlockID = block.ObjectId;
            blocks = blks;
        }

        public List<(BlockReference block, double distance)> Detect(Point3d position)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var objectPosition = new List<Point3d>();
            List<(BlockReference point, double distance)> points = new List<(BlockReference point, double distance)>();
            Dictionary<string,string> data = new Dictionary<string,string>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead)as BlockTableRecord;
                
                foreach (BlockReference blkRef in blocks)
                {
                    if (blkRef.ObjectId == _movingBlockID) continue;
                    data = XDataUtil.ReadXData(blkRef.ObjectId, "aaa");

                    double _dangerDistance = double.Parse(data["Danger"]);
                    double distance = blkRef.Position.DistanceTo(movingBlock.Position);
                    if (distance <= _dangerDistance)
                        points.Add((blkRef, _dangerDistance));
                    Dictionary<string, string> map = new Dictionary<string, string>();
                }
                tr.Commit();
            }
            return points;
        }
    }
}