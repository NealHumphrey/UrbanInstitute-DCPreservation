﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="manage-users.aspx.cs" MasterPageFile="~/masters/Default.Master" Inherits="Urban.DCP.Web.admin.ManageUsers" %>
<%@ Import Namespace="Azavea.Web"%>
<%@ MasterType TypeName="Urban.DCP.Web.masters.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="contentPlaceHolder" runat="server">
     <h1 id="pdp-profile-header">User Administration</h1>
     <div id="pdp-main" class="manage">
         <div id="pdp-pdb-table-content" style="display:block;">
            <div id="pdp-admin-user-table"></div>
            <div id="pdp-user-table-pager" class="pdp-user-table-pager"></div>
        </div>

    </div>   
    
    <div id="pdp-form-dialog" title="Edit User Data">

        <h2>User Profile</h2>
        <fieldset class="pdp-form">
            <ul class="pdp-form-list">
                <li>
                    <label for="pdp-active" class="pdp-form-label">Active</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-active" type="checkbox" class="pdp-input" />
                    </div>
                    <label for="pdp-email-confirm" class="pdp-form-label">Email Confirmed</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-email-confirm" type="checkbox" class="pdp-input" />
                    </div>
                </li>
                <li>
                    <label for="pdp-username" class="pdp-form-label">Username</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-username" type="text" class="pdp-input pdp-input-text" readonly="readonly" />
                    </div>
                </li>
                <li>
                    <label for="pdp-name" class="pdp-form-label">Name</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-name" type="text" class="pdp-input pdp-input-text" tabindex="1" />
                    </div>
                </li>
                <li>
                    <label for="pdp-email" class="pdp-form-label">Email</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-email" type="text" class="pdp-input pdp-input-text" tabindex="2" />
                    </div>
                </li>
                <li>
                    <label for="pdp-affiliation" class="pdp-form-label">Affiliation</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-affiliation" type="text" class="pdp-input pdp-input-text" tabindex="3" />
                    </div>
                </li>
                <li>
                    <label for="pdp-requesting" class="pdp-form-label">Requesting Network Membership</label>
                    <div class="pdp-form-ctrl">
                        <input id="pdp-requesting" type="checkbox" disabled class="pdp-input" />
                    </div>
                </li>
                <li>
                    <label class="pdp-form-label" for="pdp-select-org">Network Org</label>
                    <div class="pdp-form-ctrl">
                         <select id="pdp-select-org" class="pdp-input" >
                            <option >test</option>
                        </select>
                     </div>
                </li>
             </ul>   
                <input id="pdp-change-password" type="checkbox" tabindex="3" class="pdp-input" />
                <label for="pdp-change-password" class="pdp-form-list-label">Change Password</label>
                <div id="pdp-password-container">
                    <ul class="pdp-form-list">
                        <li>
                            <label for="pdp-password" class="pdp-form-label">Password</label>
                            <div class="pdp-form-ctrl">
                                <input id="pdp-password" type="password" class="pdp-input pdp-input-text" tabindex="4" />
                            </div>
                        </li>
                        <li>
                            <label for="pdp-password-2" class="pdp-form-label">Confirm Password</label>
                            <div class="pdp-form-ctrl">
                                <input id="pdp-password-2" type="password" class="pdp-input pdp-input-text" tabindex="5" />
                            </div>
                        </li>
                    </ul>
                </div>
                
                <fieldset class="pdp-form">
                    <legend>User Privileges</legend>
                    <p>The Network priveledge wil be automatically addded if an organization is selected.</p>
                    <ul>
                        <li>
                            <label for="pdp-role-public" class="pdp-form-label">Public</label>
                            <div class="pdp-form-ctrl">
                                <input id="pdp-role-public" type="checkbox" value="public" class="pdp-input pdp-input-checkbox pdp-role-input" tabindex="6" />
                            </div>
                        </li>
                        <li>
                            <label for="pdp-role-limited" class="pdp-form-label">Limited</label>
                            <div class="pdp-form-ctrl">
                                <input id="pdp-role-limited" type="checkbox" value="limited" class="pdp-input pdp-input-checkbox pdp-role-input" tabindex="7" />
                            </div>
                        </li>
                        <li>
                            <label for="pdp-role-sysadmin" class="pdp-form-label">SysAdmin</label>
                            <div class="pdp-form-ctrl">
                                <input id="pdp-role-sysadmin" type="checkbox" value="SysAdmin" class="pdp-input pdp-input-checkbox pdp-role-input" tabindex="8" />
                            </div>
                        </li>
                    </ul>
                </fieldset>
	    
        </fieldset>
    </div>


    <script type="text/javascript">
        PDP.ManageUsers({ 
            target:'#content',
            tableTarget:'#pdp-admin-user-table',
            pagerTarget:'#pdp-user-table-pager',
            dialogTarget:'#pdp-form-dialog',
            appUrl: '<%=WebUtil.GetApplicationUrl(Request) %>'
        }).init();
    </script>
    
</asp:Content>