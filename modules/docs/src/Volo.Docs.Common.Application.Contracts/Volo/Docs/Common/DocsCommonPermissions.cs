using Volo.Abp.Reflection;

namespace Volo.Docs.Common
{
    public class DocsCommonPermissions
    {
        public const string GroupName = "Docs.Common";

        public static class Documents
        {
            public const string PdfCreation = GroupName + ".PdfCreation";
        }

        public static string[] GetAll()
        {
            return ReflectionHelper.GetPublicConstantsRecursively(typeof(DocsCommonPermissions));
        }
    }
}
