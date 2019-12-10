using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public class HtmlHelper
    {

        public enum ToHTMLStatus
        {
            UnableToLoadTemplate,
            UnableToLocateNode,
            Success
        }

        public static ToHTMLStatus ToHTMLFile(string htmlTemplate, string htmlOutput, string html)
        {
            if (String.IsNullOrEmpty(htmlTemplate) || !System.IO.File.Exists(htmlTemplate))
            {
                return ToHTMLStatus.UnableToLoadTemplate;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.Load(htmlTemplate);

            HtmlNode dbCompareResultsNode = doc.GetElementbyId("resultsView");
            if (dbCompareResultsNode == null)
            {
                return ToHTMLStatus.UnableToLocateNode;
            }

            dbCompareResultsNode.InnerHtml = html.ToString();

            doc.Save(htmlOutput);

            return ToHTMLStatus.Success;
        }



    }
}
