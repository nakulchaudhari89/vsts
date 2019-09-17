using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using VSTS.Models;

namespace VSTS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            SqlConnection con = new SqlConnection("ConnectionString");
            con.Open();

            #region Azure DevOps data connection

            Uri orgUrl = new Uri("VstsUrl");
            String personalAccessToken = "PAT";
            // Create a connection
            VssConnection connection = new VssConnection(orgUrl, new VssBasicCredential(string.Empty, personalAccessToken));

            #endregion

            AddOrUpdateApplicationDetail(connection, con).Wait();
            con.Close();
            return View();
        }

        static private async Task AddOrUpdateApplicationDetail(VssConnection connection, SqlConnection con)
        {
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                Wiql wiql = new Wiql();

                wiql.Query = "SELECT QUERY";                   

                WorkItemQueryResult tasks = await witClient.QueryByWiqlAsync(wiql);

                IEnumerable<WorkItemReference> tasksRefs;
                tasksRefs = tasks.WorkItems.OrderBy(x => x.Id);
                List<WorkItem> tasksList = witClient.GetWorkItemsAsync(tasksRefs.Select(wir => wir.Id)).Result;
            }
            catch (AggregateException aex)
            {
                VssServiceException vssex = aex.InnerException as VssServiceException;
                if (vssex != null)
                {
                    Console.WriteLine(vssex.Message);
                }
            }
        }
    }
}
