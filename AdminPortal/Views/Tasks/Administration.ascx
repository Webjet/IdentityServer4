<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Administration.ascx.cs" Inherits="TSA.Applications.WebjetTsa.Admin.Administration" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Import Namespace="TSA.ServiceAgents.FlyBuys" %>

<meta content="True" name="vs_showGrid">
<table width="100%" cellSspacing="0" cellpadding="0" border="0">
  <tr>
    <td width="10px"><IMG height="0" src="Images/px.gif" width="10px"></td>
    <td width="100%" valign="top">
        <FONT face="Arial" size="4"><SPAN></SPAN></FONT>

<%--<TABLE id="Table1" cellSpacing="1" cellPadding="1" width="100%" border="0">
	<TR>
		<TD class="Webjet_Form_CategoryDivider" ></TD>
	</TR>
</TABLE>--%>
<TABLE id="Table4" style="HEIGHT: 104px" cellSpacing="4" cellPadding="3" width="100%" border="0">
    <tr>
	    <td colspan="5" class="Webjet_Form_CategoryHeader"><asp:label id="Label2"  runat="server"> SERVICE CENTRE ACTIVITIES</asp:label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        </td>
    </tr>
	<TR>

		<TD style="WIDTH: 39px; HEIGHT: 19px" align="center"><asp:image id="Image6" runat="server" ImageUrl="../Images/Administration/Logon1.gif"></asp:image></TD>
		<TD style="WIDTH: 232px; HEIGHT: 19px">
            <%--<asp:linkbutton id="Linkbutton3" runat="server" CommandArgument="LogonAsCustomer" CommandName="Navigate">Logon as a Member</asp:linkbutton>--%>
            <a id="Linkbutton3" runat="server" onserverclick="Linkbutton3_Click">Logon as a Member</a>
		</TD>
		<TD style="WIDTH: 26px; HEIGHT: 19px"></TD>
		<TD style="WIDTH: 26px; HEIGHT: 6px" align="center" width="39" colSpan="1" rowSpan="1"></TD>
		<TD style="HEIGHT: 19px"></TD>
	</TR>
	
	<TR>
		<TD style="HEIGHT: 19px" align="center"><asp:image id="Image7" runat="server" ImageUrl="../Images/Administration/Review1.gif"></asp:image></TD>
		<TD style="HEIGHT: 19px">
            <%--<asp:linkbutton id="Linkbutton4" runat="server" CommandArgument="PendingItems" CommandName="Navigate">Review Pending Bookings</asp:linkbutton>--%>
            <a id="Linkbutton4" runat="server" onserverclick="Linkbutton4_Click">Review Pending Bookings</a>
		</TD>
		<TD style="HEIGHT: 19px"></TD>
		<TD align="center"><asp:image id="Image9" runat="server" ImageUrl="../Images/Administration/SingleCustomer.gif"></asp:image></TD>
		<TD style="HEIGHT: 19px">
            <%--<asp:linkbutton id="lnkCustomerDrillDown" runat="server" CommandArgument="Customer" CommandName="Navigate">Member Drill Down</asp:linkbutton>--%>
            <a id="lnkCustomerDrillDown" runat="server" href="javascript:void(0)">Member Drill Down</a>
		</TD>
	</TR>
	<TR style="DISPLAY:none">
		<TD style="HEIGHT: 19px" align="center">
			<asp:image id="Image21" runat="server" ImageUrl="../Images/Administration/Review1.gif"></asp:image></TD>
		<TD style="HEIGHT: 19px">
			<%--<asp:linkbutton id="Linkbutton7" runat="server" CommandName="Navigate" CommandArgument="PendingItems">View Cross-Site Pending Bookings</asp:linkbutton>--%>
            <a id="Linkbutton7" runat="server" onserverclick="Linkbutton7_Click">View Cross-Site Pending Bookings</a>
            &nbsp;<FONT color="#009900"></FONT></TD>
		<TD style="HEIGHT: 19px"></TD>

	</TR>
	<TR>
		<TD align="center">
			<asp:image id="Image18" runat="server" ImageUrl="../Images/Administration/Review2.gif"></asp:image></TD>
		<TD >
			<P>
				<%--<asp:linkbutton id="Linkbutton5" runat="server" CommandName="Navigate" CommandArgument="CancelConfirmedItems">Cancel Confirmed Bookings</asp:linkbutton>--%>
                <a id="Linkbutton5" runat="server" onserverclick="Linkbutton5_Click">Cancel Confirmed Bookings</a>
			</P>
		</TD>
		<TD ></TD>
		<TD  align="center"><asp:image id="Image22" runat="server" ImageUrl="../Images/Administration/cc.gif"></asp:image></TD>
		<TD style="HEIGHT: 19px">
            <%--<asp:linkbutton id="lnkCreditCardCheck" runat="server" CommandArgument="ViewCreditCardCheck" CommandName="Navigate">Credit Card Transactions To Check</asp:linkbutton>--%>
            <a id="lnkCreditCardCheck" runat="server" onserverclick="lnkCreditCardCheck_Click">Credit Card Transactions To Check</a>
		</TD>
	</TR>
	<TR>
		<TD  align="center">
			<asp:image id="Image20" runat="server" ImageUrl="../Images/Administration/Review2.gif"></asp:image></TD>
		<TD >
			<P>
				<%--<asp:linkbutton id="Linkbutton6" runat="server" CommandName="Navigate" CommandArgument="ViewCancelledItems">Show Cancelled Bookings</asp:linkbutton>--%>
                <a id="Linkbutton6" runat="server" onserverclick="Linkbutton6_Click">Show Cancelled Bookings</a>
			</P>
		</TD>
		<TD></TD>
		<TD align="center" style="width:60px"><asp:image id="Image8" runat="server" ImageUrl="../Images/Administration/iknow.png"></asp:image></TD>
		<TD style="HEIGHT: 19px">
            <a id="A1" target="_blank" href="https://webjet:webjet@webjet-iknow.inbenta.com">iKnow</a>
		</TD>

