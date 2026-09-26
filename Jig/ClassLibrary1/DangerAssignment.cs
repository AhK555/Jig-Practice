using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using NSVLib.Utilities;
using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    public class DangerAssignment
    {
        public double zFactor;
        public double dist;
        public string shape;
        List<BlockReference> blocks = new List<BlockReference>();
        public List<BlockReference> AssignShit()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            PromptSelectionOptions options = new PromptSelectionOptions();
            options.MessageForAdding =
                "\nSelect BlockReferences for assigning the distance: ";

            SelectionFilter filter = new SelectionFilter(
                new[]
                {
            new TypedValue((int)DxfCode.Start, "INSERT")
                });

            PromptSelectionResult result = ed.GetSelection(options, filter);

            if (result.Status != PromptStatus.OK)
                throw new Exception("wrong");

            foreach (SelectedObject sel in result.Value)
            {
                AssignRealShit(sel.ObjectId);
            }

            return blocks;
        }
        public void AssignRealShit(ObjectId id)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            { 
                BlockReference blk =
                    tr.GetObject(id, OpenMode.ForWrite) as BlockReference;
                if (blk == null)
                    throw new Exception("wrong");

                getshit(blk, ed);

                BlockTable bt =
    tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord modelSpace =
                    tr.GetObject(
                        bt[BlockTableRecord.ModelSpace],
                        OpenMode.ForRead
                    ) as BlockTableRecord;

                double danger = Math.Sqrt(zFactor *  zFactor + dist * dist);
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("dist", dist.ToString());
                map.Add("Zfactor", zFactor.ToString());
                map.Add("Danger", danger.ToString());
                map.Add("Shape", shape);

                foreach (ObjectId blkID in modelSpace)
                {
                    BlockReference similarBlocks = tr.GetObject(blkID, OpenMode.ForWrite) as BlockReference;
                    if (similarBlocks == null)
                        continue;

                    if (similarBlocks.BlockTableRecord != blk.BlockTableRecord)
                        continue;

                    XDataUtil.SetXdata(tr, similarBlocks, "aaa", map);
                    blocks.Add(similarBlocks);
                }
                tr.Commit();
            }
        }
        private  void getshit(BlockReference bllk , Editor ed)
        {
            // * double read. Unit?
            PromptDoubleOptions distanceOptions =
            new PromptDoubleOptions("\nSpecify danger distance: ");
            bllk.Highlight();
            distanceOptions.AllowNegative = false;
            distanceOptions.AllowZero = false;

            PromptDoubleResult distanceResult = ed.GetDouble(distanceOptions);

            if (distanceResult.Status != PromptStatus.OK)
                throw new Exception("wrong");

            PromptDoubleOptions zOptions =
            new PromptDoubleOptions("\nSpecify danger Z: ");
            bllk.Highlight();
            zOptions.AllowNegative = false;
            zOptions.AllowZero = false;

            PromptDoubleResult zResults = ed.GetDouble(zOptions);

            if (zResults.Status != PromptStatus.OK)
                throw new Exception("wrong");

            PromptKeywordOptions shapeOptions = new PromptKeywordOptions("\nChoose danger shape [circle/Square] : ");
            shapeOptions.Keywords.Add("Circle");
            shapeOptions.Keywords.Add("Square");
            shapeOptions.AllowNone = false;
            bllk.Highlight();

            PromptResult shapeResult = ed.GetKeywords(shapeOptions);
            if (shapeResult.Status != PromptStatus.OK)
                throw new Exception("wrong");
            shape = shapeResult.StringResult;
            zFactor = zResults.Value;
            dist = distanceResult.Value;
        }

    }
}
