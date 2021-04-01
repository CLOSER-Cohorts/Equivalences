using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using ColecticaSdkMvc.Models;
using ColecticaSdkMvc.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ColecticaSdkMvc.Controllers
{

    public class HomeController : Controller
    {     

        // save
        public ActionResult List(ItemTypesModel model, string agency = null, string id = null, long version = 0)
        {
            if (agency != null)
            {
                model.ItemType = "Question Group";
                var clientitem = ClientHelper.GetClient();
                var item1 = clientitem.GetRepositoryItem(new Guid(id), agency, version);
            }

            model.ItemTypes = GetItemTypes();

            // Since all the information in the sample Repository is
            // in en-US, we can set the CurrentCulture here and 
            // use MultilingualString's Current property to access
            // the text.
            //
            // If your data has multiple languages, you may want to
            // access those specific languages instead.
            MultilingualString.CurrentCulture = "en-GB";

            // Create a new SearchFacet that will find all
            // StudyUnits, CodeSchemes, and CategorySchemes.

            SearchFacet facet = new SearchFacet();
            switch (model.ItemType)
            {
                case "Action":
                    facet.ItemTypes.Add(DdiItemType.Archive);
                    break;
                case "Archive":
                    facet.ItemTypes.Add(DdiItemType.Archive);
                    break;
                case "Category":
                    facet.ItemTypes.Add(DdiItemType.Category);
                    break;
                case "Category Group":
                    facet.ItemTypes.Add(DdiItemType.CategoryGroup);
                    break;
                case "Category Set":
                    facet.ItemTypes.Add(DdiItemType.CategoryScheme);
                    break;
                case "Code List":
                    facet.ItemTypes.Add(DdiItemType.CodeList);
                    break;
                case "Code List Group":
                    facet.ItemTypes.Add(DdiItemType.CodeListGroup);
                    break;
                case "Code List Scheme":
                    facet.ItemTypes.Add(DdiItemType.CodeListScheme);
                    break;
                case "Code Set":
                    facet.ItemTypes.Add(new Guid("8b108ef8-b642-4484-9c49-f88e4bf7cf1d"));
                    break;
                case "Concept":
                    facet.ItemTypes.Add(DdiItemType.Concept);
                    break;
                case "Concept Group":
                    facet.ItemTypes.Add(DdiItemType.ConceptGroup);
                    break;
                case "Concept Scheme":
                    facet.ItemTypes.Add(DdiItemType.ConceptScheme);
                    break;
                case "Data Collection":
                    facet.ItemTypes.Add(DdiItemType.DataCollection);
                    break;
                case "Data File":
                    facet.ItemTypes.Add(new Guid("a51e85bb-6259-4488-8df2-f08cb43485f8"));
                    break;
                case "Group":
                    facet.ItemTypes.Add(DdiItemType.Group);
                    break;
                case "Instrument":
                    facet.ItemTypes.Add(DdiItemType.Instrument);
                    break;
                case "Question Item":
                    facet.ItemTypes.Add(DdiItemType.QuestionItem);
                    break;
                case "Question Group":
                    facet.ItemTypes.Add(DdiItemType.QuestionGroup);
                    break;
                case "Question Set":
                    facet.ItemTypes.Add(DdiItemType.QuestionScheme);
                    break;
                case "Study":
                    facet.ItemTypes.Add(DdiItemType.StudyUnit);
                    break;
                case "Variable":
                    facet.ItemTypes.Add(DdiItemType.Variable);
                    break;
                case "Variable Group":
                    facet.ItemTypes.Add(DdiItemType.VariableGroup);
                    break;
                case "Variable Scheme":
                    facet.ItemTypes.Add(DdiItemType.VariableScheme);
                    break;
             
                case "Variable Statistic":
                    facet.ItemTypes.Add(DdiItemType.VariableStatistic);
                    break;
                default:
                    facet.ItemTypes.Add(DdiItemType.Group);
                    break;

            }


            // Set the sort order of the results. Options are 
            // Alphabetical, ItemType, MetadataRank, and VersionDate.
            facet.ResultOrdering = SearchResultOrdering.Alphabetical;
            facet.SearchLatestVersion = true;            
      

            // Now that we have a facet, search for the items in the Repository.
            // The client object takes care of making the Web Services calls.
            var client = ClientHelper.GetClient();
            SearchResponse response = client.Search(facet);

            // Create the model object, and add all the search results to 
            model.Results = response.Results.OrderBy(a => a.VersionDate).ToList();

            // Return the view, passing in the model.
            return View(model);
        }
        
        // save
        private List<SelectListItem> GetItemTypes()
        {
            List<SelectListItem> itemtypes = new List<SelectListItem>();           
            itemtypes.Add(new SelectListItem() { Text =  "Archive", Value = "Archive" });
            itemtypes.Add(new SelectListItem() { Text =  "Category", Value = "Category" });
            itemtypes.Add(new SelectListItem() { Text =  "Category Group", Value = "Category Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Category Set", Value = "Category Set" });
            itemtypes.Add(new SelectListItem() { Text =  "Code List", Value = "Code List" });
            itemtypes.Add(new SelectListItem() { Text =  "Code List Group", Value = "Code List Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Code List Scheme", Value = "Code List Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Code Set", Value = "Code Set" });
            itemtypes.Add(new SelectListItem() { Text =  "Concept", Value = "Concept" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Group", Value = "Concept Group" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Scheme", Value = "Concept Scheme" });
            itemtypes.Add(new SelectListItem() { Text =  "Data Collection", Value = "Data Collection" });
            itemtypes.Add(new SelectListItem() { Text = "Data File", Value = "Data File" });
            itemtypes.Add(new SelectListItem() { Text =  "Group", Value = "Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Instrument", Value = "Instrument" });
            itemtypes.Add(new SelectListItem() { Text = "Question", Value = "Question" });
            itemtypes.Add(new SelectListItem() { Text = "Question Item", Value = "Question Item" });
            itemtypes.Add(new SelectListItem() { Text = "Question Group", Value = "Question Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Study", Value = "Study" });
            itemtypes.Add(new SelectListItem() { Text = "Variable", Value = "Variable" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Group", Value = "Variable Group" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Scheme", Value = "Variable Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Statistic", Value = "Variable Statistic" });
            return itemtypes;
        }
    }
}
