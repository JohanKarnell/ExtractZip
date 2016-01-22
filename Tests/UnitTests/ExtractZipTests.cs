using System;
using System.Globalization;
using BizTalkComponents.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;
using System.IO.Compression;
using System.IO;

namespace BizTalkComponents.PipelineComponents.ExtractZip.Tests.UnitTests
{
    [TestClass]
    public class ExtractZipTests
    {
        [TestMethod]
        public void TestExtractNoOfFiles()
        {
            int expectedNoOfFilesInZip = 3;
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new ExtractZip();
            string zipPath = @"TestData\test.zip";
            pipeline.AddComponent(component, PipelineStage.Disassemble);

            using (FileStream fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var message = MessageHelper.CreateFromStream(sr.BaseStream);
                    var outout = pipeline.Execute(message);
                    Console.WriteLine(outout.Count.ToString());

                    Assert.IsTrue(outout.Count == expectedNoOfFilesInZip);
                }
            }
        }
    }
}
