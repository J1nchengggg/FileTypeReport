// Program.cs
//
// CECS 342 Assignment 2
// File Type Report
// Solution Template

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FileTypeReport {
  internal static class Program {
    // 1. Enumerate all files in a folder recursively
    private static IEnumerable<string> EnumerateFilesRecursively(string path) {
      	DirectoryInfo place  = new DirectoryInfo(path); //get the directory path
		FileInfo[] files = place.GetFiles(); //get all the files and store in files
		DirectoryInfo[] dirs = place.GetDirectories(); //get all the other directories and store in dirs
		
		//loop and yield all files
		foreach (FileInfo f in files) {           
		    yield return f.FullName;
		}
		
		//loop and go to all sub directories recursively
		foreach (DirectoryInfo d in dirs) {
			foreach (string file in EnumerateFilesRecursively(d.FullName)) { //using FullName to get the path
	            yield return file;
	        }
		}
    }

    // Human readable byte size
    private static string FormatByteSize(long byteSize) {
      // TODO: Fill in your code here.
      string[] units = { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB" };
      double size = byteSize;
      int unitIndex = 0;
      //Start with the original number of bytes.
      while (size >= 1000 && unitIndex < units.Length - 1) {
	      size /= 1000;
	      unitIndex++;
      }
      //While the size is >= 1000: Divide by 1000. Move to the next unit (B → kB → MB → etc.).

      return $"{size:F2} {units[unitIndex]}";
      //format the number with 2 digits after the decimal point.
    }
    

    // Create an HTML report file
    private static XDocument CreateReport(IEnumerable<string> files) {
      // 2. Process data
      var query =
        from file in files // for each file 
        let extension = Path.GetExtension(file).ToLower() 
        //get its extension (like .txt, .jpg) and make lowercase.
        group file by extension into g
        //group all files by type.
        let totalSize = g.Sum(f => new FileInfo(f).Length)
        //add up the file sizes.
        orderby totalSize descending
        //biggest types first.
        select new {
          Type = g.Key,
          Count = g.Count(),
          TotalSize = totalSize
        };//create a new object with Type, Count, TotalSize.

      // 3. Functionally construct XML
      var alignment = new XAttribute("align", "right");
      var style = "table, th, td { border: 1px solid black; }";

      var tableRows = query.Select(item =>
        new XElement("tr",
          new XElement("td", item.Type),
          new XElement("td", alignment, item.Count),
          new XElement("td", alignment, FormatByteSize(item.TotalSize))
          //new XElement("tr", ...) → makes a new table row <tr>.
          //Each td (cell) shows: File type, Count, Formatted total size (using FormatByteSize()!) 
        )
      );
        
      var table = new XElement("table",
        new XElement("thead",
          new XElement("tr",
            new XElement("th", "Type"),
            new XElement("th", "Count"),
            new XElement("th", "Total Size"))),
        new XElement("tbody", tableRows));

      return new XDocument(
        new XDocumentType("html", null, null, null),
          new XElement("html",
            new XElement("head",
              new XElement("title", "File Report"),
              new XElement("style", style)),
            new XElement("body", table)));
    }

    // Console application with two arguments
    public static void Main(string[] args) 
    //args is a list of strings you type when you run the program (command line arguments)
    {
      try {
        string inputFolder = args[0];//args[0] = first input = folder to scan.
        string reportFile  = args[1];//args[1] = second input = file path to save HTML report.
        CreateReport(EnumerateFilesRecursively(inputFolder)).Save(reportFile);
      } //Calls EnumerateFilesRecursively(inputFolder) → get all files inside.
      //Passes that list to CreateReport(). and Saves the output as the HTML file.
      catch {
        Console.WriteLine("Usage: FileTypeReport <folder> <report file>");
      }
    }
  }
}