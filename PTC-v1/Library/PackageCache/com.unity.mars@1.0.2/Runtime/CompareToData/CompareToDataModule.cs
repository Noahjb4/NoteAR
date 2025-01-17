﻿using System;
using System.Collections.Generic;
using Unity.MARS.Query;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Compares data chosen in the simulation view to conditions and allows conforming the conditions to match.
    /// </summary>
    public class CompareToDataModule :
        IModuleDependency<DataVisualsModule>,
        IModuleDependency<QueryPipelinesModule>
    {
        const int k_MaxSnapshots = 4;

        bool m_Comparing;
        TraitDataSnapshot m_HoverDataSnapshot = new TraitDataSnapshot();
        List<TraitDataSnapshot> m_SelectionDataSnapshots = new List<TraitDataSnapshot>();
        Proxy m_ProxyToCompare;
        Proxy m_ProxyToModify;
        bool m_IsHovering;

        QueryPipelinesModule m_QueryPipelinesModule;
        DataVisualsModule m_DataVisualsModule;

        /// <summary>
        /// Called when the data being compared has changed
        /// </summary>
        public event Action CompareDataChanged;

        /// <summary>
        /// Get whether the module is currently comparing data
        /// </summary>
        public bool IsComparing => m_Comparing;

        /// <summary>
        /// The proxy that is being compared to data currently
        /// </summary>
        public Proxy ProxyToCompare => m_ProxyToCompare;

        public void ConnectDependency(DataVisualsModule dependency)
        {
            m_DataVisualsModule = dependency;
        }

        public void ConnectDependency(QueryPipelinesModule dependency)
        {
            m_QueryPipelinesModule = dependency;
        }

        public void LoadModule() { }

        public void UnloadModule() { }

        /// <summary>
        /// Get the current data to be used for comparison.
        /// </summary>
        /// <param name="selectionIndex">The index into the current selection if there is multiple objects selected. Defaults to 0</param>
        /// <returns>The selected or hovered data</returns>
        public TraitDataSnapshot GetComparisonData(int selectionIndex = 0)
        {
            if (selectionIndex == 0 && m_HoverDataSnapshot != null && m_IsHovering)
                return m_HoverDataSnapshot;

            if (selectionIndex < m_SelectionDataSnapshots.Count)
                return m_SelectionDataSnapshots[selectionIndex];

            return null;
        }

        /// <summary>
        /// Change the data proxy that is being compared against the main proxy. The current match data for each proxy will be captured.
        /// Selected data will be used for comparison if there is no data being hovered.
        /// </summary>
        /// <param name="proxies"> A list of all the data proxies that are currently selected</param>
        public void OnDataSelectionChanged(List<Proxy> proxies)
        {
            m_SelectionDataSnapshots.Clear();
            var count = Mathf.Min(k_MaxSnapshots, proxies.Count);
            if (count >= m_SelectionDataSnapshots.Count)
                m_SelectionDataSnapshots.Fill(count - m_SelectionDataSnapshots.Count);

            for (var i = 0; i < count; i++)
            {
                if (m_SelectionDataSnapshots[i] == null)
                    m_SelectionDataSnapshots[i] = new TraitDataSnapshot();

                UpdateComparisonData(proxies[i], m_SelectionDataSnapshots[i]);
            }
        }

        /// <summary>
        /// End the current hover. After calling this, the comparison data will use the data set via OnDataSelectionChanged
        /// </summary>
        public void OnHoverEnd()
        {
            m_IsHovering = false;
            UpdateComparisonData(null, m_HoverDataSnapshot);
        }

        /// <summary>
        /// Start or change the data that is currently being hovered to compare against the main proxy.
        /// </summary>
        /// <param name="proxy">A proxy that is matched to the data to compare</param>
        public void OnHoverChange(Proxy proxy)
        {
            m_IsHovering = false;

            if (proxy == null)
            {
                UpdateComparisonData(null, m_HoverDataSnapshot);
            }
            else
            {
                m_IsHovering = true;
                UpdateComparisonData(proxy, m_HoverDataSnapshot);
            }
        }

        void UpdateComparisonData(Proxy dataSourceProxy, TraitDataSnapshot dataSnapshot)
        {
            var queryResult = dataSourceProxy == null ? null : dataSourceProxy.currentData;
            dataSnapshot.TakeSnapshot(queryResult);

            if (CompareDataChanged != null)
                CompareDataChanged();
        }

        /// <summary>
        /// For each condition on the proxy set via SetProxyToModify, try to include the current comparison data.
        /// Only enabled conditions that implement ICreateFromData will be modified.
        /// </summary>
        public void IncludeAll()
        {
            foreach (var condition in m_ProxyToModify.GetComponents<Condition>())
            {
                var createFromData = condition as ICreateFromData;

                if (createFromData == null || !condition.enabled)
                    continue;

                createFromData.IncludeData(GetComparisonData());
            }
        }


        /// <summary>
        /// For each condition on the proxy set via SetProxyToModify, try to optimize for the current comparison data.
        /// Only enabled conditions that implement ICreateFromData will be modified.
        /// </summary>
        public void OptimizeAll()
        {
            foreach (var condition in m_ProxyToModify.GetComponents<Condition>())
            {
                var createFromData = condition as ICreateFromData;

                if (createFromData == null || !condition.enabled)
                    continue;

                createFromData.OptimizeForData(GetComparisonData());
            }
        }

        /// <summary>
        /// Start comparing and request data visuals to show the rating for the conditions on the proxy set via SetProxyToCompare
        /// </summary>
        public void StartComparing()
        {
            m_Comparing = true;
            UpdateDataVisualsForContentProxy();
            m_DataVisualsModule.RequestDataVisuals(this, true);
        }

        /// <summary>
        /// Stop comparing and clear any data visuals showing a rating.
        /// </summary>
        public void StopComparing()
        {
            m_Comparing = false;
            m_DataVisualsModule.ClearDataRatings();
            m_DataVisualsModule.RequestDataVisuals(this, false);
        }

        /// <summary>
        /// Sets the proxy whose conditions will be compared against data. The proxy must be active and running.
        /// The proxy that will be modified via Include or Optimize actions must be set separately via SetProxyToModify
        /// </summary>
        /// <param name="proxy">An active running proxy whose conditions will be compared to data</param>
        public void SetProxyToCompare(Proxy proxy)
        {
            m_ProxyToCompare = proxy;
            if (m_Comparing)
                UpdateDataVisualsForContentProxy();
        }

        /// <summary>
        /// Sets the proxy that will be modified when compare actions "Include" and "Optimize"
        /// This can be different from the proxy set via SetProxyToCompare. For example, when simulating
        /// the proxy to compare is the active copy, but the proxy to modify is the original scene version.
        /// </summary>
        /// <param name="proxy">The proxy whose conditions will be modified. </param>
        public void SetProxyToModify(Proxy proxy)
        {
            m_ProxyToModify = proxy;
        }

        void UpdateDataVisualsForContentProxy()
        {
            if (m_ProxyToCompare != null)
            {
                ShowDataRatingsRelativeTo(m_ProxyToCompare);
            }
            else
            {
                m_DataVisualsModule.ClearDataRatings();
            }
        }

        void ShowDataRatingsRelativeTo(Proxy proxy)
        {
            m_QueryPipelinesModule.GetConditionRatingsForProxy(proxy, out var conditionRatings);
            m_DataVisualsModule.VisualizeRatingsForData(conditionRatings);
        }

        /// <summary>
        /// Get the condition rating results as a list of key value pairs. Each pair is a condition name and float rating from 0 to 1
        /// </summary>
        /// <param name="conditionRatingsResult"> The list that will be cleared and populated with the results</param>
        public void GetConditionRatings(List<KeyValuePair<string, float>> conditionRatingsResult)
        {
            conditionRatingsResult.Clear();

            if (m_ProxyToCompare == null)
                return;

            var compareData = GetComparisonData();
            if (compareData == null)
                return;

            var conditions = new List<ICreateFromData>();
            m_ProxyToCompare.GetComponents(conditions);
            foreach (var condition in conditions)
            {
                if (condition is MonoBehaviour behaviour && !behaviour.enabled)
                    continue;

                var rating = condition.GetConditionRatingForData(compareData);

                if (!(rating >= 0f))
                    continue;

                string conditionName;
                if (condition is SemanticTagCondition tagCondition)
                    conditionName = $"{tagCondition.matchRule.ToString()} {tagCondition.traitName} Tag Condition";
                else
                    conditionName = condition.GetType().Name.InsertSpacesBetweenWords();

                conditionRatingsResult.Add(new KeyValuePair<string, float>(conditionName, rating));
            }
        }
    }
}
