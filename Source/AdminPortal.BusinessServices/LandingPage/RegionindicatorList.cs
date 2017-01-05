using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPortal.BusinessServices.LandingPage
{


    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class RegionIndicatorList
    {

        private RegionIndicatorListRegionIndicator[] regionIndicatorField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("RegionIndicator")]
        public RegionIndicatorListRegionIndicator[] RegionIndicator
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

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RegionIndicatorListRegionIndicator
    {

        private string idField;

        private string showDescriptionField;

        private string descriptionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ShowDescription
        {
            get
            {
                return this.showDescriptionField;
            }
            set
            {
                this.showDescriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }
    }



    //public class Rootobject
    //{
    //    public Regionindicator[] RegionIndicator { get; set; }
    //}

    //public class Regionindicator
    //{
    //    public string Id { get; set; }
    //    public string ShowDescription { get; set; }
    //    public string Description { get; set; }
    //}



}
