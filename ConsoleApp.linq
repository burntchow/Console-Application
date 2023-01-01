<Query Kind="Program" />

using System.IO;
using System.Linq;
/*
 * Enumerate all files in a given folder recursively including the entire
 * sub-folder hierarchy.
 * References: 
 * https://learn.microsoft.com/en-us/dotnet/api/system.io.directory?view=net-7.0
 * https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=net-7.0
 */
static IEnumerable<string> EnumerateFilesRecursively(string path){
	var files = from file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories) select file;
	return files; 
}

/*
 * Format a byte size in human readable form. Use units: B, kB, MB, GB, TB, PB, EB, ZB
 * where 1kB = 1000B 
 * The numerical value should be greater or equal to 1, less than 1000, and rounded to 2 digits 
 * after the decimal point, e.g. "1.30kB" 
 */
static string FormatByteSize(long byteSize){ 
	
	string[] units = {"B","kB","MB","GB","TB","PB","EB","ZB"};
	string byteString = "";
	double newBytes = 0;

	// Compare by 10^0,3,6,9.. to get into x.xx form
	// Keep track of how many times determines the unit 
	for(int i = units.Length; i > 0; i--){
		
		double compareThreshold = Math.Pow(10, (i-1)*3); 
	
		if(byteSize >= compareThreshold){ 
			newBytes = (double)byteSize / compareThreshold;  
			byteString = String.Format("{0:0.##}", newBytes) + units[i-1];
			return byteString; 
		}
	}
	return byteString; 
}

/*
 * Create an HTML document containing a table with three columns: "Type", "Count", "Size" 
 * for the file name extension (converted to lower case), the number of files with this type, and 
 * the total size of all files with this type respectively 
 * Reference: https://learn.microsoft.com/En-Us/Dotnet/csharp/programming-guide/concepts/linq/how-to-group-files-by-extension-linq
 */
static XDocument CreateReport(IEnumerable<string> files){ 

	XDocument tableFile = new XDocument();

	IEnumerable<FileInfo> toFileInfo = from file in files
									   select new FileInfo(file); 
									   
	var queryExt = from file in toFileInfo 
				   group file by file.Extension.ToLower() into fileGroup
				   orderby fileGroup.Key ascending 
				   select fileGroup;
		 	   
	//Console.WriteLine(queryExt);
	
	XElement html = new XElement("html");
	// Head
		// Title -- File Report 
	
	XElement head = new XElement("head");
	head.Add(new XElement("title", "File Report"));
	head.Add(new XElement("style", "table, th, td { border: 1px solid black; }"));
	
	// Body
	XElement body = new XElement("body");
		// table
			// thread	 
				
	XElement table = new XElement("table");
	XElement thread = new XElement("thread"); 
				// tr
					// th -- type
					// th -- count 
					// th total size 
	XElement tr = new XElement("tr");
	tr.Add(new XElement("th", "Type"));
	tr.Add(new XElement("th", "Count"));
	tr.Add(new XElement("th", "Total Size"));
	thread.Add(tr);
				
			// tbody
	XElement tbody = new XElement("tbody");
	
	foreach(var query in queryExt){
		if(query.Key != ""){
			XElement tr2 = new XElement("tr");
			tr2.Add(new XElement("td", query.Key)); 
			tr2.Add(new XElement("td", query.Count()));	
			tr2.Add(new XElement("td", FormatByteSize(query.Sum(group => group.Length))));
				
			tbody.Add(tr2); 
		}
	}
	table.Add(thread);
	table.Add(tbody);
	body.Add(table);
	html.Add(head);
	html.Add(body);
	tableFile.Add(html);
	tableFile.Dump();
	return tableFile;
}

/*
 * Take two command line arguments. The first value is the path of the input folder 
 * and the second path of the HTML report output file. Call the functions above to 
 * create the report file 
 */
public static void Main(string[] args){ 
	// Test with my Stats Folder:
	/*
	IEnumerable<string> files = EnumerateFilesRecursively(@"C:\Users\Aveline\Documents\Ave Fall 2022\EE 381 - Probability & Stats");
	XDocument report = CreateReport(files);
	report.Save(@"C:\Users\Aveline\Documents\Ave Fall 2022\EE 381 - Probability & Stats\report.html");
	*/
	
	// Test with command line arguments 
	if(args.Length > 0){
		IEnumerable<string> files = EnumerateFilesRecursively(@args[0]);
		XDocument report = CreateReport(files);
		report.Save(@args[1]);

	}else{
		Console.WriteLine("No arguments found");
	}
}

 