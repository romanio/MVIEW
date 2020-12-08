﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;

namespace mview
{
    public class MainFormModel
    {
        private readonly ProjectManager pm = new ProjectManager();

        public void UpdateActiveProject()
        {
            pm.UpdateActiveProject();
        }

        public void OpenNewModel()
        {
            pm.OpenECLProject();
        }

        public List<string> GetProjectNames()
        {
            return
                (from item in pm.projectList
                 select item.name).ToList();
        }

        public List<int> GetSelectedProjectIndex()
        {
            var indices = new List<int>();

            for (int it = 0; it < pm.projectList.Count; ++it)
                if (pm.projectList[it].selected) indices.Add(it);

            return indices;
        }

        public void SetSelectedProjectIndex(List<int> selectedIndices)
        {
            for (int it = 0; it < selectedIndices.Count; ++it)
                pm.projectList[selectedIndices[it]].selected = true;

            // Первый в списке назначается активным

            if (selectedIndices.Count > 0)
                pm.SetActiveProject(selectedIndices[0]);
        }


        public ProjectManager GetProjectManager()
        {
            return pm;
        }

        public string[] GetNamesFromVGroup(string selected_pad)
        {
            var wells = (from item in pm.virtualGroup
                         where item.pad == selected_pad
                         select item.wellname).ToArray();

            return wells.ToArray();
        }

        public string[] GetVirtualGroups()
        {
            if (pm.virtualGroup != null)
            {
                var pads = (from item in pm.virtualGroup
                            select item.pad).Distinct().ToArray();
                return pads;
            }
            return null;
        }

        public string GetActiveProjectName()
        {
            return pm.projectList[pm.activeProjectIndex].name;
        }

        public void ExportToExcel()
        {
            // save to excel

            var XL = new Microsoft.Office.Interop.Excel.Application
            {
                Visible = false,
                Interactive = false,
                ScreenUpdating = false,
                SheetsInNewWorkbook = 1
            };

            Workbook wb = XL.Workbooks.Add();
            Worksheet ws = XL.Worksheets[1];
            ws.Name = "DATA";

            var range = ws.get_Range(
                ((Range)ws.Cells[2, 1]).Address,
                ((Range)ws.Cells[2 + pm.activeProject.SUMMARY.KEYWORDS.Length - 1, 4]).Address);

            object[,] keys = new object[pm.activeProject.SUMMARY.KEYWORDS.Length, 4];

            for (int iw = 0; iw < pm.activeProject.SUMMARY.KEYWORDS.Length; ++iw)
            {
                keys[iw, 0] = pm.activeProject.SUMMARY.KEYWORDS[iw];
                keys[iw, 1] = pm.activeProject.SUMMARY.WGUNITS[iw];
                keys[iw, 2] = pm.activeProject.SUMMARY.WGNAMES[iw];
                keys[iw, 3] = pm.activeProject.SUMMARY.NUMS[iw];
            }
            range.Value = keys;

            range = ws.get_Range(
                ((Range)ws.Cells[2, 5]).Address,
                ((Range)ws.Cells[2 + pm.activeProject.SUMMARY.KEYWORDS.Length - 1, 5 + pm.activeProject.SUMMARY.NTIME - 1]).Address);

            object[,] data = new object[pm.activeProject.SUMMARY.KEYWORDS.Length, pm.activeProject.SUMMARY.NTIME];

            for (int it = 0; it < pm.activeProject.SUMMARY.NTIME; ++it)
            {
                for (int iw = 0; iw < pm.activeProject.SUMMARY.KEYWORDS.Length; ++iw)
                {
                    data[iw, it] = pm.activeProject.SUMMARY.DATA[it][iw];
                }
            }

            range.Value = data;
            XL.Visible = true;
            XL.Interactive = true;
            XL.ScreenUpdating = true;
        }

        public EclipseProject GetActiveProject()
        {
            return pm.activeProject;
        }

        public string[] GetAllKeywords()
        {
            return pm.activeProject.SUMMARY.KEYWORDS.Distinct().ToArray();
        }

        public string[] GetNamesByType(NameOptions type)
        {
            return
                (from item in pm.activeProject.VECTORS
                 where item.Type == type
                 select item.Name).ToArray();
        }
    }
}