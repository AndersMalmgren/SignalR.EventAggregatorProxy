using System.Web;
using System.Web.Optimization;

namespace SignalR.EventAggregatorProxy.Demo.MVC4
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/api").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.signalR-{version}.js",
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/angular.js"));

            bundles.Add(new ScriptBundle("~/bundles/eventAggregator").Include(
                "~/Scripts/jquery.signalR.eventAggregator-{version}.js",
                "~/Scripts/jquery.signalR.eventAggregator.angular-{version}.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/app").IncludeDirectory(
                "~/Scripts/app", "*.js"));

        }
    }
}