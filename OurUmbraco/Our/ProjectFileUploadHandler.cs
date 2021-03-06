﻿using System;
using System.Collections.Generic;
using System.Web;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;

namespace OurUmbraco.Our {
    public class ProjectFileUploadHandler : IHttpHandler {

        #region IHttpHandler Members

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {
            
            //TODO: Authorize this request!!!

            HttpPostedFile file = context.Request.Files["Filedata"];
            string userguid = context.Request.Form["USERGUID"];
            string nodeguid = context.Request.Form["NODEGUID"];
            string fileType = context.Request.Form["FILETYPE"];
            string fileName = context.Request.Form["FILENAME"];
            string umbraoVersion = context.Request.Form["UMBRACOVERSION"];
            string dotNetVersion = context.Request.Form["DOTNETVERSION"];

            List<UmbracoVersion> v = new List<UmbracoVersion>() { UmbracoVersion.DefaultVersion() };

            if (!string.IsNullOrEmpty(umbraoVersion))
            {
                v.Clear();
                v = WikiFile.GetVersionsFromString(umbraoVersion);
            }

            if (!string.IsNullOrEmpty(userguid) && !string.IsNullOrEmpty(nodeguid) && !string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(fileName)) {

                Document d = new Document( Document.GetContentFromVersion(new Guid(nodeguid)).Id );
                Member mem = new Member(new Guid(userguid));

                if (d.ContentType.Alias == "Project" && d.getProperty("owner") != null && (d.getProperty("owner").Value.ToString() == mem.Id.ToString() ||  Utils.IsProjectContributor(mem.Id,d.Id))) {
                    WikiFile.Create(fileName, new Guid(nodeguid), new Guid(userguid), file, fileType, v, dotNetVersion);

                    //the package publish handler will make sure we got the right versions info on the package node itself.
                    //ProjectsEnsureGuid.cs is the handler
                    if (fileType.ToLower() == "package")
                    {
                        d.Publish(new umbraco.BusinessLogic.User(0));
                        umbraco.library.UpdateDocumentCache(d.Id);
                    }
                } else {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 0, "wrong type or not a owner");
                }
            
            }
        }

        #endregion
    }
}
