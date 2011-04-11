﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Reflection;
using System.IO;
#if USE35
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif

using Dlrsoft.VBScript.Hosting;
namespace Dlrsoft.VBScriptTest
{
    public class VBScriptTestHelper
    {
        static public void Run(string filePath)
        {
            // Setup DLR ScriptRuntime with our languages.  We hardcode them here
            // but a .NET app looking for general language scripting would use
            // an app.config file and ScriptRuntime.CreateFromConfiguration.
            var setup = new ScriptRuntimeSetup();
            string qualifiedname = typeof(VBScriptContext).AssemblyQualifiedName;
            setup.LanguageSetups.Add(new LanguageSetup(
                qualifiedname, "vbscript", new[] { "vbscript" }, new[] { ".vbs" }));
            var dlrRuntime = new ScriptRuntime(setup);

            //Add the VBScript runtime assembly
            dlrRuntime.LoadAssembly(typeof(global::Dlrsoft.VBScript.Runtime.BuiltInFunctions).Assembly);

            // Get a VBScript engine and run stuff ...
            var engine = dlrRuntime.GetEngine("vbscript");
            var scriptSource = engine.CreateScriptSourceFromFile(filePath);
            var compiledCode = scriptSource.Compile();
            var feo = engine.CreateScope();
            //feo = engine.ExecuteFile(filename, feo);
            feo.SetVariable("Assert", new NunitAssert());
            compiledCode.Execute(feo);
        }

        static public string AssemblyDirectory 
        { 
            get 
            { 
                string codeBase = Assembly.GetExecutingAssembly().CodeBase; 
                UriBuilder uri = new UriBuilder(codeBase); 
                string path = Uri.UnescapeDataString(uri.Path); 
                return Path.GetDirectoryName(path); 
            } 
        }

        static public void Walk(string path, Action<string> fileHandler)
        {
            FileAttributes fa = File.GetAttributes(path);
            if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Walk(new DirectoryInfo(path), fileHandler);
            }
            else
            {
                Walk(new FileInfo(path), fileHandler);
            }
        }

        static private void Walk(DirectoryInfo di, Action<string> fileHandler)
        {
            foreach (FileInfo fi in di.GetFiles())
            {
                Walk(fi, fileHandler);
            }

            foreach (DirectoryInfo cdi in di.GetDirectories())
            {
                Walk(cdi, fileHandler);
            }
        }

        static private void Walk(FileInfo fi, Action<string> fileHandler)
        {
            fileHandler(fi.FullName);
        }

    }
}
