// This class is auto generated from UILinksMapping.xml using 'Edit>>Paste Special' option from menu.
//Reference URL - http://stackoverflow.com/questions/3187444/convert-xml-string-to-object/19613953#19613953

namespace AdminPortal.BusinessServices.LandingPage
{

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class UiLinks
    {

        private UiLinksLandingPageTab[] landingPageTabField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LandingPageTab")]
        public UiLinksLandingPageTab[] LandingPageTab
        {
            get
            {
                return this.landingPageTabField;
            }
            set
            {
                this.landingPageTabField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class UiLinksLandingPageTab
    {

        private UiLinksLandingPageTabSection[] sectionField;

        private string keyField;

        private string textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Section")]
        public UiLinksLandingPageTabSection[] Section
        {
            get
            {
                return this.sectionField;
            }
            set
            {
                this.sectionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class UiLinksLandingPageTabSection
    {

        private UiLinksLandingPageTabSectionMenuItem[] menuItemField;

        private string keyField;

        private string textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MenuItem")]
        public UiLinksLandingPageTabSectionMenuItem[] MenuItem
        {
            get
            {
                return this.menuItemField;
            }
            set
            {
                this.menuItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class UiLinksLandingPageTabSectionMenuItem
    {

        private string keyField;

        private string textField;

        private string linkField;

        private string regionIndicatorField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Link
        {
            get
            {
                return this.linkField;
            }
            set
            {
                this.linkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RegionIndicator
        {
            get
            {
                return this.regionIndicatorField;
            }
            set
            {
                this.regionIndicatorField = value;
            }
        }
    }







}