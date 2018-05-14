using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;

namespace Q3_K142182
{
    public partial class Service1 : ServiceBase
    {
        Timer mytimer = null;
        public static string logfilepath = ConfigurationManager.AppSettings["logfilepath"];
        public static string newxmlfile = ConfigurationManager.AppSettings["newxmlfile"];

        public Service1()
        {
            InitializeComponent();


        }
        protected override void OnStart(string[] args)
        {
            ServiceConsole("Service is Started");
            mytimer = new Timer();
            this.mytimer.Interval = 5*60000;
            this.mytimer.Elapsed += new ElapsedEventHandler(this.timer1_Tick);
            this.mytimer.Enabled = true;

        }

        protected override void OnStop()
        {
            ServiceConsole("Service Stoped");
            mytimer.Enabled = false;

        }

        private static void ServiceConsole(string content)
        {
            //Folder Must Exists
            FileStream fs = new FileStream(logfilepath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }
       

        private void timer1_Tick(object sender, EventArgs e)
        {
            String document_name = string.Format(newxmlfile+"RSSfeed-{0:yyyy-MM-dd_hh-mm}.xml", DateTime.Now);
            XmlDocument rssXmlDoc = new XmlDocument();
            XmlDocument rssXmlDoc2 = new XmlDocument();

            // Load the RSS file from the RSS URL
            rssXmlDoc.Load("http://feeds.feedburner.com/GeoBulletins");
            rssXmlDoc2.Load("http://www.dawn.com/feeds/home");

            // Parse the Items in the RSS file
            XmlNodeList rssNodes = rssXmlDoc.SelectNodes("rss/channel/item");
            XmlNodeList rssNodes2 = rssXmlDoc2.SelectNodes("rss/channel/item");



            //StringBuilder rssContent = new StringBuilder();
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("DataSet");
            xmlDoc.AppendChild(rootNode);
            // Iterate through the items in the RSS file
            foreach (XmlNode rssNode in rssNodes)
            {
                XmlNode rssSubNode = rssNode.SelectSingleNode("title");
                string title = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("channel");
                string newschannel = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("description");
                string description = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("pubDate");
                string pubdate = rssSubNode != null ? rssSubNode.InnerText : "";

                //create new xml document


                XmlNode newsitemNode = xmlDoc.CreateElement("NewsItem");
                rootNode.AppendChild(newsitemNode);

                XmlNode titleNode = xmlDoc.CreateElement("Title");
                titleNode.InnerText = clean_string(title);
                newsitemNode.AppendChild(titleNode);

                XmlNode publishdateNode = xmlDoc.CreateElement("PublishedDate");
                publishdateNode.InnerText = clean_string(pubdate);
                newsitemNode.AppendChild(publishdateNode);

                XmlNode newschannelNode = xmlDoc.CreateElement("channel");
                newschannelNode.InnerText = "Geo News";
                newsitemNode.AppendChild(newschannelNode);

                XmlNode descriptionNode = xmlDoc.CreateElement("Description");
                descriptionNode.InnerText = clean_string(description);
                newsitemNode.AppendChild(descriptionNode);


                xmlDoc.Save(document_name);


                //rssContent.Append("link: \n" + newschannel + "\ntitle:" + title + "\ndescription: " + description + "\npubdate: " + pubdate + "\n\n\n");
            }
            ServiceConsole("Document 1 saved  "+DateTime.Now.ToString());
            // Iterate through the items in the RSS file
            foreach (XmlNode rssNode2 in rssNodes2)
            {
                XmlNode rssSubNode2 = rssNode2.SelectSingleNode("title");
                string title = rssSubNode2 != null ? rssSubNode2.InnerText : "";

                rssSubNode2 = rssNode2.SelectSingleNode("channel");
                string newschannel = rssSubNode2 != null ? rssSubNode2.InnerText : "";

                rssSubNode2 = rssNode2.SelectSingleNode("description");
                string description = rssSubNode2 != null ? rssSubNode2.InnerText : "";

                rssSubNode2 = rssNode2.SelectSingleNode("pubDate");
                string pubdate = rssSubNode2 != null ? rssSubNode2.InnerText : "";

                //create new xml document


                XmlNode newsitemNode = xmlDoc.CreateElement("NewsItem");
                rootNode.AppendChild(newsitemNode);

                XmlNode titleNode = xmlDoc.CreateElement("Title");
                titleNode.InnerText = clean_string(title);
                newsitemNode.AppendChild(titleNode);

                XmlNode publishdateNode = xmlDoc.CreateElement("PublishedDate");
                publishdateNode.InnerText = clean_string(pubdate);
                newsitemNode.AppendChild(publishdateNode);

                XmlNode newschannelNode = xmlDoc.CreateElement("channel");
                newschannelNode.InnerText = "DawnNews";
                newsitemNode.AppendChild(newschannelNode);

                XmlNode descriptionNode = xmlDoc.CreateElement("Description");
                descriptionNode.InnerText = clean_string(description);
                newsitemNode.AppendChild(descriptionNode);


                xmlDoc.Save(document_name);


                //rssContent.Append("link: \n" + newschannel + "\ntitle:" + title + "\ndescription: " + description + "\npubdate: " + pubdate + "\n\n\n");
            }
            ServiceConsole("Document 2 saved  " + DateTime.Now.ToString());
            // Return the string that contain the RSS items
            //String returner = rssContent.ToString();
            //Console.WriteLine(returner);

            sort_document(newxmlfile);
            ServiceConsole("Document sorted");
            Console.ReadKey();



        }
        public static string clean_string(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        public static void sort_document(string document)
        {
            XElement root = XElement.Load(document);
            var orderedtabs = root.Elements("NewsItem")
                                  .OrderByDescending(xtab => (DateTime)xtab.Element("PublishedDate"))
                                  .ToArray();
            root.RemoveAll();
            foreach (XElement tab in orderedtabs)
                root.Add(tab);
            ServiceConsole(document);
            root.Save(document);




        }
    }
}
