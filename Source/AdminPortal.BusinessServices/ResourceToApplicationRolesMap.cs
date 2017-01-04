using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPortal.BusinessServices
{
    #region Summary
    // This class is auto generated from RoleBasedMenuItemMap.xml using 'Edit>>Paste Special' option from menu.
    //Reference URL - http://stackoverflow.com/questions/3187444/convert-xml-string-to-object/19613953#19613953
    #endregion

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ResourceToApplicationRolesMap
    {

        private ResourceToApplicationRolesMapResourceToRoles[] resourceToRolesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ResourceToRoles")]
        public ResourceToApplicationRolesMapResourceToRoles[] ResourceToRoles
        {
            get
            {
                return this.resourceToRolesField;
            }
            set
            {
                this.resourceToRolesField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ResourceToApplicationRolesMapResourceToRoles
    {

        private string resourceIdField;

        private string rolesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ResourceId
        {
            get
            {
                return this.resourceIdField;
            }
            set
            {
                this.resourceIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Roles
        {
            get
            {
                return this.rolesField;
            }
            set
            {
                this.rolesField = value;
            }
        }
    }


}
