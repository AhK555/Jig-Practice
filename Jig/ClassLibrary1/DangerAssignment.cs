using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using NSVLib.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class DangerAssignment
    {
        public double zFactor;
        public double dist;
        public List<BlockReference> AssignShit()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            List<BlockReference> blocks = new List<BlockReference>();

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
                BlockReference block = AssignRealShit(sel.ObjectId);
                blocks.Add(block);
            }

            return blocks;
        }
        public BlockReference AssignRealShit(ObjectId id)
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

                double danger = Math.Sqrt(zFactor *  zFactor + dist * dist);
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("dist", dist.ToString());
                map.Add("Zfactor", zFactor.ToString());
                map.Add("Danger", danger.ToString());
                XDataUtil.SetXdata(tr, blk, "aaa", map);

                tr.Commit();

                return blk;
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
            zFactor = zResults.Value;
            dist = distanceResult.Value;
        }

    }
}
