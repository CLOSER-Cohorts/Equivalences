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
using System.Text;

namespace ColecticaSdkMvc.Utility
{
	public static class EquivalenceHelper
	{
        public static EquivalenceModel BuildStudiesTree(EquivalenceModel model, List<TreeViewNode> nodes)
        {
            MultilingualString.CurrentCulture = "en-GB";

            SearchFacet facet = new SearchFacet();
            facet.ItemTypes.Add(DdiItemType.Group);
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
            var client1 = ClientHelper.GetClient();
            SearchResponse response = client1.Search(facet);
            facet.ItemTypes.Add(DdiItemType.StudyUnit);
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
            var client2 = ClientHelper.GetClient();
            SearchResponse allsweeps = client2.Search(facet);
            model.Studies = LoadStudies(model, response);
            List<StudyItem> studies = new List<StudyItem>();
            int i = 1;
            foreach (var result in response.Results)
            {
                State cstate = new State();
                cstate.selected = false;

                nodes.Add(new TreeViewNode { id = result.AgencyId, parent = "#", text = result.DisplayLabel, state = cstate });
                studies = BuildSweepsTree(studies, model.SelectedStudies, allsweeps, result.DisplayLabel, result.AgencyId, nodes, result.AgencyId);
                i++;
            }

            model.Results = studies;
            return model;
        }

        public static List<StudyItem> BuildSweepsTree(List<StudyItem> model, List<string> selecteditems, SearchResponse allsweeps, string study, string agency, List<TreeViewNode> nodes, string parent)
        {
            int i = 1;
            bool found = false;
            foreach (var sweep in allsweeps.Results)
            {
                StudyItem item = new StudyItem();
                item.AgencyId = sweep.AgencyId;
                item.DisplayLabel = sweep.DisplayLabel;
                item.Identifier = sweep.Identifier;
                item.ReferenceItem = study;
                if (sweep.AgencyId == agency)
                {
                    State cstate = new State();
                    List<string> search = new List<string>() { item.AgencyId + "," + item.Identifier.ToString() + "," + item.DisplayLabel };
                    if (selecteditems != null) { found = search.Any(s => selecteditems.Contains(s)); } else { found = false; }
                    cstate.selected = found;
                    nodes.Add(new TreeViewNode { id = parent + " " + item.Identifier.ToString(), parent = parent.ToString(), text = item.DisplayLabel, state = cstate });
                }
                i++;
            }
            model = model.OrderBy(r => r.ReferenceItem).ToList();
            return model;
        }

        public static List<SelectListItem> LoadStudies(EquivalenceModel model, SearchResponse response)
        {
            List<SelectListItem> studies = new List<SelectListItem>();
            foreach (var result in response.Results)
            {
                studies.Add(new SelectListItem() { Value = result.AgencyId, Text = result.DisplayLabel });
            }
            return studies;
        }

        public static List<Word> GetList(string wordselection)
        {
            List<Word> wordlist = new List<Word>();
            List<string> wordlist2 = wordselection.Split(',').ToList();
            foreach (var item in wordlist2)
            {
                Word currentword = new Word();
                currentword.Value = item;
                if (item != "") wordlist.Add(currentword);
            }
            return wordlist;
        }       
    }
}