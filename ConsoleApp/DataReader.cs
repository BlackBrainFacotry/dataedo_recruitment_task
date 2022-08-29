namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class DataReader
    {
        
        int goodInterpreterRecords = 0;
        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            // imports lines form file as strings
            var importedLines = importedLinesFromFileToImport(fileToImport);

            // parse lines to objects
            IEnumerable<ImportedObject>  ImportedObjects = parseImportedLinesToObjectList(importedLines);

            // clear and correct imported data
            ImportedObjects = clearAndCorrectImportedObjects(ImportedObjects);

            // assign number of children
            ImportedObjects = assingNumberOfChildren(ImportedObjects);

            // print data
            printAllDatabaseWithTablesAndColumns(ImportedObjects);

            Console.WriteLine(goodInterpreterRecords.ToString());
            Console.ReadLine();
        }

        private List<string> importedLinesFromFileToImport(string fileToImport)
        {
            var streamReader = new StreamReader(fileToImport);

            var importedLines = new List<string>();

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }

            return importedLines;
        }

        private List<ImportedObject> parseImportedLinesToObjectList(List<string> importedLines)
        {
            List<ImportedObject> ImportedObjects = new List<ImportedObject>() { new ImportedObject() };

            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = (importedLine+";").Split(';');
                var importedObject = new ImportedObject();
                try 
                { 
                    importedObject.Type = values[0];
                    importedObject.Name = values[1];
                    importedObject.Schema = values[2];
                    importedObject.ParentName = values[3];
                    importedObject.ParentType = values[4];
                    importedObject.DataType = values[5];
                    importedObject.IsNullable = values[6];
                    ((List<ImportedObject>)ImportedObjects).Add(importedObject);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);   // catch 6 empty lines
                }
            }

            return ImportedObjects;
        }

        private IEnumerable<ImportedObject> clearAndCorrectImportedObjects(IEnumerable<ImportedObject> importedObjects)
        {
            importedObjects = importedObjects.Where(e => e.Type != null && e.Name != null && e.Schema != null && e.ParentName != null && e.ParentType != null).ToList(); // check before do changes on nullable obj
            foreach (var importedObject in importedObjects)
            {
                importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper(); //if we use ToUpper here, we must remember about this magnification during each comparison
                importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
            }
            return importedObjects;
        }

        private IEnumerable<ImportedObject> assingNumberOfChildren(IEnumerable<ImportedObject> importedObjects)
        {
            for (int i = 0; i < importedObjects.Count(); i++)
            {
                var importedObject = importedObjects.ToArray()[i];
                foreach (var impObj in importedObjects)
                {
                    
                    if (impObj.ParentName.ToUpper() == importedObject.Name.ToUpper() && impObj.ParentType.ToUpper() == importedObject.Type.ToUpper())
                    {
                        importedObject.NumberOfChildren = 1 + importedObject.NumberOfChildren;
                    }
                    
                }
            }
            return importedObjects;
        }

        private void printAllTablesColumns(IEnumerable<ImportedObject> importedObjects, ImportedObject table)
        {
            foreach (var column in importedObjects)
                {
                    if (column.ParentType.ToUpper() == table.Type && column.ParentName.ToUpper() == table.Name.ToUpper())
                    {
                                    
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                   goodInterpreterRecords++;
                    }
                }
        }

        private void printAllDatabasesTable(IEnumerable<ImportedObject> importedObjects, ImportedObject database)
        {
             foreach (var table in importedObjects)
                    {
                        if (table.ParentType.ToUpper() == database.Type && table.ParentName.ToUpper() == database.Name.ToUpper())
                        {
                            
                            Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");
                            goodInterpreterRecords++;
                            // print all table's columns
                            printAllTablesColumns(importedObjects, table);
                            
                        }
                    }
        }

        private void printAllDatabaseWithTablesAndColumns(IEnumerable<ImportedObject> importedObjects)
        {
            foreach (var database in importedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");
                    goodInterpreterRecords++;
                    // print all database's tables
                    printAllDatabasesTable(importedObjects, database);
                }
            }
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Schema;
        public string ParentName;
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public double NumberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
