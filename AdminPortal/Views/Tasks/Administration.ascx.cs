#region Summary
///////////////////////////////////////////////////////////////////////////////
/// AUTHOR		 : vajiraw 
/// CREATE DATE	 : 27 Oct 2003  
/// PURPOSE		 : 
/// SPECIAL NOTES: 
/// FILE NAME	 : $Workfile: Administration.ascx.cs $	
/// VSS ARCHIVE	 : $Archive: /src/main/TSA/Applications/WebjetTsa/Admin/Administration.ascx.cs $
/// VERSION		 : $Revision: 1 $
///
/// ===========================================================================
#endregion

#region  Namespace Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Security.Permissions;

using Microsoft.SDC.Common;
using Microsoft.SDC.Common.Diagnostics.MessageEvent;
using Microsoft.SDC.UI.PortalFramework;
using TSA.ReferenceData;
using TSA.BusinessEntities.CampaignManagement.Enumerations;
using TSA.Common;
using TSA.UIProcesses.FinderFramework;
using EventSources = TSA.Common.EventSources;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Diagnostics;
using TSA.DataServices.ReferenceData.Entities;

#endregion

namespace TSA.Applications.WebjetTsa.Admin
{
	/// <summary>
	/// Summary description for Administration.
	/// </summary>
	[PortalContent("Administration","Administration",true)]
	// You can only access this if you are logged on & belongs to an Admin
	[PrincipalPermission(SecurityAction.Demand,Role="Admin")]
	public partial  class Administration : BaseContent
	{
        private AppState _appState = AppState.GetInstance();
#region  designer controls
        protected System.Web.UI.WebControls.LinkButton lnkPrintPendingBookings;
        protected System.Web.UI.WebControls.ImageButton imgPrintPendingBookings;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkHSAAdministration;
        protected System.Web.UI.WebControls.Image imgHSAAdministration;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkMultiplePCCManager;
        protected System.Web.UI.WebControls.Image ImageMultiplePCCManager;
        protected System.Web.UI.WebControls.Image imgFareEscalationPaymentTypeReport;
        protected System.Web.UI.WebControls.Image imgFareEscalationReport;

#endregion  //designer controls

        #region DLL Import Declarations

        #endregion //Imports

        #region Enums - Class level

        #endregion

        #region Private Constants
        private string CNST_SOFTWARE_ELEMENT = "Administration";
        private string CNST_ITINERARIES_LIST = "PendingItems";//TODO rename control to be ItinerariesList
        #endregion //Constants
		
		#region Private Variables
		protected string reportingServerLocation = "";
		#endregion //Private Variables
		
		#region Constructors/Destructors/Finalizers
		public Administration()
		{

		}
		
		#endregion //Constructor/Destructor/Finalizers

		#region Public Methods and Properties

		#endregion //Public Methods and Properties

