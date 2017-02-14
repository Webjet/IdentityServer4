using System;
using System.Collections.Generic;
using System.Linq;
using Webjet.DotNet.Common.Collections;
namespace AdminPortal.BusinessServices
{
    public class ResourceIdsConfigValidator
    {


        public string Validate(ResourceToApplicationRolesMap map,string filepath)
        {
            string message = "";
             var list = FindDuplicatesInResourceItemsWithRoles(map);
            if (list.Count > 0)
            {
                message = "Duplicated resource entries " + list.ToCSVString() + " in configuration file " + filepath;
            }
            return message;
        }
        internal List<string> FindDuplicatesInResourceItemsWithRoles(ResourceToApplicationRolesMap mapper)
        {
            //http://stackoverflow.com/questions/18547354/c-sharp-linq-find-duplicates-in-list
            var list = mapper.ResourceToRoles.GroupBy(x => x.ResourceId).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            return list;
        }
    }
}