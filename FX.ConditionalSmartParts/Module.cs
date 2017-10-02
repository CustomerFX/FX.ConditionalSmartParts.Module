﻿using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using Sage.Platform.Application;
using Sage.Platform.Application.UI;
using Sage.Platform.Orm.Interfaces;
using Sage.Platform.WebPortal.Workspaces;
using Sage.Platform.WebPortal.Workspaces.Tab;
using FX.ConditionalSmartParts.Utility;
using FX.ConditionalSmartParts.Configuration;

namespace FX.ConditionalSmartParts
{
    public class Module : IModule
    {
        static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string _CONFIGFILE = "FX.ConditionalSmartParts.json";

        [ServiceDependency]
        public IEntityContextService EntityContext { get; set; }

        [ServiceDependency(Type = typeof(WorkItem))]
        public UIWorkItem PageWorkItem { get; set; }

        private MainContentWorkspace _mainworkspace = null;
        private MainContentWorkspace MainContentWorkspace => _mainworkspace ?? (_mainworkspace = PageWorkItem.Workspaces["MainContent"] as MainContentWorkspace);

        private TabWorkspace _tabworkspace = null;
        private TabWorkspace TabWorkspace => _tabworkspace ?? (_tabworkspace = PageWorkItem.Workspaces["TabControl"] as TabWorkspace);

        private TaskPaneWorkspace _taskworkspace = null;
        private TaskPaneWorkspace TaskPaneWorkspace => _taskworkspace ?? (_taskworkspace = PageWorkItem.Workspaces["TaskPane"] as TaskPaneWorkspace);

        public void Load()
        {
            try
            {
                if (!EntityContext.HasEntityContext) return;

                if (Configuration == null || !Configuration.Active || Configuration.ConfigEntities.Count == 0) return;
                
                foreach (var entityConfig in Configuration.ConfigEntities.Where(x => x.Entity.Equals(EntityName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var entityValue = GetEntityValue(entityConfig.EntityProperty);
                    if (entityValue == null) return;

                    foreach (var valueConfig in entityConfig.ConfigValues)
                    {
                        var valueMatch = valueConfig.Value.ToString().Equals(entityValue.ToString(), StringComparison.CurrentCultureIgnoreCase);
                        foreach (var smartPart in valueConfig.SmartParts)
                        {
                            SetSmartPartVisibility(MainContentWorkspace, smartPart, valueMatch);
                            SetSmartPartVisibility(TabWorkspace, smartPart, valueMatch);
                            SetSmartPartVisibility(TaskPaneWorkspace, smartPart, valueMatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private void SetSmartPartVisibility(IWorkspace workspace, string smartPartId, bool show)
        {
            if (workspace is TabWorkspace)
            {
                ((TabWorkspace)workspace).Hide(smartPartId, !show);
            }
            else
            {
                foreach (var smartPart in workspace.SmartParts.Cast<UserControl>().Where(smartPart => smartPart.ID == smartPartId))
                {
                    if (show)
                        workspace.Show(smartPart);
                    else
                        workspace.Hide(smartPart);
                }
            }
        }

        private object GetEntityValue(string property, IDynamicEntity entity = null)
        {
            if (entity == null)
                entity = EntityContext.GetEntity(false) as IDynamicEntity;

            if (entity == null)
                return null;

            var propertyParts = property.Split('.');
            if (propertyParts.Length == 1)
                return entity[propertyParts[0]];

            entity = entity[propertyParts[0]] as IDynamicEntity;
            var newProperties = new string[propertyParts.Length - 1];
            Array.Copy(propertyParts, 1, newProperties, 0, propertyParts.Length - 1);

            return entity == null ? null : GetEntityValue(string.Join(".", newProperties), entity);
        }

        private ModuleConfig _config = null;
        private ModuleConfig Configuration => _config ?? (_config = File.Exists(EntityConfigFile) ? File.ReadAllText(EntityConfigFile).FromJson<ModuleConfig>() : null);

        private string EntityConfigFile => Path.Combine(HttpRuntime.AppDomainAppPath, _CONFIGFILE);

        private string EntityName => EntityContext.EntityType.ToString().Split('.').ToList().Last().Substring(1);
    }
}
