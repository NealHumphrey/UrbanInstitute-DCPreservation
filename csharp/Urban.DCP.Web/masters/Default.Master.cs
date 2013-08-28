﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Azavea.Open.Common;
using Azavea.Web;

namespace Urban.DCP.Web.masters
{
    public partial class Default : MasterBase
    {
        protected Config _config = Config.GetConfig("PDP.Web");

        protected override void OnInit(EventArgs e)
        {
#if DEBUG
            UseMinifiedFiles = false;
#else
            UseMinifiedFiles = true;
#endif
            base.OnInit(e);

            string appUrl = WebUtil.GetApplicationUrl(Request);

            RegisterCssFile("http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css");
            RegisterCssFile(appUrl + "client/pdp-core.css", true);
            RegisterJavascriptFile("http://code.jquery.com/jquery-1.9.1.js");
            RegisterJavascriptFile("http://code.jquery.com/jquery-migrate-1.1.0.js");
            RegisterJavascriptFile("http://code.jquery.com/ui/1.10.3/jquery-ui.js");

            RegisterJavascriptFile(appUrl + "client/pdp-core.js", true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerifyBrowser();
        }

        public void VerifyBrowser()
        {
            string appUrl = WebUtil.GetApplicationUrl(Request);

            IDictionary<string, string> browsers = _config.GetParametersAsDictionary("UnsupportedBrowsers");
            foreach (KeyValuePair<string, string> kvp in browsers)
            {
                if (Request.UserAgent == null || Regex.IsMatch(Request.UserAgent, kvp.Value))
                {
                    RegisterJavascriptFile(appUrl + "client/pdp-browser-warning.js", true);
                    _log.Warn("Detected unsupported browser. Regex: [" + kvp.Value + "] UserAgent: [" + Request.UserAgent + "]");
                    break;
                }
            }
        }

        public void SetTitle(string pageTitle, bool fullTitle)
        {

            if (fullTitle)
            {
                titleElement.Text = pageTitle;
            }
            else
            {
                titleElement.Text = pageTitle + " - " + titleElement.Text;
            }
        }

        public void SetTitle(string pageTitle)
        {
            SetTitle(pageTitle, false);
        }
    }
}
