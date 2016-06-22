using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace FjeeaResource
{
    public class ResourceGetter
    {
        public static ResourceSet GetResources()
        {
            return Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        }
    }
}