<%--		<TD style="HEIGHT: 19px" align="center"><asp:image id="Image25" runat="server" ImageUrl="../Images/Administration/Review1.gif"></asp:image></TD>
		<TD style="HEIGHT: 19px"><asp:linkbutton id="Linkbutton8" runat="server" CommandArgument="ReviewDuplicateItems" CommandName="Navigate">Review Duplicated Bookings</asp:linkbutton></TD>
--%>	</TR>
	<TR>
		<td align="center"><asp:image id="Image29" runat="server" ImageUrl="../Images/Administration/Review1.gif" />
        </td>
		<TD align="left">
			<%--<asp:linkbutton id="Linkbutton9" runat="server" CommandName="Navigate" CommandArgument="CancelConfirmedItems">Fare Escalation Manual Input</asp:linkbutton>--%>
            <a id="Linkbutton9" runat="server" onserverclick="Linkbutton9_Click">Fare Escalation Manual Input</a>
		</TD>
	</TR>
	<TR >
	
<%--		<TD style="HEIGHT: 19px" align="center">
		<asp:ImageButton id="imgPrintPendingBookings" runat="server" ImageUrl="../Images/Administration/Print.gif"  />
        </TD>
		<TD style="HEIGHT: 19px">
            <asp:LinkButton ID="lnkPrintPendingBookings" runat="server" CommandArgument="PendingItems" CommandName="Navigate" >
            Print Pending Bookings</asp:LinkButton>
            </TD>
--%>
		
		<TD align="center"><asp:image id="Image30" runat="server" 
                ImageUrl="../Images/Administration/Review1.gif"></asp:image></TD>
		<td><asp:HyperLink  ID="hlnkMobileSupport" runat="server" NavigateUrl="~/Admin/MobileSupport/MobileSupportPage.aspx">View Mobile Support Requests</asp:HyperLink></td>
		<td></td>
	</TR>

</TABLE>
        <br>
