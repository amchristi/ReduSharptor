using Adaptive.QuickSortTest;
using AdaptiveExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ReduceFailingInput
{
    class Program
    {
        /// <summary>
        /// File path for the test example being simplified
        /// </summary>
        private static string testExample { get; set; }

        /// <summary>
        /// Method name for test in file
        /// </summary>
        private static string testName { get; set; }

        /// <summary>
        /// Test solution to be compiled
        /// </summary>
        private static string testProj { get; set; }

        /// <summary>
        /// Output file path for the samples to be written to. Will create additional folders inside
        /// </summary>
        private static string outputFilePath { get; set; }

        /// <summary>
        /// Tests the basic example with int inputs with the QuickSort example
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static public bool IsTestSuccessful(List<int> array)
        {
            try
            {
                QuickSortTest.NoDuplicateEntries(array.ToArray());
            }
            catch
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Build and run the test. Return the result
        /// </summary>
        /// <param name="testStatements">Test statements to test if successful</param>
        /// <returns>True if the test is successful. False if unsuccessful</returns>
        static public bool BuildAndRunTest(List<StatementSyntax> testStatements)
        {
            // Write out statements to file
            SimplifyExtensions.SetTestStatements(testExample, testExample, testName, testStatements);

            Console.WriteLine("Building current version of test.");

            // Run the build command
            if (!SimplifyExtensions.ExecuteCommand("dotnet", "build \"" + testProj + "\""))
            {
                Console.WriteLine("Build failed. Continue searching for failing test.");

                // We don't want to record build failures, so we return true to not remember them in the algorithm
                return true;
            }

            Console.WriteLine("Running test for failure...");

            bool isSuccessful = SimplifyExtensions.ExecuteCommand("dotnet", "test \"" + testProj + "\" --filter \"FullyQualifiedName=" + SimplifyExtensions.GetTestCallString(testExample, testName) + "\"");

            if (isSuccessful)
            {
                Console.WriteLine("Test was successful. Continue looking for failing test.");
            }
            else
            {
                Console.WriteLine("Test was unsuccessful. Shrink test statements.");
            }

            // Run the test
            return isSuccessful;
        }

        /// <summary>
        /// Shows a few examples about using the Adaptive Extention methods
        /// </summary>
        /// <param name="args">Arguments to control what to simplify; (Path to test, name of test, path to testProj, output path)</param>
        static void Main(string[] args)
        {
            // ------------------------------------------- Run with int list ------------------------------------------- //

            //// Hard coded failing test input
            //List<int> array = new List<int>() { 20, 72, 12, 6, 81, 97, 37, 59, 52, 1, 20 };

            //// Get the function to run the test against
            //Func<List<int>, bool> compareTestInput = IsTestSuccessful;

            //// Loop until found smallest input
            //List<int> output = SimplifyExtensions.FindSmallestFailingInput<int>(array, compareTestInput);

            //Console.WriteLine(output);


            // ------------------------------------------- Run with test statement list ------------------------------------------- //

            Console.WriteLine("\n\nReduce Failing Input.");
            bool isDD = true;
            // Console.ReadLine();

            // If user doesn't pass args, then use default; if they more then needed, return
            if (args.Length < 4)
            {

                /* Daivd hardcoded stuff
                Console.WriteLine("No arguments passed, using default unit test.\n");
                testExample = Path.GetFullPath("C:\\Users\\einnu\\Documents\\Notes\\Thesis\\Experimentation projects\\_1. Projects for experiments\\language-ext\\LanguageExt.Tests\\ApplicativeTests.cs");
                testName = "ListCombineTest";
                testProj = Path.GetFullPath("C:\\Users\\einnu\\Documents\\Notes\\Thesis\\Experimentation projects\\_1. Projects for experiments\\language-ext\\LanguageExt.Tests\\LanguageExt.Tests.csproj");
                outputFilePath = Path.GetFullPath("C:\\Users\\einnu\\Documents\\Notes\\Thesis\\Adaptive Programming\\Simplified Test Results");*/
                /*
                 * Arpit hardcoded stuff for testing. 
                 */

                Console.WriteLine("No arguments passed, using default unit test.\n");
                testExample = Path.GetFullPath("C:/arpit/personal/source/ArpitTest/ArpitTestTest/UnitTest1.cs");
                testName = "Test1";
                testProj = Path.GetFullPath("C:/arpit/personal/source/ArpitTest/ArpitTestTest/ArpitTestTest.csproj");
                outputFilePath = Path.GetFullPath("C:/arpit/personal/source/Simplified/"); 


            }
            else if (args.Length > 4)
            {
                Console.WriteLine("Incorrect arguments\n");
                return;
            } 
            else
            {
                Console.WriteLine("Using command line arguments");
                testExample = Path.GetFullPath(args[0]);
                testName = args[1];
                testProj = Path.GetFullPath(args[2]);
                outputFilePath = Path.GetFullPath(args[3]);
            }

            
            // Validate that user file import already exist
            if (!File.Exists(testExample))
            {
                Console.Write("Test file doesn't exist\n");
                return;
            }
            if (!File.Exists(testProj))
            {
                Console.Write("Sln file doesn't exist\n");
                return;
            }

            if (!isDD)
            {
                //Use this for hdd implementation. 
                int heigestLevelOfTree = SimplifyExtensions.GetHighestLevelTestStatements(testExample, testName);
                for(int  i = heigestLevelOfTree; i >= 0; i--)
                {
                    SyntaxList<StatementSyntax> nThLevelStatements = SimplifyExtensions.GetNthLevelTestStatements(testExample, testName, i);
                    System.Diagnostics.Debug.WriteLine(nThLevelStatements);

                }


            }
            // Get test statements in a list
            SyntaxList<StatementSyntax> testStatementsRaw = SimplifyExtensions.GetTestStatements(testExample, testName);

            List<StatementSyntax> testStatements = new List<StatementSyntax>(testStatementsRaw);

            // Create the function to edit file, build, and run test
            Func<List<StatementSyntax>, bool> buildAndCompareTest = BuildAndRunTest;

            // Copy original file to keep a record
            SimplifyExtensions.SetTestStatements(testExample, Path.Combine(outputFilePath, "Original", testName + "_" + Path.GetFileName(testExample)), testName, testStatements);
            Console.WriteLine("Here is the starting file.");

            List<StatementSyntax> simplifiedStatements = new List<StatementSyntax>();

            try
            {
                // Run algorithm with parameters
                simplifiedStatements = SimplifyExtensions.FindSmallestFailingInput<StatementSyntax>(testStatements, buildAndCompareTest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Revert the original test file back to the original form
                SimplifyExtensions.SetTestStatements(testExample, testExample, testName, testStatements);
                Console.WriteLine("Reverting the original file.\nHere is the original file");
            }


            // Output test results
            SimplifyExtensions.SetTestStatements(testExample, Path.Combine(outputFilePath, "Simplified", testName + "_" + Path.GetFileName(testExample)), testName, simplifiedStatements);
            Console.WriteLine("Here are the simpified results.");


            //Console.ReadLine();
        }
    }
}


/* Examples he mentioned
 * https://mir.cs.illinois.edu/gyori/pubs/icse13tool-LambdaFicator.pdf
 * https://dl.acm.org/doi/abs/10.1145/2593902.2593925
 * https://pypi.org/project/picireny/
 * 
 * More examples
 * https://en.wikipedia.org/wiki/Delta_debugging
 */