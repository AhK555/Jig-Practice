
namespace jigPractice
{
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;
    using ClassLibrary1;
    using System.Collections.Generic;

    public class Class1
    {
        [CommandMethod("blkMV")]
        public static void MoveBlk ()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;
            List<BlockReference> dangerBlocks = new List<BlockReference>();
            DangerAssignment danger = new DangerAssignment();
              dangerBlocks = danger.AssignShit();

            PromptEntityOptions peo = new PromptEntityOptions("\nSelect a block refrence");
            peo.SetRejectMessage("\nPlease select a blockrefrence");
            peo.AddAllowedClass(typeof(BlockReference),false);

            PromptEntityResult per = ed.GetEntity(peo);
            if (per == null) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockReference movingblock = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as BlockReference;
                if (movingblock == null) return;

                var jig = new BlockMoveJig(movingblock,dangerBlocks);

                PromptResult drag = ed.Drag(jig);
                if (drag.Status != PromptStatus.OK)
                    return;
                movingblock.Position = jig.Position;

                tr.Commit();
            }
        }
    }
}
