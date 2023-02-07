using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AdaptiveExtensions
{
    public static class SimplifyExtensions
    {
        #region Private Methods

        /// <summary>
        /// Divides the array into equal parts
        /// </summary>
        /// <typeparam name="T">Type of the list to be split</typeparam>
        /// <param name="sizeOfArrays">Size of the parts to be split into</param>
        /// <param name="array">Array to be split</param>
        /// <returns>A list of equal parts of the array</returns>
        static private List<List<T>> GetDividedSections<T>(int numSections, List<T> array)
        {
            List<List<T>> result = new List<List<T>>();

            // Add all sub lists in list array
            for (int i = 0; i < numSections; i++)
            {
                result.Add(new List<T>());
            }

            int split = array.Count / numSections;
            int innerList = 0;

            if (split == 0)
            {
                return result;
            }

            for (int i = 0; i < array.Count; i += split)
            {
                for (int j = i; j < array.Count && j < i + split; j++)
                {
                    if (innerList >= numSections)
                    {
                        innerList--;
                    }
                    result[innerList].Add(array[j]);
                }


                innerList++;
            }



            // // Splits into sized sections, not number of sections
            //for (int i = 0; i < array.Count; i += numSections)
            //{
            //    List<T> newSection = new List<T>();
            //
            //    for (int j = i; j < i + numSections; j++)
            //    {
            //        if (array.Count <= j)
            //        {
            //            break;
            //        }
            //        newSection.Add(array[j]);
            //    }
            //    result.Add(newSection);
            //}

            return result;
        }

        /// <summary>
        /// Gets the compliment of the section provided
        /// </summary>
        /// <typeparam name="T">Type of the list to get the compliment of</typeparam>
        /// <param name="array">Array to get compliment from</param>
        /// <param name="sectionIndex">Index of the section to get the compliment of</param>
        /// <returns>Compliment of the section index provided</returns>
        static private List<T> GetSectionCompliment<T>(List<List<T>> array, int sectionIndex)
        {
            List<T> compliment = new List<T>();

            foreach (List<T> section in array)
            {
                if (array.IndexOf(section) == sectionIndex)
                {
                    continue;
                }

                compliment.AddRange(section);
            }

            return compliment;
        }

        #endregion

        #region Public Methods

        #region File Editing

        /// <summary>
        /// Gets the method for the selected unit test to simplify
        /// </summary>
        /// <param name="testFilePath">File the test is in</param>
        /// <param name="testName">Name of the test method</param>
        /// <returns>Method declaration syntax for the method</returns>
        static public MethodDeclarationSyntax GetTestMethod(string testFilePath, string testName)
        {
            string text = File.ReadAllText(testFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);

            CompilationUnitSyntax input = tree.GetCompilationUnitRoot();
            var nameSpaceOriginal = ((NamespaceDeclarationSyntax)input.Members[0]);
            var classOriginal = (ClassDeclarationSyntax)nameSpaceOriginal.Members[0];

            var classMembers = classOriginal.DescendantNodes().OfType<MemberDeclarationSyntax>();

            foreach (var member in classMembers)
            {
                //var property = member as PropertyDeclarationSyntax;
                //if (property != null)
                //    Console.WriteLine("Property: " + property.Identifier);
                var method = member as MethodDeclarationSyntax;
                if (method != null)
                {
                    if (method.Identifier.ToString() == testName)
                    {
                        return method;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the statement list for the test file provided
        /// </summary>
        /// <param name="testFilePath">Test file path for the statement list</param>
        /// <returns>Statement list for the test provided</returns>
        static public SyntaxList<StatementSyntax> GetTestStatements(string testFilePath, string testName)
        {
            var method = GetTestMethod(testFilePath, testName);
            var blockX = (BlockSyntax)method.Body;

            return blockX.Statements;
        }

        static public int GetHighestLevelTestStatements(string testFilePath, string testName)
        {
            var method = GetTestMethod(testFilePath, testName);
            var blockX = (BlockSyntax)method.Body;
            return GetHighestLevelTestStatements(blockX);
            
        }


        static public int GetHighestLevelTestStatements(StatementSyntax statement)
        {
            int n = 0;
            if(statement is BlockSyntax)
            {
                SyntaxList<StatementSyntax> statementList = ((BlockSyntax)statement).Statements;
                int max = 0;
                foreach(StatementSyntax s in statementList)
                {
                    int current = GetHighestLevelTestStatements(s);
                    if (current > max)
                        max = current;

                }
                return max + 1;
            }
            if(statement is IfStatementSyntax)
            {
                StatementSyntax ifPart = ((IfStatementSyntax)statement).Statement;
                int ifPartHeight = 0;
                int elsePartHeight = 0;
                if(((IfStatementSyntax)statement).Else == null)
                {

                }
                else
                {
                    StatementSyntax elsePart = ((IfStatementSyntax)statement).Else.Statement;
                    elsePartHeight = GetHighestLevelTestStatements(elsePart);
                }
                
                ifPartHeight = GetHighestLevelTestStatements(ifPart);
                
                return Math.Max(ifPartHeight, elsePartHeight);
            }
            return 0;
        }

        static public SyntaxList<StatementSyntax> GetNthLevelTestStatements(string testFilePath, string testName, int n)
        {
            var method = GetTestMethod(testFilePath, testName);
            var blockX = (BlockSyntax)method.Body;
            var statements = GetNthLevelTestStatements(blockX, n);


            return statements;
        }

        static public SyntaxList<StatementSyntax> GetNthLevelTestStatements(StatementSyntax rootLevelStmt, int n)
        {
            SyntaxList<StatementSyntax> statementList = new SyntaxList<StatementSyntax>();
            if(n == 0)
            {
                
                statementList =  statementList.Add(rootLevelStmt);
                return statementList;
            }
            if(rootLevelStmt is BlockSyntax)
            {
                if (n == 0)
                    return ((BlockSyntax)rootLevelStmt).Statements;
                else
                {
                    foreach(StatementSyntax s in ((BlockSyntax)rootLevelStmt).Statements)
                    {
                         if(s is IfStatementSyntax)
                            statementList = statementList.AddRange(GetNthLevelTestStatements(s, n-1));
                        else
                        {
                            if(n == 1)
                                statementList = statementList.Add(s);
                        }
                    }
                }
            }
            if(rootLevelStmt is IfStatementSyntax)
            {
                var ifpart = ((IfStatementSyntax)rootLevelStmt).Statement;
                statementList =  statementList.AddRange(GetNthLevelTestStatements(ifpart, n ));
                if (((IfStatementSyntax)rootLevelStmt).Else != null)
                {
                    var elsePart = ((IfStatementSyntax)rootLevelStmt).Else.Statement;


                     statementList =  statementList.AddRange(GetNthLevelTestStatements(elsePart, n - 1));
                }
                    

            }
            if(rootLevelStmt is ExpressionStatementSyntax)
            {

            }

            return statementList;
        }


        static public string GetTestCallString(string testFilePath, string testName)
        {
            string text = File.ReadAllText(testFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);

            CompilationUnitSyntax input = tree.GetCompilationUnitRoot();
            var nameSpaceOriginal = ((NamespaceDeclarationSyntax)input.Members[0]);
            var classOriginal = (ClassDeclarationSyntax)nameSpaceOriginal.Members[0];

            return nameSpaceOriginal.Name + "." + classOriginal.Identifier + "." + testName;
        }

        /// <summary>
        /// Gets the statement list for the test file provided
        /// </summary>
        /// <param name="testFilePath">Test file path for the statement list</param>
        /// <returns>Statement list for the test provided</returns>
        static public bool SetTestStatements(string testFilePath, string outputFilePath, string testName, List<StatementSyntax> statementsToReplace)
        {
            if (!File.Exists(outputFilePath))
            {
                try
                {
                    FileStream file = File.Create(outputFilePath);
                    file.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File was not created at " + outputFilePath + ". " + ex.Message);
                }
            }

            string text = File.ReadAllText(testFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);

            CompilationUnitSyntax input = tree.GetCompilationUnitRoot();
            var nameSpaceOriginal = ((NamespaceDeclarationSyntax)input.Members[0]);
            var classOriginal = (ClassDeclarationSyntax)nameSpaceOriginal.Members[0];

            MethodDeclarationSyntax methodSyntax = null;
            var classMembers = classOriginal.DescendantNodes().OfType<MemberDeclarationSyntax>();

            foreach (var member in classMembers)
            {
                //var property = member as PropertyDeclarationSyntax;
                //if (property != null)
                //    Console.WriteLine("Property: " + property.Identifier);
                var method = member as MethodDeclarationSyntax;
                if (method != null)
                {
                    if (method.Identifier.ToString() == testName)
                    {
                        methodSyntax = method;
                        break;
                    }
                }
            }

            if (methodSyntax == null)
            {
                return false;
            }

            var blockX = (BlockSyntax)methodSyntax.Body;

            var statements = blockX.RemoveNodes(blockX.Statements, SyntaxRemoveOptions.KeepNoTrivia);

            var x = statements.AddStatements(statementsToReplace.ToArray());

            MethodDeclarationSyntax tempMethod = methodSyntax.WithBody(x);
            var newClass = classOriginal.ReplaceNode(methodSyntax, tempMethod);
            var output = input.ReplaceNode(classOriginal, newClass);
            System.Diagnostics.Debug.WriteLine(tempMethod.ToString());

            System.Console.WriteLine("\n\n--------------------------------------\n" + tempMethod.ToString());

            WaitForFile(outputFilePath);
            Console.Write(output.ToString());
            File.WriteAllText(outputFilePath, output.ToString());

            return true;
        }

        /// <summary>
        /// Writes all lines back to a file
        /// </summary>
        /// <param name="path">Path to write to</param>
        /// <param name="statements">Statements to write to file</param>
        /// <returns>True if successful, false if unsuccessful</returns>
        static public bool WriteToFile(string path, List<StatementSyntax> statements)
        {
            try
            {
                // Write out statements to file
                File.WriteAllText(path, "");
                foreach (StatementSyntax line in statements)
                {
                    File.AppendAllText(path, line.ToString() + '\n');
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }   
        
        /// <summary>
        /// Blocks until the file is not locked any more.
        /// </summary>
        /// <param name="fullPath"></param>
        public static bool WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        // If we got this far the file is ready
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                    if (numTries > 100)
                    {
                        return false;
                    }

                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(500);
                }
            }

            return true;
        }

        /// <summary>
        /// Runs a cmd command from another process
        /// </summary>
        /// <param name="fileName">Command to run</param>
        /// <param name="arguments">Arguments to run with the command</param>
        /// <returns>True if successful; False if unsucessful</returns>
        static public bool ExecuteCommand(string fileName, string arguments, int timeout = 5000)
        {
            try
            {
                ProcessStartInfo processInfo;
                Process process;

                processInfo = new ProcessStartInfo(fileName, arguments);
                //processInfo.CreateNoWindow = false;
                //processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;

                process.Start();

                process.WaitForExit(timeout);
                string output = process.StandardOutput.ReadToEnd();
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Finds the smallest input for the test and input provided for the test to continue to fail
        /// </summary>
        /// <typeparam name="T">Type of the list in the input</typeparam>
        /// <param name="array">Input array for the failing test</param>
        /// <param name="compareTestInput">Function to compare the test input against</param>
        /// <returns>A list of the smallest failing input for the test to continue to fail</returns>
        static public List<T> FindSmallestFailingInput<T>(List<T> array, Func<List<T>, bool> compareTestInput)
        {
            // Amount of items to split the groups into
            int numSections = 2;

            while (true)
            {
                bool isSuccessful = true;

                // Divides the array into equal sized sections
                List<List<T>> sectionedArray = GetDividedSections(numSections, array);

                // Test the sections for failing input
                foreach (List<T> arrSection in sectionedArray)
                {
                    isSuccessful = compareTestInput(arrSection);

                    if (!isSuccessful && arrSection.Any())
                    {
                        // Section off failing input and try again
                        array = arrSection;
                        numSections = 2;

                        break;
                    }

                }

                if (!isSuccessful)
                {
                    continue;
                }

                // Test the compliments of the sections for failing input
                foreach (List<T> arrSection in sectionedArray)
                {
                    List<T> compliment = GetSectionCompliment(sectionedArray, sectionedArray.IndexOf(arrSection));
                    isSuccessful = compareTestInput(compliment);

                    if (!isSuccessful && compliment.Any())
                    {
                        // Section off failing input and try again
                        array = compliment;

                        // n = max(n-1, 2)
                        numSections = Math.Max(numSections - 1, 2);

                        break;
                    }

                }

                if (!isSuccessful)
                {
                    continue;
                }

                // If all previous inputs pass, increase granularity, create more equal parts
                // array = array;
                numSections = 2 * numSections; // Math.Min(2 * numSections, array.Count);

                if (numSections > array.Count)
                {
                    return array;
                    //break;
                }

                continue;
            }

            return array;
        }

        #endregion
    }
}
