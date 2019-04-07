﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.BackgroundPages.Core;
using SiteServer.BackgroundPages.Utils;
using SiteServer.CMS.Caches;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.Database.Attributes;
using SiteServer.CMS.Database.Core;
using SiteServer.CMS.Fx;
using SiteServer.CMS.Plugin;
using SiteServer.Plugin;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalSelectColumns : BasePageCms
    {
        protected override bool IsSinglePage => true;

        protected CheckBoxList CblDisplayAttributes;

        private int _channelId;
        private Dictionary<string, Dictionary<string, Func<IContentContext, string>>> _pluginColumns;

        public static string GetOpenWindowString(int siteId, int channelId)
        {
            return LayerUtils.GetOpenScript("设置显示项", FxUtils.GetCmsUrl(siteId, nameof(ModalSelectColumns), new NameValueCollection
            {
                {"channelId", channelId.ToString()}
            }));
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            WebPageUtils.CheckRequestParameter("siteId");

            _channelId = AuthRequest.GetQueryInt("channelId");

            var channelInfo = ChannelManager.GetChannelInfo(SiteId, _channelId);

            var attributesOfDisplay = TranslateUtils.StringCollectionToStringCollection(channelInfo.ContentAttributesOfDisplay);
            var pluginIds = PluginContentManager.GetContentPluginIds(channelInfo);
            _pluginColumns = PluginContentManager.GetContentColumns(pluginIds);

            if (IsPostBack) return;

            var styleInfoList = ContentUtility.GetAllTableStyleInfoList(TableStyleManager.GetContentStyleInfoList(SiteInfo, channelInfo));
            foreach (var styleInfo in styleInfoList)
            {
                if (styleInfo.Type == InputType.TextEditor) continue;
                
                var listitem = new ListItem($"{styleInfo.DisplayName}({styleInfo.AttributeName})", styleInfo.AttributeName);
                if (styleInfo.AttributeName == ContentAttribute.Title)
                {
                    listitem.Selected = true;
                }
                else
                {
                    if (attributesOfDisplay.Contains(styleInfo.AttributeName))
                    {
                        listitem.Selected = true;
                    }
                }

                CblDisplayAttributes.Items.Add(listitem);
            }

            if (_pluginColumns != null)
            {
                foreach (var pluginId in _pluginColumns.Keys)
                {
                    var contentColumns = _pluginColumns[pluginId];
                    if (contentColumns == null || contentColumns.Count == 0) continue;

                    foreach (var columnName in contentColumns.Keys)
                    {
                        var attributeName = $"{pluginId}:{columnName}";
                        var listitem = new ListItem($"{columnName}({pluginId})", attributeName);
                        if (attributesOfDisplay.Contains(attributeName))
                        {
                            listitem.Selected = true;
                        }

                        CblDisplayAttributes.Items.Add(listitem);
                    }
                }
            }
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var channelInfo = ChannelManager.GetChannelInfo(SiteId, _channelId);

            var attributesOfDisplay = SystemWebUtils.SelectedItemsValueToStringCollection(CblDisplayAttributes.Items);
            channelInfo.ContentAttributesOfDisplay = attributesOfDisplay;

            DataProvider.Channel.Update(channelInfo);

            AuthRequest.AddSiteLog(SiteId, "设置内容显示项", $"显示项:{attributesOfDisplay}");

            LayerUtils.Close(Page);
        }

    }
}