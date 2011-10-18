namespace civilsalary.data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using System.Web;
    using System.Runtime.Serialization;

    //TODO: build in HttpContext caching?
    internal sealed class BlobProvider
    {
        static readonly CloudStorageAccount _account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
        static readonly TimeSpan _Timeout = TimeSpan.FromSeconds(30);
        static readonly RetryPolicy _RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

        const string _PathSeparator = "/";

        CloudBlobClient _client;
        CloudBlobContainer _container;
        string _containerName;
        object _lock = new object();

        internal BlobProvider(string containerName)
        {
            this._containerName = containerName;
            this._client = new CloudBlobClient(_account.BlobEndpoint.ToString(), _account.Credentials);
        }

        internal string ContainerUrl
        {
            get
            {
                return string.Join(_PathSeparator, new string[] { _client.BaseUri.AbsolutePath, _containerName });
            }
        }

        internal Uri GetUrl(string name)
        {
            CloudBlobContainer container = GetContainer();

            var reference = container.GetBlobReference(name);

            return reference.Uri;
        }

        internal bool GetBlobStringWithoutInitialization(string blobName, out string output, out BlobProperties properties)
        {
                CloudBlobContainer container = GetContainer();

                try
                {
                    var blob = container.GetBlobReference(blobName);
                    output = blob.DownloadText();

                    properties = blob.Properties;
                    Log.Write(EventKind.Information, "Getting contents of blob {0}", _client.BaseUri.ToString() + _PathSeparator + _containerName + _PathSeparator + blobName);
                    return true;
                }
                catch (StorageClientException ex)
                {
                    if (ex.ErrorCode == StorageErrorCode.BlobNotFound)
                    {
                        properties = null;
                        output = null;
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.InnerException is WebException)
                    {
                        var webEx = ex.InnerException as WebException;
                        var resp = webEx.Response as HttpWebResponse;

                        if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            properties = null;
                            output = null;
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            
        }

        //internal bool GetBlobStreamWithoutInitialization(string blobName, Stream outputStream, out BlobProperties properties)
        //{
        //    var profiler = MiniProfiler.Current;  //it's ok if this is null

        //    using (profiler.Step(string.Format("GetBlobStreamWithoutInitialization('{0}', '{1}')", _containerName, blobName)))
        //    {
        //        CloudBlobContainer container = GetContainer();

        //        try
        //        {
        //            var blob = container.GetBlobReference(blobName);
        //            blob.DownloadToStream(outputStream);

        //            properties = blob.Properties;
        //            Log.Write(EventKind.Information, "Getting contents of blob {0}", _client.BaseUri.ToString() + _PathSeparator + _containerName + _PathSeparator + blobName);
        //            return true;
        //        }
        //        catch (InvalidOperationException ex)
        //        {
        //            if (ex.InnerException is WebException)
        //            {
        //                var webEx = ex.InnerException as WebException;
        //                var resp = webEx.Response as HttpWebResponse;

        //                if (resp.StatusCode == HttpStatusCode.NotFound)
        //                {
        //                    properties = null;
        //                    return false;
        //                }
        //                else
        //                {
        //                    throw;
        //                }
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //}

        //internal BlobProperties GetBlobStream(string blobName, Stream outputStream)
        //{
        //    var profiler = MiniProfiler.Current;  //it's ok if this is null

        //    using (profiler.Step(string.Format("GetBlobStream('{0}', '{1}')", _containerName, blobName)))
        //    {

        //        if (string.IsNullOrEmpty(blobName))
        //        {
        //            throw new ArgumentNullException("blobName", "Session blob name is null or empty!");
        //        }

        //        if (outputStream == null)
        //        {
        //            throw new ArgumentNullException("outputStream", "Session blob output stream is null!");
        //        }

        //        BlobProperties properties;
        //        CloudBlobContainer container = GetContainer();
        //        try
        //        {
        //            var blob = container.GetBlobReference(blobName);

        //            blob.DownloadToStream(outputStream);

        //            properties = blob.Properties;
        //            Log.Write(EventKind.Information, "Getting contents of blob {0}", ContainerUrl + _PathSeparator + blobName);
        //            return properties;
        //        }
        //        catch (InvalidOperationException sc)
        //        {
        //            Log.Write(EventKind.Error, "Error getting contents of blob {0}: {1}", ContainerUrl + _PathSeparator + blobName, sc.Message);
        //            throw;
        //        }
        //    }
        //}

        internal void UploadString(string blobName, string contentType, string data)
        {
                CloudBlobContainer container = GetContainer();
                try
                {
                    Log.Write(EventKind.Information, "Uploading contents of blob string {0}", ContainerUrl + _PathSeparator + blobName);

                    var blob = container.GetBlockBlobReference(blobName);

                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        blob.Properties.ContentType = contentType;
                    }

                    blob.UploadText(data);
                }
                catch (InvalidOperationException se)
                {
                    Log.Write(EventKind.Error, "Error uploading blob string {0}: {1}", ContainerUrl + _PathSeparator + blobName, se.Message);
                    throw;
                }
            
        }


        internal void UploadStream(string blobName, string contentType, Stream output)
        {
                CloudBlobContainer container = GetContainer();
                try
                {
                    Log.Write(EventKind.Information, "Uploading contents of blob stream {0}", ContainerUrl + _PathSeparator + blobName);

                    var blob = container.GetBlockBlobReference(blobName);

                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        blob.Properties.ContentType = contentType;
                    }

                    blob.UploadFromStream(output);
                }
                catch (InvalidOperationException se)
                {
                    Log.Write(EventKind.Error, "Error uploading blob stream {0}: {1}", ContainerUrl + _PathSeparator + blobName, se.Message);
                    throw;
                }
            
        }

        //internal void DeleteBlob(string blobName)
        //{
        //    var profiler = MiniProfiler.Current;  //it's ok if this is null

        //    using (profiler.Step(string.Format("DeleteBlob('{0}', '{1}')", _containerName, blobName)))
        //    {
        //        CloudBlobContainer container = GetContainer();
        //        try
        //        {
        //            container.GetBlobReference(blobName).Delete();
        //        }
        //        catch (InvalidOperationException se)
        //        {
        //            Log.Write(EventKind.Error, "Error deleting blob {0}: {1}", ContainerUrl + _PathSeparator + blobName, se.Message);
        //            throw;
        //        }
        //    }
        //}

        //internal void DeleteBlobsWithPrefix(string prefix)
        //{
        //    var profiler = MiniProfiler.Current;  //it's ok if this is null

        //    using (profiler.Step(string.Format("DeleteBlobsWithPrefix('{0}', '{1}')", _containerName, prefix)))
        //    {
        //        var e = ListBlobs(prefix);
        //        if (e == null)
        //        {
        //            return;
        //        }
        //        var props = e.GetEnumerator();
        //        if (props == null)
        //        {
        //            return;
        //        }
        //        while (props.MoveNext())
        //        {
        //            if (props.Current != null)
        //            {
        //                DeleteBlob(props.Current.Uri.ToString());
        //            }
        //        }
        //    }
        //}

        //public IEnumerable<IListBlobItem> ListBlobs(string folder)
        //{
        //    var profiler = MiniProfiler.Current;  //it's ok if this is null

        //    using (profiler.Step(string.Format("ListBlobs('{0}', '{1}')", _containerName, folder)))
        //    {
        //        CloudBlobContainer container = GetContainer();
        //        try
        //        {
        //            return container.ListBlobs().Where((blob) => blob.Uri.PathAndQuery.StartsWith(folder));
        //        }
        //        catch (InvalidOperationException se)
        //        {
        //            Log.Write(EventKind.Error, "Error enumerating contents of folder {0} exists: {1}", ContainerUrl + _PathSeparator + folder, se.Message);
        //            throw;
        //        }
        //    }
        //}

        private CloudBlobContainer GetContainer()
        {
                // we have to make sure that only one thread tries to create the container
                lock (_lock)
                {
                    if (_container != null)
                    {
                        return _container;
                    }
                    try
                    {
                        var container = new CloudBlobContainer(_containerName, _client);
                        var requestModifiers = new BlobRequestOptions()
                        {
                            Timeout = _Timeout,
                            RetryPolicy = _RetryPolicy
                        };

                        container.CreateIfNotExist(requestModifiers);

                        _container = container;

                        return _container;
                    }
                    catch (InvalidOperationException se)
                    {
                        Log.Write(EventKind.Error, "Error creating container {0}: {1}", ContainerUrl, se.Message);
                        throw;
                    }
                }
            
        }

        internal static string BuildHttpContextKey(string container, string name)
        {
            return string.Format("blog_{0}_{1}", container, name);
        }

        internal static T LoadSerializedBlob<T>(string container, string name) where T : class
        {
            var httpKey = BuildHttpContextKey(container, name);

            var fromCtx = RequestCache.LoadValue<T>(httpKey);

            if (fromCtx != null) return fromCtx;

            //get from azure blob storage
            var blob = new BlobProvider(container);

            string xml;
            BlobProperties props;

            blob.GetBlobStringWithoutInitialization(name, out xml, out props);

            if (string.IsNullOrWhiteSpace(xml)) return null;

            var serializer = new DataContractSerializer(typeof(T));

            try
            {
                var deserialized = (T)serializer.Deserialize(xml);

                //reset context cache
                RequestCache.SaveValue(httpKey, deserialized);

                return deserialized;
            }
            catch (SerializationException)
            {
                //data in wrong format, so ignore it
                return null;
            }
        }

        internal static void SaveSerializedBlob<T>(string container, string name, T data)
        {
            var httpKey = BuildHttpContextKey(container, name);

            //push to context cache
            RequestCache.SaveValue(httpKey, data);
            
            var blob = new BlobProvider(container);
            var serializer = new DataContractSerializer(typeof(T));
            blob.UploadString(name, "text/xml", serializer.Serialize(data));
        }


        internal void SetPublic()
        {
            var container = GetContainer();

            container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
        }
    }

    public enum EventKind
    {
        Critical,
        Error,
        Warning,
        Information,
        Verbose
    }

    internal static class Log
    {
        internal static void Write(EventKind eventKind, string message, params object[] args)
        {
            switch (eventKind)
            {
                case EventKind.Error:
                case EventKind.Critical:
                    Trace.TraceError(message, args);
                    break;
                case EventKind.Warning:
                    Trace.TraceWarning(message, args);
                    break;
                case EventKind.Information:
                case EventKind.Verbose:
                    Trace.TraceInformation(message, args);
                    break;
            }
        }
    }
}