using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ColecticaSdkMvc.Utility;
using ColecticaSdkMvc.Models;
using System.Web.Script.Serialization;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text;

namespace ColecticaSdkMvc.Controllers
{
    class GFG : IComparer<string>
    {
        public int Compare(string x, string y)
        {

            if (x == null || y == null)
            {
                return 0;
            }

            // "CompareTo()" method 
            return x.CompareTo(y);

        }
    }
  
    public class EquivalenceController : Controller
    {
        public ActionResult Index()
        {

            // This is the Controller action for the main screen on Equivalences where the user selects the options to be used in selecting Equivalences for analysis

            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            if (mymodel != null) mymodel.SelectedStudies = new List<string>();
            EquivalenceModel model = new EquivalenceModel();
            model = InitialiseModel(model, mymodel, "");
            if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
            return View(model);
        }

        public EquivalenceModel InitialiseModel(EquivalenceModel model, EquivalenceModel live,string selectedStudies)
        {
            model.Methods = GetAllMethods();
            model.SelectedMethods = new List<string>();
            model.WordList = new List<Word>();

            if (live != null) { model.AllItems = live.AllItems; } else { model.AllItems = new List<EquivalenceItem>(); }
            if (live != null) { model.MasterItems = live.MasterItems; } else { model.MasterItems = new List<EquivalenceItem>(); }
            if (live != null) { model.Items = live.Items; } else { model.Items = new List<EquivalenceItem>(); }
            if (selectedStudies != "")            
            {
                model.SelectedStudies = DeserializeStudies(model, selectedStudies);
                ViewBag.selectedItems = selectedStudies;
            }
            else { model.SelectedStudies = new List<string>(); }
            model.selectStudies = selectedStudies;
            if (model.Datasets == null) model.Datasets = new List<Study>();
            model.Name = false;
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model = EquivalenceHelper.BuildStudiesTree(model, nodes);
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);

            return model;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(EquivalenceModel model, string Study, string selectedItems, string originalItems, string fileName, string datasets, string command, HttpPostedFileBase postedFile, HttpPostedFileBase postedFile1)
       {
            // This is the Controller action for the main screen on Equivalences where the user selects the options to be used in selecting Equivalences for analysis

            DateTime start, finish;

            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            if (mymodel != null) {items = mymodel.Items; }
            string wordselection = "";
            model.Methods = GetAllMethods();
            List<TreeViewNode> nodes = new List<TreeViewNode>();
           
            if (postedFile != null)
            {
                model = GetEquivalences(model, postedFile);
                model.Methods = GetAllMethods();
                if (mymodel == null)
                {
                    model.SelectedMethods = new List<string>();
                }
                else
                {
                    if (mymodel.SelectedMethods == null)
                    {
                        model.SelectedMethods = new List<string>();
                    }
                    else
                    {
                        model.SelectedMethods = mymodel.SelectedMethods;
                    }
                }
                if (mymodel == null) { model.SelectedStudies = new List<string>(); } else { model.SelectedStudies = mymodel.SelectedStudies; }
                if (mymodel == null) { model.AllItems = new List<EquivalenceItem>(); } else { model.AllItems = mymodel.AllItems; }
                if (mymodel == null) { model.MasterItems = new List<EquivalenceItem>(); } else { model.MasterItems = mymodel.MasterItems; }
                if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
                if (model.Datasets == null) model.Datasets = new List<Study>();
                model = EquivalenceHelper.BuildStudiesTree(model, nodes);
                ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                model.SelectedStudies = DeserializeStudies(model, selectedItems);
                ViewBag.selectedItems = selectedItems;
                ViewBag.originalItems = originalItems;
                ViewBag.Datasets = datasets;
                TempData["EquivalenceModel"] = model;
                return View(model);
            }
            model.Results = new List<StudyItem>();
            model = EquivalenceHelper.BuildStudiesTree(model, nodes);
            model.SelectedStudies = new List<string>();
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
            ViewBag.selectedItems = selectedItems;
            List<TreeViewNode> selectedstudies = new List<TreeViewNode>();
            if (selectedItems != "")
            {
                selectedstudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems);
            }

            EquivalenceModel newmodel = new EquivalenceModel();
            switch (command)
            {
                case "Save":
                    newmodel = SaveItem(newmodel, model.Word, model.WordSelection);
                    var wordlist = newmodel.WordList;
                    var selectedwords = newmodel.WordSelection;
                    newmodel.Results = new List<StudyItem>();
                    newmodel.SelectedStudies = new List<string>();
                    newmodel.Word = null;
                    newmodel.WordList = wordlist;
                    newmodel.WordSelection = selectedwords;
                    newmodel.Methods = GetAllMethods();
                    newmodel.SelectedMethods = new List<string>();
                    newmodel.AllItems = new List<EquivalenceItem>();
                    newmodel.MasterItems = new List<EquivalenceItem>();
                    if (mymodel == null) newmodel.SelectedMethods = new List<string>();
                    ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                    return View(newmodel);
                case "Equivalent Questions":
                    model.EquivalenceError = GetErrors(model, selectedItems);
                    if (model.EquivalenceError != null)
                    {
                        if (model.WordList == null) model.WordList = new List<Word>();
                        if (model.SelectedMethods == null) model.SelectedMethods = new List<string>();
                        if (model.AllItems == null) model.AllItems = new List<EquivalenceItem>();
                        if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
                        if (model.MasterItems == null) model.MasterItems = new List<EquivalenceItem>();
                        ViewBag.selectedItems = selectedItems;
                        ViewBag.originalItems = originalItems;
                        model = LoadSelectedStudies(model, selectedstudies);
                        model = EquivalenceHelper.BuildStudiesTree(model, nodes);
                        ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                        return View(model);
                    }
                    else
                    {
                        
                        start = DateTime.Now;
                        newmodel = ProcessEquivalences(model, selectedItems, originalItems, selectedstudies, datasets, "Question", fileName);
                        newmodel.AllResults = DetectMultipleEquivalences(newmodel.AllResults);
                        finish = DateTime.Now;
                        var elapsedhours = (finish - start).Hours;
                        var elapsedminutes = (finish - start).Minutes;
                        var elapseseconds = (finish - start).Seconds;
                        newmodel.Elapsed = elapsedhours.ToString() + ":" + elapsedminutes.ToString() + ":" + elapseseconds.ToString();
                        ViewBag.selectedItems = selectedItems;
                        ViewBag.originalItems = originalItems;
                        TempData["EquivalenceModel"] = newmodel;
                        if (newmodel.AllResults.Where(a => a.concepts > 1).Count() == 0)
                        {
                            return View("Display", newmodel);
                        }
                        else
                        {
                            return View("Concepts", newmodel);
                        }
                    }
                case "Equivalent Variables":
                    model.EquivalenceError = GetErrors(model, selectedItems);
                    if (model.EquivalenceError != null)
                    {
                        if (model.WordList == null) model.WordList = new List<Word>();
                        if (model.SelectedMethods == null) model.SelectedMethods = new List<string>();
                        if (model.AllItems == null) model.AllItems = new List<EquivalenceItem>();
                        if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
                        if (model.MasterItems == null) model.MasterItems = new List<EquivalenceItem>();
                        ViewBag.selectedItems = selectedItems;
                        ViewBag.originalItems = originalItems;
                        model = LoadSelectedStudies(model, selectedstudies);
                        model = EquivalenceHelper.BuildStudiesTree(model, nodes);
                        ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                        return View(model);
                    }
                    else
                    {
                        start = DateTime.Now;
                        newmodel = ProcessEquivalences(model, selectedItems, originalItems, selectedstudies, datasets, "Variable", fileName);
                        newmodel.AllResults = DetectMultipleEquivalences(newmodel.AllResults);
                        ViewBag.selectedItems = selectedItems;
                        TempData["EquivalenceModel"] = newmodel;
                        finish = DateTime.Now;
                        var elapsedhours = (finish - start).Hours;
                        var elapsedminutes = (finish - start).Minutes;
                        var elapseseconds = (finish - start).Seconds;
                        newmodel.Elapsed = elapsedhours.ToString() + ":" + elapsedminutes.ToString() + ":" + elapseseconds.ToString();
                        if (newmodel.AllResults.Count == 0)
                        {
                            newmodel = InitialiseModel(model, mymodel, selectedItems);
                            newmodel.EquivalenceError = "No matching Items";
                            return View(newmodel);
                        }
                        else
                        {
                            if (newmodel.AllResults.Where(a => a.concepts > 1).Count() == 0)
                            {
                                return View("Display", newmodel);
                            }
                            else
                            {
                                return View("Concepts", newmodel);
                            }
                        }
                    }
                case "Upload":
                    newmodel =  FileReader(postedFile1);
                    model.AllItems = newmodel.AllItems;

                    model.AllResults = newmodel.AllResults.OrderBy(a => a.concept).OrderBy(a => a.studyGroup).OrderBy(a => a.uniqueId).ToList();

                    model.Datasets = newmodel.Datasets;
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;
                    ViewBag.AllResults = serializer.Serialize(newmodel.AllResults);
                    if (model.AllResults.Where(a => a.concepts > 1).Count() == 0)
                    {
                        return View("Display", model);
                    }
                    else
                    {
                        return View("Concepts", model);
                    }
                default:
                    break;
            }

