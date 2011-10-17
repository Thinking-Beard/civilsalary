using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf.parser;

namespace civilsalary.datasync
{
    public class TableTextExtractionStrategy : ITextExtractionStrategy
    {

        private Vector lastStart;
        private Vector lastEnd;

        /** used to store the resulting String. */
        private StringBuilder result = new StringBuilder();

        public void BeginTextBlock()
        {
        }

        public void EndTextBlock()
        {
        }

        public String GetResultantText()
        {
            return result.ToString();
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
            bool hardReturn = false;

            LineSegment segment = renderInfo.GetBaseline();
            Vector start = segment.GetStartPoint();
            Vector end = segment.GetEndPoint();

            if (lastStart != null && lastEnd != null)
            {
                Vector x0 = start;
                Vector x1 = lastStart;
                Vector x2 = lastEnd;

                // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
                float dist = (x2.Subtract(x1)).Cross((x1.Subtract(x0))).LengthSquared / x2.Subtract(x1).LengthSquared;

                float sameLineThreshold = 1f; // we should probably base this on the current font metrics, but 1 pt seems to be sufficient for the time being
                if (dist > sameLineThreshold)
                    hardReturn = true;

                // Note:  Technically, we should check both the start and end positions, in case the angle of the text changed without any displacement
                // but this sort of thing probably doesn't happen much in reality, so we'll leave it alone for now
            }

            if (hardReturn)
            {
                //System.out.Println("<< Hard Return >>");
                result.Append('\n');
            }
            else if (lastStart != null && lastEnd != null)
            {
                if (result[result.Length - 1] != ' ' && renderInfo.GetText()[0] != ' ')
                { // we only insert a blank space if the trailing character of the previous string wasn't a space, and the leading character of the current string isn't a space
                    float spacing = lastEnd.Subtract(start).Length;
                    if (spacing > renderInfo.GetSingleSpaceWidth() / 2f)
                    {
                        result.Append('\t');
                        //System.out.Println("Inserting implied space before '" + renderInfo.GetText() + "'");
                    }
                }
            }
            else
            {
                //System.out.Println("Displaying first string of content '" + text + "' :: x1 = " + x1);
            }

            //System.out.Println("[" + renderInfo.GetStartPoint() + "]->[" + renderInfo.GetEndPoint() + "] " + renderInfo.GetText());
            result.Append(renderInfo.GetText());

            lastStart = start;
            lastEnd = end;
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            // do nothing - we aren't tracking images in this renderer
        }
    }
}
