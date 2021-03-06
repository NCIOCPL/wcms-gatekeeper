﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace GateKeeperTest.localhost {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="GateKeeperSoap", Namespace="http://www.cancer.gov/webservices/")]
    public partial class GateKeeper : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback RequestOperationCompleted;
        
        private System.Threading.SendOrPostCallback RequestStatusOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public GateKeeper() {
            this.Url = global::GateKeeperTest.Properties.Settings.Default.GateKeeperTest_localhost_GateKeeper;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event RequestCompletedEventHandler RequestCompleted;
        
        /// <remarks/>
        public event RequestStatusCompletedEventHandler RequestStatusCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.cancer.gov/webservices/Request", RequestNamespace="http://www.cancer.gov/webservices/", ResponseNamespace="http://www.cancer.gov/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode Request(string source, string requestID, System.Xml.XmlNode message) {
            object[] results = this.Invoke("Request", new object[] {
                        source,
                        requestID,
                        message});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public void RequestAsync(string source, string requestID, System.Xml.XmlNode message) {
            this.RequestAsync(source, requestID, message, null);
        }
        
        /// <remarks/>
        public void RequestAsync(string source, string requestID, System.Xml.XmlNode message, object userState) {
            if ((this.RequestOperationCompleted == null)) {
                this.RequestOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRequestOperationCompleted);
            }
            this.InvokeAsync("Request", new object[] {
                        source,
                        requestID,
                        message}, this.RequestOperationCompleted, userState);
        }
        
        private void OnRequestOperationCompleted(object arg) {
            if ((this.RequestCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RequestCompleted(this, new RequestCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.cancer.gov/webservices/RequestStatus", RequestNamespace="http://www.cancer.gov/webservices/", ResponseNamespace="http://www.cancer.gov/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode RequestStatus(string source, string requestID, string statusType) {
            object[] results = this.Invoke("RequestStatus", new object[] {
                        source,
                        requestID,
                        statusType});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        /// <remarks/>
        public void RequestStatusAsync(string source, string requestID, string statusType) {
            this.RequestStatusAsync(source, requestID, statusType, null);
        }
        
        /// <remarks/>
        public void RequestStatusAsync(string source, string requestID, string statusType, object userState) {
            if ((this.RequestStatusOperationCompleted == null)) {
                this.RequestStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRequestStatusOperationCompleted);
            }
            this.InvokeAsync("RequestStatus", new object[] {
                        source,
                        requestID,
                        statusType}, this.RequestStatusOperationCompleted, userState);
        }
        
        private void OnRequestStatusOperationCompleted(object arg) {
            if ((this.RequestStatusCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.RequestStatusCompleted(this, new RequestStatusCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void RequestCompletedEventHandler(object sender, RequestCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RequestCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RequestCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Xml.XmlNode Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Xml.XmlNode)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    public delegate void RequestStatusCompletedEventHandler(object sender, RequestStatusCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1099.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class RequestStatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal RequestStatusCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Xml.XmlNode Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Xml.XmlNode)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591