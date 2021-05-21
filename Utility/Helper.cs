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

namespace ColecticaSdkMvc.Utility
{
	public static class Helper
	{

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
            facet.UseDistinctResultItem = false;

            var referencingItemsDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);
            List<RepositoryItemMetadata> referenceItems = referencingItemsDescriptions.ToList();
            return referenceItems;
        }  

    }
}