		#region Private Methods and Properties
		protected void Page_Load(object sender, System.EventArgs e)
		{
            reportingServerLocation = WebConfigurationManager.AppSettings["ReportingServer"];
			if (reportingServerLocation != null)
			{
				lnkDailyStatistics.Attributes.Add("OnClick","OpenReportWindow('stats','../Admin/BookingStatistics.aspx',150)");
				lnkCustomerSummary.Attributes.Add("OnClick","OpenReportWindow('summary','../Admin/CustomerSummary.aspx',160)");
				lnkCustomerDrillDown.Attributes.Add("OnClick","OpenReportWindow('drillDown','../Admin/CustomerDrillDown.aspx',220)");
				lnkAuditEvents.Attributes.Add("OnClick","OpenReportWindow('auditEvents','../Admin/AuditEvents.aspx',230)");
				lnkRevenue.Attributes.Add("OnClick","OpenReportWindow('revenueAnalysis','../Admin/RevenueAnalysis.aspx',150)");
				lnkPassenger.Attributes.Add("OnClick","OpenReportWindow('passengerAnalysis','../Admin/PassengerAnalysis.aspx',150)");
				lnkAdvancedBookingAnalysis.Attributes.Add("OnClick","OpenReportWindow('advancedBookings','../Admin/AdvancedBookings.aspx',150)");
				lnkAuditBookings.Attributes.Add("OnClick","OpenReportWindow('bookingAudit','../Admin/BookingAudit.aspx',150)");
                lnkFareEscalationReport.Attributes.Add("OnClick", "OpenReportWindow('FareEscalation','../Admin/FareEscalationReport.aspx',150)");

                string sReportViewFeatures="maximize=yes,status=yes,toolbar=no,menubar=yes,resizable=yes";
            //    JScriptHelper.AddOnClickOpenNewWindow(lnkPrintPendingBookings, PrintPendingBookingsUrl(), sReportViewFeatures);
            //    JScriptHelper.AddOnClickOpenNewWindow(imgPrintPendingBookings, PrintPendingBookingsUrl(), sReportViewFeatures);
            }
			else
			{
                lnkDailyStatistics.Disabled = true;
                lnkCustomerSummary.Disabled = true;
				lnkCustomerDrillDown.Disabled = true;
				lnkAuditEvents.Disabled = true;
                lnkRevenue.Disabled = true;
                lnkPassenger.Disabled = true;
				lnkAdvancedBookingAnalysis.Disabled = true;
                lnkAuditBookings.Disabled = true;

                ErrorMessageEvent.Raise(Microsoft.SDC.Common.EventSources.ReportingServicesEventSource, "The web.config file did not contain the Reporting Server name", (int)SDCType.ExceptionSeverity.Severe, CNST_SOFTWARE_ELEMENT);
			}
            //access to Management part of Admin Tools

            string managementAccess = GetRefItem(ReferenceDataGroup.AccessSetting, "AdminManagementAccess");
            if (managementAccess == null)
            {
                pnlManagement.Visible = false;
            }
            else
            {
                if (managementAccess.Length == 0)
                {
                    pnlManagement.Visible = true;
                }
                else
                {
                    pnlManagement.Visible = false;
                    if (HttpContext.Current.User.Identity.Name != null && managementAccess.IndexOf(HttpContext.Current.User.Identity.Name,StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        pnlManagement.Visible = true;
                    }
                }
            }
            if (pnlManagement.Visible)
            {
                var userEmail = HttpContext.Current.User.Identity.Name;
                if (userEmail != null)
                {
                    var menusToUsersDictionary = LoadMenusToUsersAccessDictionary();
                    SetLinksVisibility(menusToUsersDictionary, userEmail);
                }
            }
		}

        /// <summary>
        /// 
        /// TODO: refactor, shouldn't be called for each security line, should be called once.
        /// </summary>
        /// <param name="menusToUsersDictionary">E.g DailyStats-user1;User2;User3</param>
        /// <param name="userEmail"></param>
        private void SetLinksVisibility(Dictionary<string, string[]> menusToUsersDictionary, string userEmail)
        {
            if (!menusToUsersDictionary.IsNullOrEmpty())
            {
                //DailyStats
                lnkDailyStatistics.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "DailyStats", lnkDailyStatistics.Visible);
                Image2.Visible = lnkDailyStatistics.Visible;

                //AuditEvents
                lnkAuditEvents.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "AuditEvents", lnkAuditEvents.Visible);
                Image10.Visible = lnkAuditEvents.Visible;

                //AuditBookings
                lnkAuditBookings.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "AuditBookings", lnkAuditBookings.Visible);
                Image19.Visible = lnkAuditBookings.Visible;

                //TrendAnalysis
                lblTrendAnalysis.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "TrendAnalysis", lblTrendAnalysis.Visible);
                Image1.Visible = lblTrendAnalysis.Visible;
                lnkRevenue.Visible = lblTrendAnalysis.Visible; ;
                Image15.Visible = lblTrendAnalysis.Visible; ;
                lnkPassenger.Visible = lblTrendAnalysis.Visible; ;
                Image16.Visible = lblTrendAnalysis.Visible; ;
                lnkAdvancedBookingAnalysis.Visible = lblTrendAnalysis.Visible; ;
                Image17.Visible = lblTrendAnalysis.Visible; ;