<asp:Panel ID="pnlManagement" runat="server"> 

        <TABLE id="Table12" cellSpacing="4" cellPadding="4" width="100%" border="0">
        <tr>
        	        <td Class="Webjet_Form_CategoryHeader" colspan="5">
	            <asp:label id="Label5" runat="server"> MANAGEMENT REPORTING</asp:label>
            </td>
        </tr>
			<tr>
        <td style="HEIGHT: 19px" align="center" valign="top">
            <asp:image id="Image5" runat="server" ImageUrl="../Images/Administration/Status.gif"></asp:image>
        </td>
        <td style="HEIGHT: 19px">
            <%--<asp:linkbutton id="Linkbutton2" runat="server" CommandArgument="UserManagement" CommandName="Navigate">Change User Status</asp:linkbutton>--%>
            <a id="Linkbutton2" runat="server" onserverclick="Linkbutton2_Click">Change User Status</a>
        </td>
         <TD style="height: 33px; "></TD>
		<TD  align="center"> </TD>
		<TD style=""></TD>
     </tr>
	        <TR>
		        <TD style="WIDTH: 40px;" align="center"><asp:image id="Image2" runat="server" ImageUrl="../Images/Administration/BookingStats.gif"></asp:image></TD>
		        <TD style="WIDTH: 232px">
                    <a id="lnkDailyStatistics" runat="server" href="javascript:void(0)">Daily Booking Statistics</a>
		        </TD>
		        <TD ></TD>
		        <TD style="WIDTH: 26px" align="center"><asp:image id="Image10" runat="server" ImageUrl="../Images/Administration/View.gif"></asp:image></TD>
		        <TD >
                    <a id="lnkAuditEvents" runat="server" href="javascript:void(0)">View Audit Events</a>
			        
                    <a id="btnEventsReport" runat="server" Visible="False" Enabled="False" onserverclick="btnEventsReport_Click">View Auditing Events</a>
		        </TD>
	        </TR>
	        <TR>
		        <TD  align="center">
			        <asp:image id="Image19" runat="server" ImageUrl="../Images/Administration/Stats.gif"></asp:image></TD>
		        <TD >
                    <a id="lnkAuditBookings" runat="server" href="javascript:void(0)">Audit Bookings and Payments</a>
		        </TD>
		        <TD ></TD>
		        <TD ><asp:image id="Image1" runat="server" ImageUrl="../Images/Administration/Administration.gif"></asp:image></TD>
		        <TD><asp:Label id='lblTrendAnalysis' runat="server">Bookings and Trend Analysis</asp:Label></TD>

	        </TR>
	        <TR>
		        <TD  align="center"><asp:image id="Image3" runat="server" ImageUrl="../Images/Administration/MultipleCustomers2.gif"></asp:image></TD>
		        <TD >
                    <%--<asp:linkbutton id="lnkCustomerSummary" runat="server" CommandArgument="Customer" CommandName="Navigate"> Member Analysis</asp:linkbutton>--%>
			        <a id="lnkCustomerSummary" runat="server" href="javascript:void(0)"> Member Analysis</a>

                    <%--<asp:linkbutton id="lnkBookingTrends" runat="server" CommandName="Navigate" CommandArgument="Booking"
				        Enabled="False" Visible="False">Bookings and Trend Analysis</asp:linkbutton>--%>
                    <a id="lnkBookingTrends" runat="server" Enabled="False" Visible="False">Bookings and Trend Analysis</a>
                </TD>
		        <TD  ></TD>
		        <TD  ></TD>
		        <TD  >	<asp:Image id="Image15" runat="server" ImageUrl="../Images/Administration/SubHeading.gif"></asp:Image>&nbsp;
		            <%--<asp:linkbutton id="lnkRevenue" runat="server" CommandName="Navigate" CommandArgument="Booking">Revenue Analysis</asp:linkbutton>--%>
                    <a id="lnkRevenue" runat="server" href="javascript:void(0)">Revenue Analysis</a>
		        </TD>

	        </TR>
	        <TR>
		        <TD align="center">	</TD>
		        <TD ></TD>
		        <TD  ></TD>
		        <TD   ></TD>
		        <TD  >	
		            <asp:Image id="Image16" runat="server" ImageUrl="../Images/Administration/SubHeading.gif"></asp:Image>&nbsp;
                    <a id="lnkPassenger" runat="server" href="javascript:void(0)">Passenger Analysis</a>

		        </TD>
	        </TR>
	        <TR>
	            <TD  align="center"><asp:image id="Image26" runat="server" ImageUrl="../Images/Administration/wand.gif"></asp:image></TD>
		        <TD >
                    <%--<asp:linkbutton id="lnkInternationalMatrix" runat="server" CommandArgument="InternationalMatrixDisplay" CommandName="Navigate">Inter. Matrix - Data Collection</asp:linkbutton>--%>
                    <a id="lnkInternationalMatrix" runat="server" onserverclick="lnkInternationalMatrix_Click">Inter. Matrix - Data Collection</a>
		        </TD>

		<TD  ></TD>
		<TD  ></TD>
		<TD  >	
		    <asp:Image id="Image17" runat="server" ImageUrl="../Images/Administration/SubHeading.gif"></asp:Image>&nbsp;
			<%--<asp:linkbutton id="lnkAdvancedBookingAnalysis" runat="server" CommandName="Navigate" CommandArgument="Booking">Advance Booking Analysis</asp:linkbutton>--%>
            <a id="lnkAdvancedBookingAnalysis" runat="server" href="javascript:void(0)">Advance Booking Analysis</a>
		</TD>
	</TR>
	<TR>
		<TD  align="center" >
		    <asp:image id="Image27" runat="server"  ImageUrl="../Images/Administration/inter_matrix_report.gif"></asp:image>
		 </TD>
		<TD >
		    <%--<asp:linkbutton id="lnkMatrixInterFareReport" runat="server" CommandArgument="MatrixInterFareReport" CommandName="Navigate">Inter. Matrix - Daily Fare Finder Report</asp:linkbutton>--%>
            <a id="lnkMatrixInterFareReport" runat="server" onserverclick="lnkMatrixInterFareReport_Click">Inter. Matrix - Daily Fare Finder Report</a>
		 </TD>
		 <TD  ></TD>
		 <TD><asp:image id="Image28" runat="server" ImageUrl="../Images/Administration/ViewDB.gif"></asp:image></TD>
         <TD >
             <%--<asp:linkbutton id="lnkPackageAdmin" runat="server" CommandArgument="PackageAdmin" CommandName="Navigate">Package Administration</asp:linkbutton>--%>
             <a id="lnkPackageAdmin" runat="server" onserverclick="lnkPackageAdmin_Click">Package Administration</a>
         </TD>
	</TR>
	<TR>
		<TD style="HEIGHT: 19px" align="center" valign="top">
		    <asp:image id="ImageRefFaresManager" runat="server"  ImageUrl="../Images/Administration/Fares.gif"></asp:image>
		 </TD>
		<TD >
			    <a id="lnkSaleCampaignManagemement" runat="server" Visible="true" onserverclick="lnkSaleCampaignManagemement_Click">Sale Fare Campaign Management</a>

            <br/>
			    <a id="lnkVirtualFareCampaignManager" runat="server" onserverclick="lnkVirtualFareCampaignManager_Click">Virtual Fare Campaign Management</a>
            <br/>
            <% if (FlyBuysConfig.IsEnabledFlyBuys())
               { %>
		            <a id="lnkFlyBuysBonusCampaignManager" runat="server" onserverclick="lnkFlyBuysBonusCampaignManager_Click">FlyBuys Bonus Campaign Management</a>
                    <br/>
            <% } %>
                <a id="lnkCouponsCampaign" runat="server" onserverclick="lnkCouponsCampaign_Click">Coupon Campaign Management</a>
            <br/>
                <a id="lnkVelocityBonusCampaignManager" runat="server" onserverclick="lnkVelocityBonusCampaignManager_Click">Velocity Bonus Campaign Management</a>
	 </TD>
		<TD  ></TD>
	</TR>
	<TR>
		<TD  align="center" valign="top">
		 </TD>
		<TD >
		 </TD>
		<TD style="height: 33px; "></TD>
		 <TD  align="center"><asp:image id="ImageRefInsuranceManager" runat="server"  ImageUrl="../Images/Administration/insurance.gif"></asp:image></TD>
		 <TD style="">
             <a id="lnkRefInsuranceManager" runat="server" onserverclick="lnkRefInsuranceManager_Click">Insurance Price Management</a>
		 </TD> 
    </TR>
    <TR>
		<TD style="HEIGHT: 19px" align="center" valign="top">
		    <asp:image id="ImageFlightFiltersManager" runat="server"  ImageUrl="../Images/Administration/FlightFilters.gif"></asp:image>
		 </TD>
		<TD >
            <a id="lnkFlightFiltersManager" runat="server" onserverclick="lnkFlightFiltersManager_Click">Flight/Fare Filters Management</a>
		 </TD>
		<TD  ></TD>
		 <TD  align="center">
         </TD>
		 <TD style="">
		 </TD> 
    </TR>
     <tr>
        <TD  align="center">
		    <asp:ImageButton id="imgFareEscalationJournal" runat="server" ImageUrl="../Images/Administration/Print.gif" OnClick="imgFareEscalationJournal_Click"  />
        </TD>
        		<TD >
            <a ID="lnkFareEscalationJournal" runat="server" onserverclick="lnkFareEscalationJournal_Click" >Fare Escalation Journal</a>
            </TD>
        <td ></td>

    </TR>
    <TR>
		<TD  align="center"> <asp:image id="imgFareEscalationReport" runat="server"  ImageUrl="../Images/Administration/BookingStats.gif"></asp:image></TD>
		<TD style="">
            <a id="lnkFareEscalationReport" runat="server" href="javascript:void(0)">Fare Escalation Report</a>
        </TD>
    </TR>
     <TR>
		<TD style="HEIGHT: 19px" align="center" valign="top">
		    <asp:image id="ImageMultiplePCCManager" runat="server"  ImageUrl="../Images/Administration/MPCC.gif"></asp:image>
	        
        </TD>
        <TD style="HEIGHT: 19px">
	        <a id="lnkMultiplePCCManager" runat="server" onserverclick="lnkMultiplePCCManager_Click">Multiple PCCs Manager</a>
        </TD>
        <TD style="height: 33px; "></TD>
		<TD  align="center"> <asp:image id="imgFareEscalationPaymentTypeReport" runat="server"  ImageUrl="../Images/Administration/BookingStats.gif"></asp:image></TD>
		<TD style="">
            <a id="lnkFareEscalationPaymentTypeReport" runat="server" href="javascript:void(0)">Fare Esc. Finance AMEX Report</a>
        </TD>
    </TR>
    <tr>

		<TD  align="center"> <asp:image id="imgInterlineManager" runat="server"  ImageUrl="../Images/Administration/FlightFilters.gif"></asp:image></TD>
		<TD style="">
            <%--<asp:linkbutton id="lnkInterlineManager" runat="server"  CommandName="Navigate">Interline Manager</asp:linkbutton>--%>
            <a id="lnkInterlineManager" runat="server" onserverclick="lnkInterlineManager_Click">Interline Manager</a>
    </tr>

