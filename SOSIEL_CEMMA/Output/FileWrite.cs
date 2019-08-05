using System;
using System.IO;

namespace SOSIEL_CEMMA.Output
{
    class FileWrite
    {
        public String Name { get; set; } = "Steps.html";
        public String FolderName { get; set; }
        readonly StreamWriter fileStream;
        Agent agent;
        public FileWrite(String Folder = null)
        {
            if (Folder != null)
                FolderName = Folder + "/";
            fileStream = new StreamWriter(File.Create(FolderName + Name));
            fileStream.WriteLine("<html>");
            fileStream.WriteLine("<head>");
            fileStream.WriteLine("<style type='text/css'>");
            fileStream.WriteLine("table { border-collapse: collapse; font-family:Arial }");
            fileStream.WriteLine(".indexc { background-color:#333333; color:white; font-size:10px; font-style:italic; height:20px }");
            fileStream.WriteLine(".indexr { background-color:#333333; color:white; font-size:10px; font-style:italic; width:20px }");
            fileStream.WriteLine("td { padding:0px; text-align: center; vertical-align: middle;  width:35px; height:35px; border:solid 1px; gray; background-color:#FFFFCC  }");
            fileStream.WriteLine(".contrib { background-color:#FF9933; }");
            fileStream.WriteLine(".noncontrib {  background-color:#3300FF; color:white  }");
            fileStream.WriteLine("div {  font-family:monospace  }");
            fileStream.WriteLine("</style>");
            fileStream.WriteLine("</head>");
            fileStream.WriteLine("<body>");
        }

        public void Add(String str)
        {
            fileStream.WriteLine("<div>" + str + "</div>");
        }
        public void Add(SocialSpace sosialspace)
        {
            fileStream.WriteLine("<table>");
            fileStream.WriteLine("<tr><td style='width:20px; height:20px; border:none; background-color:white'></td>");
            for (int c = 0; c < sosialspace.Rows; c++)
                fileStream.WriteLine($"<td class='indexc'>{c}</td>");
            fileStream.WriteLine("</tr>");

            for (int r = 0; r < sosialspace.Rows; r++)
            {
                fileStream.WriteLine("<tr>");
                fileStream.WriteLine($"<td class='indexr'>{r}</td>");
                for (int c = 0; c < sosialspace.Cols; c++)
                {
                    agent = sosialspace[r, c];
                    if (agent == null)
                    {
                        fileStream.WriteLine("<td></td>");
                        continue;
                    }
                    fileStream.WriteLine($"<td class='{(agent.Contrib?"contrib":"noncontrib")}'>" +  agent.Id + "</td>");
                }

                fileStream.WriteLine("</tr>");
            }

            fileStream.WriteLine("</table>");
            fileStream.WriteLine("<br />");
        }

        public void Close()
        {
            fileStream.WriteLine("</body>");
            fileStream.WriteLine("</html>");
            fileStream.Close();
        }
    }
}
