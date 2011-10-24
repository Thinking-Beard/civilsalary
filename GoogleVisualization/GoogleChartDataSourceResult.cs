using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using FluentJson;
using System.Collections;

namespace GoogleVisualization
{
    public sealed class GoogleChartDataSourceResult : ActionResult
    {
        public const string Version = "0.6";

        public GoogleDataTable Data { get; set; }
        public string ReqId { get; set; }
        public string ResponseHandler { get; set; }

        //public GoogleChartDataSourceResult(QueryResult result, string tqx)
        //{
        //    var parameters = tqx.SplitDictionary(";", ":");

        //    if (parameters.ContainsKey("out") && parameters["out"] != "json") throw new InvalidOperationException("Only JSON output is supported.");

        //    ReqId = parameters["reqId"];
        //    Data = result.Table;

        //    if (parameters.ContainsKey("responseHandler")) ResponseHandler = parameters["responseHandler"];
        //}

        //public GoogleChartDataSourceResult(IQueryable data, string dataQuery, string tqx, object additionalProperties)
        //    : this(data, dataQuery, tqx, new RouteValueDictionary(additionalProperties))
        //{
        //}

        public static GoogleChartDataSourceResult Create<T>(IEnumerable<T> data, string tqx, IDictionary<string, object> additionalProperties)
        {
            return new GoogleChartDataSourceResult(data, typeof(T), tqx, additionalProperties);
        }

        public GoogleChartDataSourceResult(IEnumerable data, Type elementType, string tqx, IDictionary<string, object> additionalProperties)
        {
            var parameters = tqx.SplitDictionary(";", ":");

            ReqId = parameters != null && parameters.ContainsKey("reqId") ? parameters["reqId"] : null;

            Data = data.ToGoogleDataTable(elementType, additionalProperties);
        }

        public GoogleChartDataSourceResult(IQueryable data, string dataQuery, string tqx, IDictionary<string, object> additionalProperties)
        {
            var query = Query.Parse(dataQuery);
            var parameters = tqx.SplitDictionary(";", ":");

            ReqId = parameters != null && parameters.ContainsKey("reqId") ? parameters["reqId"] : null;

            var result = query.Execute(data, additionalProperties);

            Data = result.Table;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (this.Data == null) throw new InvalidOperationException();

            var response = context.HttpContext.Response;

            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;

            var json = JsonObject.Create()
                .AddProperty("version", Version)
                .AddProperty("reqId", ReqId)
                .AddProperty("status", "ok")
                .AddProperty("table", Data.ToJsonObject());

            var handler = "google.visualization.Query.setResponse";

            if (!string.IsNullOrWhiteSpace(ResponseHandler)) handler = ResponseHandler;

            response.Write(handler);
            response.Write("(");
            response.Write(json.ToString());
            response.Write(")");
        }
    }
}
