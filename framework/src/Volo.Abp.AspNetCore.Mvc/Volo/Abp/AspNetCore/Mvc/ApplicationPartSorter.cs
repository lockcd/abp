using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Volo.Abp.Modularity;

namespace Volo.Abp.AspNetCore.Mvc;

/// <summary>
/// This class is used to align order of the MVC Application Parts with the order of
/// ABP module dependencies.
/// </summary>
public static class ApplicationPartSorter
{
    public static void Sort(ApplicationPartManager partManager, IModuleContainer moduleContainer)
    {
        var sortedParts = new List<ApplicationPart>();
        foreach (var module in moduleContainer.Modules)
        {
            var parts = partManager.ApplicationParts.Where(part =>
                    (part is AssemblyPart ap && ap.Assembly == module.Assembly) ||
                    (part is CompiledRazorAssemblyPart crp && crp.Assembly == module.Assembly))
                .ToList();

            if (!parts.IsNullOrEmpty())
            {
                sortedParts.AddRange(parts);
            }
        }
        sortedParts.Reverse();
        partManager.ApplicationParts.Clear();
        foreach (var applicationPart in sortedParts)
        {
            partManager.ApplicationParts.Add(applicationPart);
        }
    }
}
