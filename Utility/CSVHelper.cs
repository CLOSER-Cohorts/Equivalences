using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Repository.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ColecticaSdkMvc.Models;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;
using System.IO;

namespace ColecticaSdkMvc.Utility
{
    public class CSVHelper
    {
        public static void ProcessPage1(List<EquivalenceItem> results)
        {
            string line;

            var target = Path.Combine(Directory.GetCurrentDirectory(), "out", "page1.csv");

            var filename = @"C:\Users\qtnvwhn\Downloads\" + "page1.csv";
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(filename))
            {
                line = "Search Term, Question Name, Question Text, Variable Name, Variable Text, Agency, Study, Concept";
                file.WriteLine(line);
                foreach (var item in results)
                {
                    line = item.equivalence + "," + item.questionName + "," + item.questionText.Trim().Replace(",", " ") + "," +
                        item.variableName + "," + item.variableText.Trim().Replace(",", " ") + "," +
                        "," + item.study.Trim() + "," + item.studyGroup + "," + item.concept;
                    file.WriteLine(line);
                }
            }
        }

        public static void ProcessPage2(List<EquivalenceItem> results)
        {
            string line;

            var target = Path.Combine(Directory.GetCurrentDirectory(), "out", "page2.csv");

            var filename = @"C:\Users\qtnvwhn\Downloads\" + "page2.csv";
            //filename = filename.Replace("\\", "/");
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(filename))
            {
                int counter1 = 1; int uniqueid = 0;
                line = "Search Term, Name, Description, Variable Name, Variable Text, Variable Item, Question Name, Question Text, Question Item";
                file.WriteLine(line);
                foreach (var item in results)
                {
                    if (item.uniqueId != uniqueid) { counter1 = 1; }
                    if (counter1 == 1)
                    {
                        line = item.equivalence + "," + item.name + "," + item.description + "," + item.variableName + "," + item.variableText.Trim().Replace(",", " ") + "," + item.variableIdentifier + "," +
                            item.questionName + "," + item.questionText.Trim().Replace(",", " ") + "," + "," + item.questionIdentifier;
                    }
                    else
                    {
                        line = "," +  "," +  "," + item.variableName + "," + item.variableText.Trim().Replace(",", " ") + "," + item.variableIdentifier + "," +
                            item.questionName + "," + item.questionText.Trim().Replace(",", " ") + "," + "," + item.questionIdentifier;
                    }
                    file.WriteLine(line);
                    uniqueid = item.uniqueId;
                    counter1++;
                }
            }
        }

        public static void ProcessPage3(List<ExpectedItem> results)
        {
            string line;
            int counter = 1;
            var target = Path.Combine(Directory.GetCurrentDirectory(), "out", "page3.csv");

            var filename = @"C:\Users\qtnvwhn\Downloads\" + "page3.csv";
            //filename = filename.Replace("\\", "/");
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(filename))
            {
                int counter1 = 1; int uniqueid = 0;
                foreach (var item in results)
                {
                    if (counter == 1)
                    {
                        uniqueid = item.UniqueId;
                        line = "Search Term, Description, Topic";
                        foreach (var wave in item.Waves)
                        {
                            line = line + ", " + wave.StudyName;
                        }
                        file.WriteLine(line);
                    }
                    if (item.UniqueId != uniqueid) { counter1 = 1; }
                    if (counter1 == 1)
                    {
                        line = item.Name + "," + item.Description.Replace(",", ".") + "," + item.Topic;
                    }
                    else
                    {
                        line = "," + "," + item.Topic;
                    }
                    foreach (var wave in item.Waves)
                    {
                        line = line + ", " + wave.Value;
                    }
                    file.WriteLine(line);
                    uniqueid = item.UniqueId;
                    counter++;
                    counter1++;
                }
            }


        }

        public static void ProcessPage4(List<ExpectedItem> results)
        {
            string line;
            int counter = 1;
            var target = Path.Combine(Directory.GetCurrentDirectory(), "out", "page4.csv");

            var filename = @"C:\Users\qtnvwhn\Downloads\" + "page4.csv";
            //filename = filename.Replace("\\", "/");
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(filename))
            {
                int counter1 = 1; int uniqueid = 0;
                foreach (var item in results)
                {
                    if (counter == 1)
                    {
                        uniqueid = item.UniqueId;
                        line = "Search Term, Description, Topic";
                        foreach (var wave in item.Waves)
                        {
                            line = line + ", " + wave.StudyName;
                        }
                        file.WriteLine(line);
                    }
                    if (item.UniqueId != uniqueid) { counter1 = 1; }
                    if (counter1 == 1)
                    {
                        line = item.Name + "," + item.Description.Replace(",", ".") + "," + item.Topic;
                    }
                    else
                    {
                        line = "," + "," + item.Topic;
                    }
                    foreach (var wave in item.Waves)
                    {
                        line = line + ", " + wave.Value;
                    }

                    file.WriteLine(line);
                    uniqueid = item.UniqueId;
                    counter++;
                    counter1++;
                }
            }
        }
    }
}