            DateTime dateTime1 = DateTime.Now;
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();
            model.Results = new List<StudyItem>();

            model = EquivalenceHelper.GetStudies(model, null);
            model.SelectedStudies = new List<string>();
            if (wordselection == null) model.WordList = new List<Word>();
            else
            {
                if (wordselection.Length != 0) model.WordList = EquivalenceHelper.GetList(wordselection);
                if (wordselection.Length == 0) model.WordList = new List<Word>();
            }
            if (selectedItems == "")
            {
                return View(model);
            }

           
          
            model = EquivalenceHelper.GetStudies(model, null);

            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            stepOneModel.Duration = res.ToString();
            return View(model);
        }

        public List<string> DeserializeStudies(EquivalenceModel model, string selectedItems)
        {
            List<string> studies = new List<string>();
            List<TreeViewNode> selectedStudies = new List<TreeViewNode>();
            if (selectedItems != "") { selectedStudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }
            foreach (var selectedStudy in selectedStudies)
            {
                var input = selectedStudy.id.Replace(" ",",") + "," + selectedStudy.text;
                studies.Add(input);
            }
            return studies;
        }

        public List<Study> DeserializeDatasets(string selectedDatasets)
        {
            List<Study> studies = new List<Study>();
            if (selectedDatasets != "") { if (selectedDatasets != null) studies = (new JavaScriptSerializer()).Deserialize<List<Study>>(selectedDatasets); }        
            return studies;
        }

        public string SerializeDatasets(List<Study> selectedDatasets)
        {
            string datasets = (new JavaScriptSerializer()).Serialize(selectedDatasets);
            return datasets;
        }

        public string GetErrors(EquivalenceModel model, string selectedstudies)
        {
            string error = null;
            if (selectedstudies == "")
            {
                error = error + " Select a Study or Studies. ";
            }
            if (model.WordList == null && model.SelectedMethods == null)
            {
                error = error + " Select a method or equivalences";
            }
            else
            {
                if (model.WordList == null && model.SelectedMethods == null)
                {
                    error = error + " Select a method or equivalences";
                }
            }
            return error;
        }

        public EquivalenceModel ProcessEquivalences(EquivalenceModel model, string selectedItems, string originalItems, List<TreeViewNode> selectedstudies, string datasets, string type, string fileName)
        {
            // Processes the model used for Equivalences and if selectedItems does not match originalItems populates AllResults with data from the repository

            int i = 0, j = 0;
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model.EquivalenceError = null;
            if (model == null) { model.MasterItems = new List<EquivalenceItem>(); } 
            if (model == null) { model.AllItems = new List<EquivalenceItem>(); } 
            model = LoadSelectedStudies(model, selectedstudies);
            var result = CompareStudyLists(originalItems, selectedItems);
            if (result == false)
            {
                model.Datasets = new List<Study>();
                model.AllItems = new List<EquivalenceItem>();
                model.MasterItems = new List<EquivalenceItem>();
                model = ProcessStudies(model);
            }
            else { model.Datasets = DeserializeDatasets(datasets); }
            EquivalenceModel newmodel = new EquivalenceModel();
            newmodel.AllResults = ProcessMethods(model.AllItems, model.MasterItems, model.SelectedMethods, type, i, j);
            newmodel.AllItems = model.AllItems;
            newmodel.Datasets = model.Datasets;
            newmodel.SelectedStudies = model.SelectedStudies;
            newmodel.WordList = model.WordList;
            newmodel.Name = model.Name;
            if (model.WordList != null) { newmodel = PopulateQuestionMessages(newmodel, nodes, type, i, j); }
            newmodel.AllItems.SetValue(a => a.removed = false).ToList();
            var removed = newmodel.AllItems.Where(a => a.removed == false).ToList();

            newmodel.FileName = fileName;
            newmodel.Type = type;
            newmodel.Datasets = model.Datasets;
            model.selectStudies = selectedItems;
            TempData["EquivalenceModel"] = newmodel;
            return newmodel;

        }