</TABLE>


<br />
<%--<TABLE id="Table3" cellSpacing="1" cellPadding="1" width="100%" border="0">
	<TR>
		<TD class="Webjet_Form_CategoryDivider" id="Td1"></TD>
	</TR>
</TABLE>--%>
<TABLE id="Table5" cellSpacing="4" cellPadding="4" width="100%" border="0">
<tr>
<td class="Webjet_Form_CategoryHeader" colspan="5">
    <asp:label id="Label3"  runat="server"> APPLICATION DIAGNOSTICS</asp:label>
</td>
</tr>
	<TR>
		<TD style="WIDTH: 40px; HEIGHT: 10px" align="center"><asp:image id="Image12" runat="server" ImageUrl="../Images/Administration/ViewTable.gif" Width="27px"></asp:image></TD>
		<TD style="WIDTH: 33px; HEIGHT: 10px" align="center"><asp:image id="Image4" runat="server" ImageUrl="../Images/Administration/Data.gif"></asp:image></TD>
		<TD  >
            <%--<asp:linkbutton id="Linkbutton1" runat="server" CommandArgument="ReferenceDataManagement" CommandName="Navigate">View Reference Data</asp:linkbutton>--%>
            <a id="Linkbutton1" runat="server" onserverclick="Linkbutton1_Click">View Reference Data</a>
		</TD>
	</TR>

