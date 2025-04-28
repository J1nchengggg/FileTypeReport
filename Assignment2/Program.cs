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
		    yield return f.Name;
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

      while (size >= 1000 && unitIndex < units.Length - 1) {
	      size /= 1000;
	      unitIndex++;
      }

      return $"{size:F2} {units[unitIndex]}";
    }
    

    // Create an HTML report file
    private static XDocument CreateReport(IEnumerable<string> files) {
      // 2. Process data
      var query =
        from file in files
        // TODO: Fill in your code here.
        let extension = Path.GetExtension(file).ToLower()
        group file by extension into g
        let totalSize = g.Sum(f => new FileInfo(f).Length)
        orderby totalSize descending
        select new {
          Type = g.Key,
          Count = g.Count(),
          TotalSize = totalSize
        };

      // 3. Functionally construct XML
      var alignment = new XAttribute("align", "right");
      var style = "table, th, td { border: 1px solid black; }";

      var tableRows = query.Select(item =>
        new XElement("tr",
          new XElement("td", item.Type),
          new XElement("td", alignment, item.Count),
          new XElement("td", alignment, FormatByteSize(item.TotalSize))
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
    public static void Main(string[] args) {
      try {
        string inputFolder = args[0];
        string reportFile  = args[1];
        CreateReport(EnumerateFilesRecursively(inputFolder)).Save(reportFile);
      } catch {
        Console.WriteLine("Usage: FileTypeReport <folder> <report file>");
      }
    }
  }
}