        public bool CompareStudyLists(string originalItems, string selectedItems)
        {
            if (selectedItems == null) return false;
            else
            {
                // compares originalItems to selectedItems to determine if it is necessary to load study questions and variables from the repository
                var result = originalItems.Equals(selectedItems, StringComparison.Ordinal);
                return result;
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Concepts(EquivalenceModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            // This displays all the Equivalences with more for one concept within a single equivalence. 

            var test = model.AllResults.Where(a => a.concepts > 1).ToList();
            List<TreeViewNode> selectedstudies = new List<TreeViewNode>();
            if (selectedItems != null) { selectedstudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            EquivalenceModel equivalencemodel = new EquivalenceModel();
            switch (command)
            {
               
                case "Previous":
                    equivalencemodel = InitialiseModel(equivalencemodel, mymodel, selectedItems);
                    equivalencemodel.SelectedStudies = model.SelectedStudies;
                    equivalencemodel.AllItems = model.AllItems;
                    equivalencemodel.AllResults = model.AllResults;
                    TempData["EquivalenceModel"] = equivalencemodel;
                    ViewBag.selectedItems = selectedItems;
                    ViewBag.originalItems = selectedItems;
                    if (equivalencemodel.AllResults.Where(a => a.concepts > 1).Count() == 0)
                    {
                        return View("Concepts", model);
                    }
                    else
                    {
                        return View("Index", equivalencemodel);
                    }
                case "Next":
                    equivalencemodel.SelectedStudies = model.SelectedStudies;
                    equivalencemodel.AllItems = model.AllItems;
                    equivalencemodel.AllResults = model.AllResults;
                    equivalencemodel.Datasets = model.Datasets;
                    ViewBag.selectedItems = selectedItems;
                    TempData["EquivalenceModel"] = model;
                    var selected = model.AllResults.Where(a => a.selected == true).Count();
                    var unselected = model.AllResults.Where(a => a.selected == false).Count();

                    return View("Display", equivalencemodel);
                default:
                    return View(model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Display(EquivalenceModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            // This displays all the Equivalences using the options selected using the Index page. 

            List<TreeViewNode> selectedstudies = new List<TreeViewNode>();
            if (selectedItems != null) { selectedstudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            EquivalenceModel equivalencemodel = new EquivalenceModel();

            switch (command)
            {               
               
                case "Previous":
                    equivalencemodel = InitialiseModel(equivalencemodel, mymodel, selectedItems);
                    equivalencemodel.SelectedStudies = model.SelectedStudies;
                    equivalencemodel.AllItems = model.AllItems;
                    TempData["EquivalenceModel"] = equivalencemodel;
                    ViewBag.selectedItems = selectedItems;
                    ViewBag.originalItems = selectedItems;
                    return View("Index", equivalencemodel);
                case "Next":
                    equivalencemodel.SelectedStudies = model.SelectedStudies;
                    equivalencemodel.AllItems = model.AllItems;
                    equivalencemodel.AllResults = model.AllResults;
                    equivalencemodel.Datasets = model.Datasets;
                    ViewBag.selectedItems = selectedItems;
                    TempData["EquivalenceModel"] = model;
                    var selected = model.AllResults.Where(a => a.selected == true).Count();
                    var unselected = model.AllResults.Where(a => a.selected == false).Count();

                    return View("Variables", equivalencemodel);
                default:
                    return View(model);
            }
        }

        public List<EquivalenceItem> ProcessSelected(List<EquivalenceItem> results)
        {
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            foreach (var result in results)
            {
                if (result.selected == true)
                {
                    EquivalenceItem item = new EquivalenceItem();
                    item.uniqueId = result.uniqueId;
                    item.equivalence = result.equivalence;
                    item.name = result.variableName;
                    item.description = result.variableText;
                    item.counter = result.counter;
                    item.variableName = result.variableName;
                    item.variableText = result.variableText;
                    item.variableItem = result.variableItem;
                    item.variableIdentifier = result.variableIdentifier;
                    item.questionName = result.questionName;
                    item.questionText = result.questionText;
                    item.questionItem = result.questionItem;
                    item.questionIdentifier = result.questionIdentifier;
                    item.studyGroup = result.studyGroup;
                    item.study = result.study;
                   
                    item.concept = result.concept;
                    item.column = result.column;
                    item.selected = result.selected;
                    item.dataset = result.dataset;
                    items.Add(item);
                }
            }
            return items;
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Variables(EquivalenceModel model, string selectedItems, string studyName, string itemType, string command)
        {
            // This displays all the Equivalences in an amended layout and allows users to amend variable names and variable text of selected equivalences. 
            
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            switch (command)
            {               
                case "Previous":
                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;

                    List<EquivalenceItem> items = new List<EquivalenceItem>();
                    foreach (var result in model.AllResults)
                    {
                        // result.selected = true;
                        items.Add(result);

                    }
                    model.AllResults = items;
                    ModelState.Clear();
                    return View("Display", model);
               case "Next":
                    var waves = from r in model.AllResults
                                group r by r.study into r1
                                select new { Name = r1.Key };
                
                    ExpectedModel expectedmodel = new ExpectedModel();
                    expectedmodel = GetExpectedItemsWaves(model.AllResults);
                    expectedmodel.AllItems = model.AllItems;
                    expectedmodel.AllResults = ProcessChanges(model.AllResults);
                    expectedmodel.SelectedStudies = DeserializeStudies(model, selectedItems);
                    expectedmodel.Datasets = model.Datasets;
                    var selected = model.AllResults.Where(a => a.selected == true).Count();
                    var unselected = model.AllResults.Where(a => a.selected == false).Count();

                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    return View("Waves", expectedmodel);              
                default:
                    return View(model);
            }
        }

        public List<EquivalenceItem> ProcessChanges(List<EquivalenceItem> results)
        {
            string currentquestion = "";
            string currentname = "";
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            foreach (var result in results)
            {
                if (result.name != null) { currentquestion = result.name; }
                if (result.description != null) { currentname = result.description; }

                EquivalenceItem item = new EquivalenceItem();
                item.uniqueId = result.uniqueId;
                item.equivalence = result.equivalence;
                item.name = currentquestion;
                item.description = currentname;
                item.counter = result.counter;
                item.questionName = result.questionName;
                item.questionText = result.questionText;              
                item.questionItem = result.questionItem;
                item.questionIdentifier = result.questionIdentifier;
                item.variableName = currentquestion;
                item.variableText = currentname;
                item.variableAgency = result.variableAgency;
                item.variableItem = result.variableItem;
                item.variableIdentifier = result.questionIdentifier;
                item.studyGroup = result.studyGroup;
                item.study = result.study;
                item.dataset = result.dataset;
                item.concept = result.concept;
                item.column = result.column;
                item.selected = result.selected;
                items.Add(item);
            }
            return items;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Waves(ExpectedModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            // This displays all the Equivalences in an amended layout, based on the waves associated with each variable. 

            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            ExpectedModel expectedmodel = new ExpectedModel();
            EquivalenceModel equivalencemodel = new EquivalenceModel();

            switch (command)
            {
             
               case "Previous":
                    equivalencemodel.AllItems = model.AllItems;
                    equivalencemodel.AllResults = model.AllResults;
                    equivalencemodel.SelectedStudies = model.SelectedStudies;
                    equivalencemodel.Datasets = model.Datasets;
                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    return View("Variables", equivalencemodel);
                case "Next":
                    if (model.Datasets != null)
                    {
                        model.AllResults.SetValue(a => a.removed = false).ToList();
                        expectedmodel = GetExpectedItemsDataset(model.AllResults, model.SelectedStudies, selectedItems, model.Datasets);
                        expectedmodel.AllItems = model.AllItems;
                        expectedmodel.AllResults = ProcessChanges(model.AllResults);
                        expectedmodel.Datasets = model.Datasets;
                        expectedmodel.SelectedStudies = model.SelectedStudies;
                        TempData["EquivalenceModel"] = mymodel;
                        ViewBag.selectedItems = selectedItems;
                        ViewBag.originalItems = originalItems;
                        return View("Datasets", expectedmodel);
                    }
                    else
                    {

                        equivalencemodel = InitialiseModel(equivalencemodel, mymodel, selectedItems);
                        equivalencemodel.SelectedStudies = model.SelectedStudies;
                        equivalencemodel.AllItems = model.AllItems;
                        equivalencemodel.Datasets = model.Datasets;
                        equivalencemodel.SelectedMethods = new List<string>();
                        TempData["EquivalenceModel"] = equivalencemodel;
                        ViewBag.selectedItems = selectedItems;
                        ViewBag.originalItems = selectedItems;
                        ViewBag.Datasets = SerializeDatasets(model.Datasets);

                        return View("Index", equivalencemodel);
                    }
                default:
                    return View(model);
            }
        }

        public ExpectedModel GetExpectedItemsWaves(List<EquivalenceItem> results)
        {
            results = results.Where(a => a.selected == true).ToList();
            var selected = results.Where(a => a.selected == true).Count();
            ExpectedModel model = new ExpectedModel();
            model.AllResults = results.OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();
            string currentdescription = null, currentname = null;
            results.SetValue(a => a.selected = true).ToList();

            List<ExpectedItem> expecteditems = new List<ExpectedItem>();
            var equivalences = from r in results
                               where r.selected == true
                               group r by r.uniqueId into r1
                               select new { Name = r1.Key };
            equivalences = equivalences.ToList();

            foreach (var equivalence in equivalences)
            {
                var topics = (from r in results
                              where r.selected == true
                              where r.uniqueId == equivalence.Name
                              group r by r.concept into r1
                              select new { Name = r1.Key }).ToList();
                foreach (var topic in topics)
                {
                    var questions = results.Where(a => a.uniqueId == equivalence.Name).Where(a => a.concept == topic.Name.ToString()).Where(a => a.selected == true).OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();
                    while (questions.Count != 0)
                    {
                        var waves = GetAllStudies(results);

                        ExpectedItem expecteditem = new ExpectedItem();
                        foreach (var wave in waves)
                        {

                            var question = (from q in questions
                                            where q.studyGroup == wave.StudyName
                                            where q.selected == true
                                            select q).FirstOrDefault();
                            if (question != null)
                            {

                                if (question.description != null) { currentdescription = question.description; }
                                if (question.name != null) { currentname = question.name; }

                                expecteditem.UniqueId = question.uniqueId;
                                expecteditem.Equivalence = question.equivalence;
                                expecteditem.Name = currentname;
                                expecteditem.Description = currentdescription;
                                expecteditem.Topic = question.concept;
                                questions.Where(a => a.variableItem == question.variableItem).SetValue(a => a.removed = true).ToList();
                                waves.Where(a => a.StudyName == wave.StudyName).SetValue(a => a.Value = question.variableName).ToList();
                            }
                        }
                        expecteditem.Waves = waves;
                        expecteditems.Add(expecteditem);
                        questions = questions.Where(a => a.removed == false).OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();
                    }
                }
            }


            model.expecteditems = expecteditems;
            return model;
        }

        public List<Study> GetAllStudies(List<EquivalenceItem> results)
        {
            var waves = from r in results
                        group r by r.studyGroup into r1
                        select new { Name = r1.Key };

            List<Study> studies = new List<Study>();
            int i = 1;
            foreach (var wave in waves)
            {
                Study study = new Study();
                study.ID = i;
                study.StudyName = wave.Name;
                studies.Add(study);
                i++;
            }
            return studies;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Datasets(ExpectedModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            // This displays all the Equivalences in an amended layout, based on the datasets associated with each variable. 

            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            ExpectedModel expectedmodel = new ExpectedModel();
            EquivalenceModel equivalencemodel = new EquivalenceModel();

            switch (command)
            {
                case "Previous":
                    ViewBag.selectedItems = selectedItems;
                    model.AllResults.SetValue(a => a.removed = false).ToList();
                    expectedmodel = GetExpectedItemsWaves(model.AllResults);
                    expectedmodel.AllItems = model.AllItems;
                    expectedmodel.AllResults = model.AllResults;
                    expectedmodel.SelectedStudies = model.SelectedStudies;
                    expectedmodel.Datasets = model.Datasets;
                    ViewBag.selectedItems = selectedItems;
                    return View("Waves", expectedmodel);
                case "Next":  
                    equivalencemodel = InitialiseModel(equivalencemodel, mymodel, selectedItems);
                    equivalencemodel.SelectedStudies = model.SelectedStudies;
                    equivalencemodel.AllItems = model.AllItems;
                    equivalencemodel.Datasets = model.Datasets;
                    TempData["EquivalenceModel"] = equivalencemodel;
                    ViewBag.selectedItems = selectedItems;
                    ViewBag.originalItems = selectedItems;
                    ViewBag.Datasets = SerializeDatasets(model.Datasets);

                    return View("Index", equivalencemodel);
                default:
                    return View(model);
            }
        }



        public ExpectedModel GetExpectedItemsDataset(List<EquivalenceItem> results, List<string> selectedstudies, string selecteditems, List<Study> waves)
        {
            ExpectedModel model = new ExpectedModel();
            results = results.Where(a => a.selected == true).OrderBy(a => a.column).OrderBy(a => a.uniqueId).OrderBy(a => a.dataset).ToList();
            var results1 = results.Where(a => a.uniqueId == 3).ToList();
            List<ExpectedItem> expecteditems = new List<ExpectedItem>();
            var equivalences = from r in results
                               group r by r.uniqueId into r1
                               select new { Name = r1.Key };
            equivalences = equivalences.ToList();


            foreach (var equivalence in equivalences)
            {
              
                ExpectedItem expecteditem = new ExpectedItem();
                List<Study> datasets = new List<Study>();


                foreach (var wave in waves)
                {

                    var questions = results.Where(a => a.uniqueId == equivalence.Name).Where(a => a.dataset == wave.StudyName).Where(a => a.removed == false).OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();
                    if (questions.Count != 0)
                    {
                        expecteditem.UniqueId = questions.FirstOrDefault().uniqueId;
                        expecteditem.Equivalence = questions.FirstOrDefault().equivalence;
                        expecteditem.Name = questions.FirstOrDefault().name;
                        expecteditem.Description = questions.FirstOrDefault().description;
                        expecteditem.Topic = questions.FirstOrDefault().concept;
                        Study dataset = new Study();
                        foreach (var question in questions)
                        {
                            dataset.ID = equivalence.Name;
                            dataset.StudyName = question.dataset;
                            dataset.Value = dataset.Value + question.variableName + ";";
                        }
                        datasets.Add(dataset);
                    }
                    else
                    {
                        Study dataset2 = new Study();
                        dataset2.ID = equivalence.Name;
                        dataset2.StudyName = wave.StudyName;
                        datasets.Add(dataset2);
                    }


                }
                expecteditem.Waves = datasets;
                expecteditems.Add(expecteditem);
            }            

            model.expecteditems = expecteditems.OrderBy(a => a.UniqueId).ToList();
            return model;
        }

      

        public List<Study> GetAllDatasets(List<EquivalenceItem> results, List<string> selectedstudies, string selecteditems)
        {           
            MultilingualString.CurrentCulture = "en-GB";
            SearchFacet facet = new SearchFacet();
            facet.ItemTypes.Add(new Guid("a51e85bb-6259-4488-8df2-f08cb43485f8"));
            facet.ResultOrdering = SearchResultOrdering.Alphabetical;
            facet.SearchLatestVersion = true;
            var client = ClientHelper.GetClient();
            SearchResponse datasets = client.Search(facet);
            List<SearchResult> datasets1 = new List<SearchResult>();
            List<TreeViewNode> selectedStudies = new List<TreeViewNode>();
            selectedStudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selecteditems);

            List<Study> studies = new List<Study>();
            int i = 1;


            foreach (var selecteditem in selectedStudies)
            {
                datasets1 = datasets.Results.Where(x => x.AgencyId.Contains(selecteditem.parent) == true).OrderBy(x => x.DisplayLabel).ToList(); 
                int j = 1;
                foreach (var dataset in datasets1)
                {
                    var reference = Helper.GetReferences(dataset.AgencyId, dataset.Identifier).Where(x => x.DisplayLabel == selecteditem.text).ToList();
                    
                    if (reference.Count() != 0)
                    {
                        var datasetrepositoryitem = client.GetLatestRepositoryItem(dataset.Identifier, dataset.AgencyId);
                        Study study = new Study();
                        study.ID = j;
                        study.StudyName = GetCurrentDataSet(datasetrepositoryitem.Item);
                        studies.Add(study);
                        i++;
                        j++;
                    }
                }
            }
          
            studies = studies.OrderBy(x => x.StudyName).OrderBy(x => x.ID).ToList();
            return studies;
        }
        
        public static EquivalenceModel ProcessStudies(EquivalenceModel model)
        {
            // Extracts the agency and identifier from selected studies to be used to retrieve repository items for equivalences

            int i = 1;
            model.Datasets = new List<Study>();
            List<EquivalenceItem> items = new List<EquivalenceItem>();

            foreach (var study in model.SelectedStudies)
            {
                List<string> studylist = study.Split(',').ToList();
                string agency = "", id = "", group = "";
                int j = 0;
                foreach (var studyitem in studylist)
                {
                    if (j == 0) { agency = studyitem; }
                    if (j == 1) { id = studyitem; }
                    if (j == 2) { group = studyitem; }
                    j++;
                }
                var identifier = new Guid(id);
                var references = Helper.GetReferences(agency, identifier);
                model = GetAllRepositoryItems(model, agency, identifier, group, new Guid(id), i);
                i++;

            }
            model.Datasets = model.Datasets.OrderBy(x => x.StudyName).OrderBy(x => x.ID).ToList();
            return model;
        }


        public List<VariableItem> ProcessResults(RepositoryItemMetadata result, List<VariableItem> items, EquivalenceModel model, string equivalence, int uniqueId, int counter)
        {
           
            var variables = RepositoryHelper.GetReferences(result.AgencyId, result.Identifier).Where(x => x.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d"));
            foreach (var variable in variables)
            {
                VariableItem item = new VariableItem();
                item.name = null;
                item.description = variable.DisplayLabel;
                item.counter = counter;
                item.questionName = variable.ItemName.FirstOrDefault().Value;
                item.questionText = variable.Label.FirstOrDefault().Value;
                item.questionItem = variable.CompositeId.ToString();
                item.parentitem = result.Identifier.ToString();
                item.studyGroup = variable.AgencyId;
                item.identifier = variable.Identifier;

                var concept = (from a in model.AllConcepts
                               where a.ItemType == result.Identifier
                               select a).FirstOrDefault();
                var v = RepositoryHelper.GetConcept(result.AgencyId, result.Identifier);
                RepositoryItemMetadata mainconcept = new RepositoryItemMetadata();
                if (concept != null) { mainconcept = RepositoryHelper.GetConcept(concept.AgencyId, concept.Identifier); }
                var dataset = RepositoryHelper.GetConcept(variable.AgencyId, variable.Identifier);
                if (concept != null) item.concept = concept.Label.Values.FirstOrDefault() + " - " + mainconcept.Label.Values.FirstOrDefault();
                item.description = variable.Label.Values.FirstOrDefault();
                item.questionText = item.description;
                item.questionName = variable.ItemName.Values.FirstOrDefault();
                item.study = RepositoryHelper.GetStudy(result.AgencyId, result.Identifier);
                item.name = variable.DisplayLabel;

                items.Add(item);
                item.uniqueId = uniqueId;
                item.equivalence = equivalence.Trim();
                item.selected = true;
                item.isdeprecated = variable.IsDeprecated;
            }           
            return items;
        }

        public EquivalenceModel PopulateQuestionMessages(EquivalenceModel model, List<TreeViewNode> selecteditems, string type, int i, int j)
        {
            List<EquivalenceItem> equivalenceitems = new List<EquivalenceItem>();    
            if (model.AllResults == null) { model.AllResults = new List<EquivalenceItem>(); }
            List<Word> words1 = new List<Word>();

            var client = ClientHelper.GetClient();
            var waves = GetAllStudies(model.AllItems);
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            foreach (var selectedwords in model.WordList)
            {

                equivalenceitems = model.AllItems;
                List<Word>  words2 = new List<Word>();
                List<string> wordList2 = selectedwords.Value.Split(' ').ToList();
                foreach (var word2 in wordList2)
                {
                    Word currentword = new Word();
                    currentword.Value = word2;
                    words2.Add(currentword);
                }
                string selectedword = "";
                foreach (var currentword in words2)
                {
                    selectedword = currentword.Value;
                    switch (type)
                    {
                        case "Question":
                            switch (model.Name)
                            {
                                case false:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.questionText.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                                case true:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.questionName.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                            }
                            break;
                        case "Variable":
                            switch (model.Name)
                            {
                                case false:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.variableText.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                                case true:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.variableName.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                            }
                            break;
                    }
                }

                if (equivalenceitems.Count != 0)
                {
                    var conceptitems = (from r in equivalenceitems
                                        group r by r.questionconceptitem into r1
                                        select new SelectListItem { Text = r1.Key.ToString(), Value = r1.Key.ToString() }).ToList();

                    i++;

                    foreach (var result in equivalenceitems)
                    {
                        EquivalenceItem item = new EquivalenceItem();
                        item.counter = j;
                        item.name = result.variableName;
                        item.description = result.variableText;
                        item.variableName = result.variableName;
                        item.variableText = result.variableText;
                        item.variableItem = result.variableItem;
                        item.questionName = result.questionName;
                        item.questionText = result.questionText;
                        item.questionItem = result.questionItem;
                        item.study = result.study;
                        item.studyGroup = result.studyGroup;
                        item.dataset = result.dataset;
                        item.concept = result.concept;
                        item.questionconceptitem = result.questionconceptitem;
                        item.concepts = conceptitems.Count;
                        item.study = result.study;

                        item.uniqueId = i;
                        item.equivalence = selectedwords.Value.Trim();
                        item.column = waves.Where(a => a.StudyName == result.studyGroup).Select(a => { return a.ID; }).FirstOrDefault();
                        item.selected = true;
                        item.dataset = result.dataset;
                        if (conceptitems.Count > 1)
                        {
                            item.conceptitems = conceptitems;
                            model.AllResults.Add(item);
                        }
                        else
                        {
                            model.AllResults.Add(item);
                        }


                    }
                }
            }
            model.AllResults = model.AllResults.OrderBy(x => x.dataset).OrderBy(x => x.uniqueId).ToList();
            return model;
        }



        public EquivalenceModel GetEquivalences(EquivalenceModel model, HttpPostedFileBase postedFile)
        {

            try
            {
                string fileExtension = Path.GetExtension(postedFile.FileName);
                if (fileExtension != ".csv")
                {
                    return model;
                }
                string wordselection = "";
                List<Word> equivalences = new List<Word>();
                using (var sreader = new StreamReader(postedFile.InputStream))
                {
                    while (!sreader.EndOfStream)
                    {
                        string[] rows = sreader.ReadLine().Split(',');
                        Word word = new Word();
                        word.Value = rows[0].ToString();
                        equivalences.Add(word);
                        wordselection = wordselection + rows[0].ToString() + ",";
                    }
                }
                model.WordList = equivalences;
                model.Results = new List<StudyItem>();
                model.FileName = postedFile.FileName.Replace(".csv","");
                return model;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return model;

        }

        public EquivalenceModel SaveItem(EquivalenceModel model, string word, string wordselection)
        {
            // Add new Equivalence to the list

            model.WordList = new List<Word>();
            wordselection = wordselection + word + ",";
            model.WordList = EquivalenceHelper.GetList(wordselection);
            model.WordSelection = wordselection;

            return model;
        }

       
        public ActionResult DeleteItem(string selectedItems, string word, string wordselection)
        {
            // Remove Equivalence from list

            wordselection = wordselection.Replace(word + ",", "");
            return RedirectToAction("Equivalences", new { wordselection = wordselection });
        }

        public EquivalenceModel LoadSelectedStudies(EquivalenceModel model, List<TreeViewNode> items)
        {
            // Populate selected studies in the model with selected study nodes from the Index view

            List<string> selectedstudies = new List<string>();

            foreach (var item in items)
            {
                selectedstudies.Add(item.id.Replace(" ", ",") + "," + item.text);
            }
            model.SelectedStudies = selectedstudies;

            return model;
        }

        public static EquivalenceModel GetAllRepositoryItems(EquivalenceModel model, string agency, Guid id, string group, Guid studyitem, int study)
        {
            // Retrieves data for the selected study from the repository to be used in determining equivalences

            MultilingualString.CurrentCulture = "en-GB";

            var client = ClientHelper.GetClient();

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            var studyUnit = item as StudyUnit;

            SetSearchFacet setFacet = new SetSearchFacet();
            setFacet.ItemTypes.Add(DdiItemType.QuestionItem);

            if (studyUnit == null) return model;
            var matches = client.SearchTypedSet(studyUnit.CompositeId,
                setFacet);
            var infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection()).ToList();
            foreach (var info in infoList)
            {

                var children = Helper.GetReferences(info.AgencyId, info.Identifier).ToList();
                var variables = children.Where(x => x.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d")).ToList();
                var concept = children.Where(x => x.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205")).FirstOrDefault();
                foreach (var variable in variables)
                {
                    if (variable.IsDeprecated == false)
                    {
                        var latestvariable = from r in variables
                                             where r.Identifier == variable.Identifier
                                             group r by r.Identifier into r1
                                             select new { Identifier = r1.Key, VersionNumber = (from t2 in r1 select t2.Version).Max() };
                        latestvariable = latestvariable.ToList();


                        if (variable.Version == latestvariable.FirstOrDefault().VersionNumber)
                        {
                            EquivalenceItem equivalenceitem = new EquivalenceItem();
                            equivalenceitem.study = info.AgencyId;
                            equivalenceitem.name = variable.ItemName.FirstOrDefault().Value;
                            equivalenceitem.description = variable.Label.FirstOrDefault().Value;
                            equivalenceitem.variableAgency = variable.AgencyId;
                            equivalenceitem.variableName = variable.ItemName.FirstOrDefault().Value;
                            equivalenceitem.variableText = variable.Label.FirstOrDefault().Value;
                            if (variable.Label.FirstOrDefault().Value != null) equivalenceitem.variableOrdered = GetOrderedText(variable.Label.FirstOrDefault().Value);
                            equivalenceitem.variableItem = variable.CompositeId.ToString();
                            equivalenceitem.variableIdentifier = variable.Identifier;
                            equivalenceitem.questionName = info.ItemName.FirstOrDefault().Value;
                            equivalenceitem.questionText = info.Summary.FirstOrDefault().Value;
                            if (info.Summary.FirstOrDefault().Value != null) equivalenceitem.questionOrdered = GetOrderedText(info.Summary.FirstOrDefault().Value);
                            equivalenceitem.questionItem = info.CompositeId.ToString();
                            equivalenceitem.questionIdentifier = info.Identifier;
                            equivalenceitem.questionconceptitem = concept.CompositeId.ToString();                           
                            equivalenceitem.studyGroup = group;
                            equivalenceitem.set = study;
                            if (concept != null) equivalenceitem.concept = concept.Label.Values.FirstOrDefault();
                            if (study == 1) { model.MasterItems.Add(equivalenceitem); }

                            var references = Helper.GetReferences(variable.AgencyId, variable.Identifier).Where(x => x.ItemType == new Guid("3b438f9f-e039-4eac-a06d-3fa1aedf48bb")).ToList();
                            var references1 = Helper.GetReferences(variable.AgencyId, variable.Identifier).ToList();
                            if (references.Count != 0)
                            {
                                var dataset = Helper.GetReferences(references.FirstOrDefault().AgencyId, references.FirstOrDefault().Identifier).OrderBy(x => x.Version).LastOrDefault();
                                var datasetrepositoryitem = client.GetLatestRepositoryItem(dataset.Identifier, dataset.AgencyId);
                                equivalenceitem.dataset = GetCurrentDataSet(datasetrepositoryitem.Item);
                            }
                            else
                            {
                                equivalenceitem.dataset = "No Dataset";
                            }
                            model.AllItems.Add(equivalenceitem);
                         
                        }
                    }

                }
            }

            var waves = from r in model.AllItems
                        group r by r.dataset into r1
                        select new { Name = r1.Key };
            waves = waves.OrderBy(x => x.Name).ToList();
            List<Study> datasets = new List<Study>();

            int k = 1;
            foreach (var wave in waves)
            {
                var existing = model.Datasets.Where(x => x.StudyName == wave.Name).ToList();
                if (existing.Count == 0)
                {
                    Study dataset = new Study();
                    dataset.ID = k;
                    dataset.StudyName = wave.Name;
                    dataset.Value = group;
                    model.Datasets.Add(dataset);
                    k++;
                }
            }

            return model;
        }

        private static string GetCurrentDataSet(string input)
        {
            int spos = input.IndexOf("<r:AlternateTitle>");
            int epos = input.IndexOf("</r:AlternateTitle");
            int spos1 = input.IndexOf("<r:String", spos);
            string test = input.Substring(spos1);
            string alttitle = input.Substring(spos, epos - spos).Replace("</r:String>", "");
            alttitle = alttitle.Replace("<r:AlternateTitle>", "");
            spos = alttitle.IndexOf(">");
            alttitle = alttitle.Substring(spos, alttitle.Length - spos).Replace(">", "");
            return alttitle;
        }

      
        private static string GetOrderedText(string sentance)
        {
            // Creates an ordered list of all words greater than three characters to be used when the match all words option is selected on the Index screen

            string ordered = "";
            GFG gg = new GFG();
            List<string> words = sentance.Split(' ').ToList();           
            words.Sort(gg);

            foreach (var word in words)
            {
                if (word.Length > 3) ordered = ordered + word + " ";
            }
           
            
            return ordered;
        }      

        private List<SelectListItem> GetAllMethods()
        {
            // Creates a selectlist of all the methods to be used for Equivalences

            List<SelectListItem> methods = new List<SelectListItem>();
            methods.Add(new SelectListItem() { Text = "Match Text", Value = "0" });
            methods.Add(new SelectListItem() { Text = "Match All Words", Value = "1" });
            return methods;
        }

        public List<EquivalenceItem> ProcessMethods(List<EquivalenceItem> items, List<EquivalenceItem> master, List<string> selectedmethods, string type, int i, int j)
        {
            List<EquivalenceItem> allresults = new List<EquivalenceItem>();
            if (selectedmethods != null)
            {
                foreach (var method in selectedmethods)
                {

                    switch (method)
                    {
                        case "Match Text":
                            allresults = GenerateEquivalences(items, master, type, "text",i,j);
                            break;
                        case "Match All Words":
                            allresults = GenerateEquivalences(items, master,type, "words",i, j);
                            break;
                    }
                }
            }
            return allresults;
           
        }

        private List<EquivalenceItem> GenerateEquivalences(List<EquivalenceItem> items, List<EquivalenceItem> master, string type, string method, int i, int j)
        {
            var waves = GetAllStudies(items);
            List<EquivalenceItem> allresults = new List<EquivalenceItem>();             
            var item3 = items.Where(x => x.set == 1).OrderBy(x => x.questionText).ToList();
            List<EquivalenceItem> equivalences = new List<EquivalenceItem>();
            foreach (var record1 in item3)
            {
                switch (method)
                {
                    case "text":
                        switch (type)
                        {
                            case "Variable":
                                equivalences = items.Where(u => u.removed != true).Where(u => u.variableText.Equals(record1.variableText)).ToList();
                                break;
                            case "Question":
                                equivalences = items.Where(u => u.removed != true).Where(u => u.questionText.Equals(record1.questionText)).ToList();
                                break;

                        }
                        break;
                    case "words":
                        switch (type)
                        {
                            case "Variable":
                                equivalences = items.Where(u => u.removed != true).Where(u => u.variableOrdered.Equals(record1.variableOrdered)).ToList();
                                break;
                            case "Question":
                                equivalences = items.Where(u => u.removed != true).Where(u => u.questionOrdered.Equals(record1.questionOrdered)).ToList();
                                break;

                        }
                        break;
                }
                        
                if (equivalences.Count > 1)
                {
                    i++;
                    foreach (var equivalence in equivalences)
                    {
                        j++;
                        EquivalenceItem item = new EquivalenceItem();
                        item = equivalence;
                        item.counter = j;                        
                        item.uniqueId = i;
                        switch (method)
                        {
                            case "text":
                                item.equivalence = "Same Text " + item.uniqueId;
                                break;
                            case "words":
                                item.equivalence = "Same Words " + item.uniqueId;
                                break;
                        }
                        item.selected = true;
                        item.column = waves.Where(a => a.StudyName == equivalence.studyGroup).Select(a => { return a.ID; }).FirstOrDefault();
                        allresults.Add(item);
                        items.Where(a => a.variableItem == equivalence.variableItem).SetValue(a => a.removed = true).ToList();
                        var removed2 = items.Where(a => a.removed == true).ToList().Count;
                    }                   
                }
            }
            var removed = items.Where(a => a.removed == true).ToList();
            return allresults;
        }

        private EquivalenceModel FileReader(HttpPostedFileBase postedFile)
        {
            // Processes a CSV of Equivalences to create the Equivalence Model

            EquivalenceModel model = new EquivalenceModel();
            List<EquivalenceItem> equivalences = new List<EquivalenceItem>();
            List<EquivalenceItem> newequivalences = new List<EquivalenceItem>();

            model.Datasets = new List<Study>();

            string sFileContents = "";

            using (var oStreamReader = new StreamReader(postedFile.InputStream))
            {
                sFileContents = oStreamReader.ReadToEnd();

            }

            var client = ClientHelper.GetClient();
            List<string[]> oCsvList = new List<string[]>();

            string[] sFileLines = sFileContents.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string sFileLine in sFileLines)
            {
                string[] oCsvItem = new string[11];

                oCsvItem = sFileLine.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (i != 0)
                {
                    EquivalenceItem equivalence = new EquivalenceItem();
                    var id = oCsvItem[4].Substring(oCsvItem[4].IndexOf(":") + 1);
                    id = id.Substring(0, id.IndexOf(":"));
                    var selecteditems = oCsvItem[4].Split(':').ToList();

                    equivalence.equivalence = oCsvItem[1];
                    equivalence.questionName = oCsvItem[2];
                    equivalence.questionText = oCsvItem[3];
                    equivalence.questionItem = oCsvItem[4];
                    equivalence.questionconceptitem = oCsvItem[5];
                    equivalence.variableName = oCsvItem[6];
                    equivalence.variableText = oCsvItem[7];
                    equivalence.variableItem = oCsvItem[8];
                    equivalence.study = oCsvItem[9];
                    equivalence.studyGroup = oCsvItem[10];
                    equivalence.concept = oCsvItem[11];
                    equivalence.dataset = oCsvItem[12];
                    equivalence.name = oCsvItem[6];             // populates the edit field for variable name with the current variable name
                    equivalence.description = oCsvItem[7];      // populates the edit field for variable text with the current variable text
                    equivalence.uniqueId = Convert.ToInt16(oCsvItem[0]);
                    equivalence.selected = true;
                    equivalences.Add(equivalence);
                }
                i++;
            }
            equivalences.SetValue(a => a.selected = true).ToList();
            newequivalences = DetectMultipleEquivalences(equivalences);
          

            model.AllItems = newequivalences;
            model.AllResults = newequivalences;

            var waves = from r in model.AllItems
                        group r by r.dataset into r1
                        select new { Name = r1.Key };
            waves = waves.OrderBy(x => x.Name).ToList();
            List<Study> datasets = new List<Study>();

            int k = 1;
            foreach (var wave in waves)
            {
                var existing = model.Datasets.Where(x => x.StudyName == wave.Name).ToList();
                if (existing.Count == 0)
                {
                    Study dataset = new Study();
                    dataset.ID = k;
                    dataset.StudyName = wave.Name;
                    model.Datasets.Add(dataset);
                    k++;
                }
            }

            return model;

        }

        public List<EquivalenceItem> DetectMultipleEquivalences(List<EquivalenceItem> equivalences)
        {
            var client = ClientHelper.GetClient();
            List<EquivalenceItem> newequivalences = new List<EquivalenceItem>();
            int counter = 1;

            var conceptitems = (from r in equivalences
                                where r.uniqueId == 1
                                group r by r.concept into r1
                                select new SelectListItem { Text = r1.Key.ToString(), Value = r1.Key.ToString() }).ToList();

            foreach (var equivalence in equivalences)
            {

                if (equivalence.uniqueId != counter)
                {
                    conceptitems = (from r in equivalences
                                    where r.uniqueId == equivalence.uniqueId
                                    group r by r.concept into r1
                                    select new SelectListItem { Text = r1.Key.ToString(), Value = r1.Key.ToString() }).ToList();
                }

                EquivalenceItem newequivalence = new EquivalenceItem();
                newequivalence.equivalence = equivalence.equivalence;
                newequivalence.questionName = equivalence.questionName;
                newequivalence.questionText = equivalence.questionText;
                newequivalence.questionItem = equivalence.questionItem;
                newequivalence.variableName = equivalence.variableName;
                newequivalence.variableText = equivalence.variableText;
                newequivalence.variableItem = equivalence.variableItem;
                newequivalence.study = equivalence.study;
                newequivalence.studyGroup = equivalence.studyGroup;
                newequivalence.concept = equivalence.concept;
                newequivalence.questionconceptitem = equivalence.questionconceptitem;
                newequivalence.dataset = equivalence.dataset;
                newequivalence.name = equivalence.name;
                newequivalence.description = equivalence.description;
                newequivalence.uniqueId = equivalence.uniqueId;
                newequivalence.concepts = conceptitems.Count;
                newequivalence.selected = equivalence.selected;
                if (conceptitems.Count > 1)
                {
                    newequivalence.conceptitems = conceptitems;
                }
                counter = equivalence.uniqueId;
                List<string> selecteditems = newequivalence.questionconceptitem.Split(':').ToList();
                newequivalences.Add(newequivalence);

            }
            return newequivalences;
        }

        [HttpPost]
        public ContentResult ChangeTheValue(EquivalenceModel model)
        {
            string OutputCSV = null;
            var m = new EquivalenceModel();
            m.AllItems = model.AllItems;
            m.AllResults = model.AllResults.Where(a => a.concepts > 1).ToList();
            m.Datasets = model.Datasets;
            var allresults = model.AllResults.Where(a => a.concepts > 1).Where(a => a.selectedconcept != null).ToList();
            OutputCSV = "Question Item URN, Question Group URN, New Topic, Related Variables\n";
            foreach (var allresult in allresults)
            {
                OutputCSV = OutputCSV + allresult.questionItem + ", " + allresult.questionconceptitem + ", " + allresult.selectedconcept + ", ";
                var test = model.AllItems.Where(a => a.concept == allresult.concept).ToList();

                var relatedvariables = model.AllResults.Where(a => a.questionItem == allresult.questionItem).ToList();
                foreach (var relatedvariable in relatedvariables)
                {
                    OutputCSV = OutputCSV + relatedvariable.variableItem + ";";
                }
                OutputCSV = OutputCSV + "\n";

            }
            var m2 = m.AllResults.Where(a => a.selectedconcept != null).ToList();
            var serializer = new JavaScriptSerializer();
            var viewModel = new EquivalenceModel();
            return Content(OutputCSV);

        }

        [HttpPost]
        public ContentResult DisplayCsv(EquivalenceModel model)
        {
            string OutputCSV = null;
            var m = new EquivalenceModel();
            m.AllItems = model.AllItems;
            m.Datasets = model.Datasets;
            var allresults = model.AllResults.Where(a => a.concepts > 1).Where(a => a.selectedconcept != null).ToList();
            OutputCSV = "ID, Search Term, Question Name, Question Text, Question Item, QuestionConceptItem, Variable Name, Variable Text, Variable Item, Agency, Study, Concept, Dataset\n";
            foreach (var allresult in model.AllResults)
            {
                if (allresult.dataset == null) { allresult.dataset = "No Dataset"; }
                OutputCSV = OutputCSV + allresult.uniqueId + ", " + allresult.equivalence + ", " + allresult.questionName + ", " + allresult.questionText.Replace(",",".") + ", " + allresult.questionItem + ", " +
                    allresult.questionconceptitem + ", " + allresult.variableName + ", " + allresult.variableText.Replace(",",".") + ", " + allresult.variableItem + ", "+ allresult.study + "," + allresult.studyGroup + "," + allresult.concept + "," +
                    allresult.dataset;
                OutputCSV = OutputCSV + "\n";

            }
           
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var outputcsv = serializer.Serialize(OutputCSV); 
            return Content(outputcsv);

        }
    }
       
    public static class PHVExtensions
    {
        public static IEnumerable<T> SetValue<T>(this IEnumerable<T> items, Action<T>
             updateMethod)
        {
            foreach (T item in items)
            {
                updateMethod(item);
            }
            return items;
        }
    }

}