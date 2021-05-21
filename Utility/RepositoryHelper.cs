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
	public static class RepositoryHelper
	{

        public static string GetStudy(string agency, Guid id)
        {
            try
            {
                MultilingualString.CurrentCulture = "en-GB";

                var client = ClientHelper.GetClient();

                // Retrieve the requested item from the Repository.
                // Populate the item's children, so we can display information about them.
                IVersionable item = client.GetLatestItem(id, agency,
                     ChildReferenceProcessing.Populate);
                // Use a graph search to find a list of all items that 
                // directly reference this item.
                GraphSearchFacet facet = new GraphSearchFacet();
                facet.TargetItem = item.CompositeId;
                facet.UseDistinctResultItem = true;
                var references = client.GetRepositoryItemDescriptionsByObject(facet);
                var referencingItemDescription = (from a in references
                                                  where a.ItemType == new Guid("0a63fcf6-ffdd-4214-b38c-147d6689d6a1")
                                                  select a).FirstOrDefault();
                for (int i = 1; i < 3; i++)
                {
                    if (referencingItemDescription != null)
                    {
                        referencingItemDescription = GetReference(referencingItemDescription.AgencyId, referencingItemDescription.Identifier);
                    }
                }
                if (referencingItemDescription == null)
                {
                    return null;
                }
                else
                {
                    return referencingItemDescription.DisplayLabel;
                }
            }
            catch
            {
                return null;
            }
        }

        public static RepositoryItemMetadata GetReference(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

            var client = ClientHelper.GetClient();

            // Retrieve the requested item from the Repository.
            // Populate the item's children, so we can display information about them.
            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);
            // Use a graph search to find a list of all items that 
            // directly reference this item.
            GraphSearchFacet facet = new GraphSearchFacet();
            facet.TargetItem = item.CompositeId;
            facet.UseDistinctResultItem = true;

            var referencingItemDescription = client.GetRepositoryItemDescriptionsByObject(facet).FirstOrDefault();
            return referencingItemDescription;
        }

        public static List<RepositoryItemMetadata> GetReferences(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

            var client = ClientHelper.GetClient();

            // Retrieve the requested item from the Repository.
            // Populate the item's children, so we can display information about them.
            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);
            // Use a graph search to find a list of all items that 
            // directly reference this item.
            GraphSearchFacet facet = new GraphSearchFacet();
            facet.TargetItem = item.CompositeId;
            facet.UseDistinctResultItem = true;

            var referencingItemDescriptions = client.GetRepositoryItemDescriptionsByObject(facet).ToList();
            return referencingItemDescriptions;
        }

        public static RepositoryItemMetadata GetConcept(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

            var client = ClientHelper.GetClient();

            // Retrieve the requested item from the Repository.
            // Populate the item's children, so we can display information about them.
            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);
            // Use a graph search to find a list of all items that 
            // directly reference this item.
            GraphSearchFacet facet = new GraphSearchFacet();
            facet.TargetItem = item.CompositeId;
            facet.UseDistinctResultItem = true;

            var referencingItemDescription = client.GetRepositoryItemDescriptionsByObject(facet).Where(x => x.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205")).FirstOrDefault();
            return referencingItemDescription;
        }
    
        public static List<RepositoryItemMetadata> GetAllRepositoryQuestions(string agency, Guid id, WcfRepositoryClient client)
        {
            // Retrieves data for the selected study from the repository to be used in determining equivalences

            List<RepositoryItemMetadata> infoList = new List<RepositoryItemMetadata>();
            MultilingualString.CurrentCulture = "en-GB";

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            var studyUnit = item as StudyUnit;

            SetSearchFacet setFacet = new SetSearchFacet();
            setFacet.ItemTypes.Add(DdiItemType.QuestionItem);

            if (studyUnit == null) return infoList;
            var matches = client.SearchTypedSet(studyUnit.CompositeId,
                setFacet);
            infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection()).ToList();
            return infoList;

        }
    }
}