                //MemberAnalysis
                lnkCustomerSummary.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "MemberAnalysis", lnkCustomerSummary.Visible);
                Image3.Visible = lnkCustomerSummary.Visible;

                //MDFData
                lnkInternationalMatrix.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "MDFData", lnkInternationalMatrix.Visible);
                Image26.Visible = lnkInternationalMatrix.Visible;

                //MDFReport
                lnkMatrixInterFareReport.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "MDFReport", lnkMatrixInterFareReport.Visible);
                Image27.Visible = lnkMatrixInterFareReport.Visible;

                //PackagesAdmin
                lnkPackageAdmin.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "PackagesAdmin", lnkPackageAdmin.Visible);
                Image28.Visible = lnkPackageAdmin.Visible;

                //FaresManager
                bool faresManagerVisible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "FaresManager", lnkSaleCampaignManagemement.Visible);
                lnkSaleCampaignManagemement.Visible = faresManagerVisible;
                lnkVirtualFareCampaignManager.Visible = faresManagerVisible;//Virtual Fare (aka House Special)
              
                lnkCouponsCampaign.Visible = faresManagerVisible;
                lnkVelocityBonusCampaignManager.Visible = faresManagerVisible;


                //InsuranceManager
                lnkRefInsuranceManager.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "InsuranceManager", lnkRefInsuranceManager.Visible);
                ImageRefInsuranceManager.Visible = lnkRefInsuranceManager.Visible;

                //FiltersManager
                lnkFlightFiltersManager.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "FiltersManager", lnkFlightFiltersManager.Visible);
                ImageFlightFiltersManager.Visible = lnkFlightFiltersManager.Visible;

  
                lnkFareEscalationJournal.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "FareEscalationJournal", lnkFareEscalationJournal.Visible);
                imgFareEscalationJournal.Visible = lnkFareEscalationJournal.Visible;


                //MultiplePCCManager
                lnkMultiplePCCManager.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "MultiplePCC", lnkMultiplePCCManager.Visible);
                ImageMultiplePCCManager.Visible = lnkMultiplePCCManager.Visible;

                //Fare Escalation report
                lnkFareEscalationReport.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "FareEscalationReport", lnkFareEscalationReport.Visible);
                imgFareEscalationReport.Visible = lnkFareEscalationReport.Visible;

                //Fare Escalation Finance AMEX report
                lnkFareEscalationPaymentTypeReport.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "FareEscalationPaymentTypeReport", lnkFareEscalationPaymentTypeReport.Visible);
                imgFareEscalationPaymentTypeReport.Visible = lnkFareEscalationPaymentTypeReport.Visible;

                //InterlineManager
                lnkInterlineManager.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "InterlineManager", lnkInterlineManager.Visible);
                imgInterlineManager.Visible = lnkMultiplePCCManager.Visible;
                if (lnkFareEscalationPaymentTypeReport.Visible)
                {
                    DateTime FromDate=DateTime.Now.AddDays(-2);
                    DateTime ToDate=DateTime.Now.AddDays(-1);
                    string strUrl = AppSettingsHelper.GetConfigurationValue("ReportingServer");
                    string options = "width=400,height=700,resizable=yes,scrollbars=yes,status=yes,menubar=no,toolbar=no,location=no,directories=no";
                    strUrl += AppSettingsHelper.GetConfigurationValue("ReportingInstance");
                    strUrl += AppSettingsHelper.GetConfigurationValue("FareEscalationFinanceAMEXReport");
                    strUrl += "&FromDate=" + FromDate.ToString("dd-MMM-yyyy") + "&ToDate=" + ToDate.ToString("dd-MMM-yyyy");
                    lnkFareEscalationPaymentTypeReport.Attributes.Add("onclick", "javascript:window.open('" + strUrl + "','FareEscalationFinanceAMEXReport','" + options + "')");
                }


				//ChangeUserStatus
				Linkbutton2.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "ChangeUserStatus", Linkbutton2.Visible);
				Image5.Visible = Linkbutton2.Visible;

                //LogonAsMember
                Linkbutton3.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "LogonAsMember", Linkbutton3.Visible);
                Image6.Visible = Linkbutton3.Visible;

                //CreditCardCheck
                lnkCreditCardCheck.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "CreditCardCheck", lnkCreditCardCheck.Visible);
                Image22.Visible = lnkCreditCardCheck.Visible;
                
                //PendingApplications
                Linkbutton4.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "PendingApplications", Linkbutton4.Visible);
                Image7.Visible = Linkbutton4.Visible;
                lnkCustomerDrillDown.Visible = Linkbutton4.Visible;
                Image9.Visible = Linkbutton4.Visible;
                Linkbutton5.Visible = Linkbutton4.Visible;
                Image18.Visible = Linkbutton4.Visible;
				//Linkbutton8.Visible = Linkbutton4.Visible;
				//Image25.Visible = Linkbutton4.Visible;
                Linkbutton6.Visible = Linkbutton4.Visible;
                Image20.Visible = Linkbutton4.Visible;
                Linkbutton9.Visible = Linkbutton4.Visible;
                Image29.Visible = Linkbutton4.Visible;

                //MobileSupport
                hlnkMobileSupport.Visible = IsMenuNameVisible(menusToUsersDictionary, userEmail, "MobileSupport", hlnkMobileSupport.Visible);
                Image30.Visible = hlnkMobileSupport.Visible;
            }
        }

	    /// <summary>
	    /// TODO: refactor, sMenuName is not required, if refItems is for single row ,e.g.HSAAdministration,;mfreidgeim;aorlov;
	    /// </summary>
	    /// <param name="menusToUsersDictionary"></param>
	    /// <param name="userEmail"></param>
	    /// <param name="sMenuName"></param>
	    /// <param name="bVisibleByDefault"></param>
	    /// <returns></returns>
	    public static bool IsMenuNameVisible(Dictionary<string, string[]> menusToUsersDictionary, string userEmail, string sMenuName, bool bVisibleByDefault)
        {
            return AdminUIUtilities.IsMenuNameVisible(menusToUsersDictionary, userEmail, sMenuName, bVisibleByDefault);
        }
		protected void Page_PreRender(object sender, System.EventArgs e)
		{
			// Set the help topic

			bool CrossSiteRequest = false;

			// Check for cross site request
			String[] keys = Request.QueryString.AllKeys; 
			for (int arg = 0; arg < keys.Length; arg++) 
			{
				string[] values = Request.QueryString.GetValues(keys[arg]);
				if (keys[arg].ToUpper() == "CROSSSITE") 
				{
					CrossSiteRequest = true;
				}
			}
			
			// Navigate to the pending bookings for cross site
			if(CrossSiteRequest)
			{
				System.Web.UI.WebControls.LinkButton lbCrossSite = new System.Web.UI.WebControls.LinkButton();
				System.EventArgs ea = new EventArgs();

				lbCrossSite.Text = "CrossSite";

				this.Linkbutton4_Click(lbCrossSite,ea);
                this.Linkbutton8_Click(lbCrossSite,ea);

			}

		}
        private string GetRefItem(string groupCode, string itemCode)
        {
            string resp = "";
            try
            {
                CodeTranslationService cts = CodeTranslationService.CTS;
                resp = cts.GetItemName(groupCode, itemCode, false);
                return resp;
            }
            catch (Exception exc)
            {
                LoggerHelper.LogException(exc, TsaLoggingConfig.WebjetTSACategorySource, TraceEventType.Warning);
                return "";
            }
        }
        private Dictionary<string, string[]> LoadMenusToUsersAccessDictionary()
        {
            try
            {
                CodeTranslationService cts = CodeTranslationService.CTS;
                return AdminUIUtilities.BuildMenusToUsersAccessDictionary(cts);
            }
            catch (Exception exc)
            {
                LoggerHelper.LogException(exc, TsaLoggingConfig.WebjetTSACategorySource, TraceEventType.Warning);
                return null;
            }
        }


	    #endregion // Private Methods and Properties

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.Load += new System.EventHandler(this.Page_Load);
            this.PreRender += new System.EventHandler(this.Page_PreRender);
 		}

        protected void lnkVirtualFareCampaignManager_Click(object sender, EventArgs e)
		{
			PageController.MoveTo("CampaignManager");
		}

		protected void lnkSaleCampaignManagemement_Click(object sender, EventArgs e)
		{
			PageController.MoveTo("SaleFareCampaignManager");
		}

        protected void lnkFlyBuysBonusCampaignManager_Click(object sender, EventArgs e)
        {
            PageController.MoveTo("FlyBuysBonusCampaignManager"); 
        }

        protected void lnkVelocityBonusCampaignManager_Click(object sender, EventArgs e)
        {
            PageController.MoveTo("VelocityBonusCampaignManager"); 
        }

		protected void lnkCouponsCampaign_Click(object sender, EventArgs e)
		{
			PageController.MoveTo("CouponCampaignManager");
		}

		#endregion

	
		protected void Linkbutton1_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("ReferenceDataManagement");
		}

		protected void Linkbutton2_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("UserManagement");
		}

		protected void Linkbutton3_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("LogonAsCustomer");
		}

		protected void Linkbutton4_Click(object sender, System.EventArgs e)
		{
            _appState[AppState.State.SearchBookingStatus] = TSAType.ItineraryBookingStatus.Pending;
            _appState[AppState.State.AdminMenuItineraryEditViewMode] = TSAType.AdminMenuItineraryEditViewMode.ReviewPendingBookings;
            this.PageController.MoveTo(CNST_ITINERARIES_LIST);
		}

		protected void lnkCreditCardCheck_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("CreditCardCheck");
		}

		protected void btnEventsReport_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("EventsReport");
		}


		protected void btnDatabaseReviewReport_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("DatabaseReviewReport");
		}

		protected void Linkbutton5_Click(object sender, System.EventArgs e)
		{
            _appState[AppState.State.SearchBookingStatus] = TSAType.ItineraryBookingStatus.Confirmed;
            _appState[AppState.State.AdminMenuItineraryEditViewMode] = TSAType.AdminMenuItineraryEditViewMode.CancelConfirmedBookings;
            this.PageController.MoveTo(CNST_ITINERARIES_LIST);//"CancelConfirmedItems");

		}

        protected void Linkbutton6_Click(object sender, System.EventArgs e)
        {
            _appState[AppState.State.SearchBookingStatus] = TSAType.ItineraryBookingStatus.Cancelled;
            _appState[AppState.State.AdminMenuItineraryEditViewMode] = TSAType.AdminMenuItineraryEditViewMode.ShowCancelledBookings;
            this.PageController.MoveTo(CNST_ITINERARIES_LIST);
        }

		protected void Linkbutton7_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("CrossSitePendingItems");
		}

		protected void lnkPackageAdmin_Click(object sender, System.EventArgs e)
		{
			this.PageController.MoveTo("PackageAdminFront2");
		}

        protected void Linkbutton8_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("ReviewDuplicateItems");
        }
        
        protected void lnkInternationalMatrix_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("MatrixInternationalDisplay");
        }
        protected void lnkMatrixInterFareReport_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("MatrixInterFareReport");
        }
        protected void lnkRefFaresManager_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("RefFaresManager");
        }
        protected void lnkRefEmailManager_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("RefEmailManager");
        }
        protected void lnkRefInsuranceManager_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("RefInsuranceManager");
        }
        protected void lnkFlightFiltersManager_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("FlightFiltersManager");
        }
  
        protected void lnkMultiplePCCManager_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("MultiplePCCManager");
        }
        protected void lnkInterlineManager_Click(object sender, System.EventArgs e)
        {
            this.PageController.MoveTo("InterlineManager");
        }
        protected void lnkCarHireCMS_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/CarHireCMS/TblCarHireCompanyCountries_admin.aspx");
        }

        protected void Linkbutton9_Click(object sender, System.EventArgs e)
        {
            _appState[AppState.State.SearchBookingStatus] = TSAType.ItineraryBookingStatus.Confirmed;
            _appState[AppState.State.AdminMenuItineraryEditViewMode] = TSAType.AdminMenuItineraryEditViewMode.FareEscalationManualBookings;
            this.PageController.MoveTo(CNST_ITINERARIES_LIST);//"CancelConfirmedItems");
        }

        
        protected void SelectCustomer(object source, System.EventArgs e)
        {
            string emailAddress = Request.Form["__EVENTARGUMENT"];
            string sRet = LogonAsCustomer.LogonAsCustomerRedirect(this.Page, emailAddress);
            if (!String.IsNullOrEmpty(sRet))
            {
                LoggerHelper.LogEvent(sRet, TsaLoggingConfig.WebjetTSACategorySource, TraceEventType.Warning);
            }
            return;

        }

 
        protected string PrintPendingBookingsUrl()
        {
            reportingServerLocation = WebConfigurationManager.AppSettings["ReportingServer"];
            string url = WebConfigurationManager.AppSettings["ReportingServer"];
            string instanceName = WebConfigurationManager.AppSettings["ReportingInstance"];
			string reportName = "PendingItinerariesList";
            url += instanceName + reportName;
            return url;
        }

        protected void lnkFareEscalationJournal_Click(object sender, EventArgs e)
        {
            this.PageController.MoveTo("FareEscalationJournal");
        }
        protected void imgFareEscalationJournal_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            lnkFareEscalationJournal_Click(sender, e);
        }

	}
}