</TABLE>
<%-- To LogonAsCustomer from DrillDown --%>
<%--<asp:LinkButton runat="server"  Text='DynamicEMailAddress' OnClick="SelectCustomer" CommandArgument='DynamicCommandArgument' ID="lnkLogonAsCustomer" style="DISPLAY: none" >
</asp:LinkButton>--%>
<a runat="server" onserverclick="SelectCustomer" ID="lnkLogonAsCustomer" style="DISPLAY: none">DynamicEMailAddress</a>

</asp:Panel>
</td>
<script language="javascript">
function OpenReportWindow(name,url,height) 
{
	newwin = window.open('Admin/ReportFrame.aspx?height='+height+'&capture='+url,'_blank','maximize=yes,status=yes,toolbar=no,menubar=no,resizable=yes');
	newwin.moveTo(0,0);newwin.resizeTo(screen.width, screen.height);
}
</script>
	<script language="javascript" >

// To LogonAsCustomer from DrillDown 
	function LogonAsCustomer(emailAddress)
	 {
	 //TODO Try to use ServerSide GetPostBackEventReference to avioid hard-coded UniqueID/UniqueIDWithDollars   
//var lnk= MM_findObj("ContentView_PageLayout_Administration_lnkLogonAsCustomer");
//lnk.innerText=emailAddress;
var clientIDWithDollars="ContentView$PageLayout$Administration$lnkLogonAsCustomer";
//	 debugger;
	 __doPostBack(clientIDWithDollars,emailAddress)
//lnk.click();
}

	</script>
